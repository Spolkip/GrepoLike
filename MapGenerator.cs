using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public int islandCount = 20;
    public float minIslandSize = 3f;
    public float maxIslandSize = 8f;
    public float islandGap = 2f;

    [Header("Spot Settings")]
    public GameObject spotPrefab;
    public int minSpots = 8;
    public int maxSpots = 20;
    public float spotDotWorldSize = 0.08f;
    public float spotSpacingWorld = 1f;
    public float spotPlacementOffset = 0.05f;

    [Header("Prefabs")]
    public GameObject waterPrefab;
    public GameObject islandPrefab;

    [Header("Camera Reference")]
    public CameraController cameraController;

    private readonly List<Vector3> islandPositions = new List<Vector3>();
    private readonly List<float> islandRadii = new List<float>();
    private readonly List<GameObject> islands = new List<GameObject>();
    private int currentIslandID = 0;
    private Dictionary<int, int> islandSpotCounters = new Dictionary<int, int>();

    void Start()
    {
        GenerateMap();
        InitializeCamera();
    }

    void GenerateMap()
    {
        ClearPreviousMap();
        CreateWaterBackground();
        GenerateIslands();
        GenerateSpots();
    }

    void ClearPreviousMap()
    {
        islandPositions.Clear();
        islandRadii.Clear();

        foreach (var island in islands)
        {
            if (island != null) Destroy(island);
        }
        islands.Clear();
    }

    void InitializeCamera()
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }

        if (cameraController != null)
        {
            cameraController.InitializeCameraBounds(mapWidth, mapHeight);
        }
    }

    void CreateWaterBackground()
    {
        if (waterPrefab == null) return;

        GameObject water = Instantiate(waterPrefab, Vector3.zero, Quaternion.identity);
        water.transform.localScale = new Vector3(mapWidth, mapHeight, 1f);
        water.name = "WaterBackground";
    }

    void GenerateIslands()
    {
        if (islandPrefab == null) return;

        for (int i = 0; i < islandCount; i++)
        {
            float size = Random.Range(minIslandSize, maxIslandSize);
            float radius = size * 0.5f;

            Vector3 position;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                position = GetRandomPosition();
                attempts++;
            }
            while (!IsPositionValid(position, radius) && attempts < maxAttempts);

            if (attempts >= maxAttempts) continue;

            GameObject island = CreateIsland(position, size, i);
            islandPositions.Add(position);
            islandRadii.Add(radius);
            islands.Add(island);
        }
    }

    GameObject CreateIsland(Vector3 position, float size, int index)
    {
        GameObject island = Instantiate(islandPrefab, position, Quaternion.identity);
        island.transform.localScale = Vector3.one * size;
        island.transform.position = new Vector3(position.x, position.y, -1);

        // Add and initialize IslandID component
        var islandIDComponent = island.AddComponent<IslandID>();
        islandIDComponent.Initialize(currentIslandID);
        islandSpotCounters.Add(currentIslandID, 0);

        currentIslandID++;
        return island;
    }


    void GenerateSpots()
    {
        if (spotPrefab == null) return;

        foreach (GameObject island in islands)
        {
            if (island == null) continue;

            float perimeter = GetColliderPerimeter(island);
            int spotCount = CalculateSpotCount(island);


            Vector3[] edgePoints = GetEdgePoints(island, spotCount);
            CreateSpotsOnIsland(island, edgePoints);
        }
    }

    int CalculateSpotCount(GameObject island)
    {
        float scale = island.transform.localScale.x; // assuming uniform scale for X and Y
        float normalized = Mathf.InverseLerp(minIslandSize, maxIslandSize, scale);
        int spotCount = Mathf.RoundToInt(Mathf.Lerp(minSpots, maxSpots, normalized));
        return spotCount;
    }


    void CreateSpotsOnIsland(GameObject island, Vector3[] points)
    {
        var islandID = island.GetComponent<IslandID>();
        if (islandID == null) return;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 point = points[i];
            GameObject spot = Instantiate(spotPrefab, point, Quaternion.identity, island.transform);
            spot.transform.localScale = Vector3.one * spotDotWorldSize;
            spot.transform.position = new Vector3(point.x, point.y, -2);

            // Add and initialize SpotID component
            var spotID = spot.AddComponent<SpotID>();
            spotID.Initialize(islandSpotCounters[islandID.islandID], islandID.islandID);
            islandSpotCounters[islandID.islandID]++;
        }
    }

    Vector3[] GetEdgePoints(GameObject island, int count)
    {
        if (count <= 0) return new Vector3[0];

        PolygonCollider2D poly = island.GetComponent<PolygonCollider2D>();
        return poly != null ?
            GetPolygonEdgePoints(poly, count) :
            GetSquarePerimeterPoints(island, count);
    }

    Vector3[] GetPolygonEdgePoints(PolygonCollider2D poly, int count)
    {
        Vector2[] verts = poly.points;
        int vertexCount = verts.Length;
        if (vertexCount == 0) return new Vector3[0];

        float[] segmentLengths = new float[vertexCount];
        float totalPerimeter = 0f;

        for (int i = 0; i < vertexCount; i++)
        {
            float distance = Vector2.Distance(verts[i], verts[(i + 1) % vertexCount]);
            segmentLengths[i] = distance;
            totalPerimeter += distance;
        }

        float stepSize = totalPerimeter / count;
        List<Vector3> points = new List<Vector3>();
        float accumulatedDistance = 0f;
        int currentSegment = 0;

        for (int pointIndex = 0; pointIndex < count; pointIndex++)
        {
            float targetDistance = pointIndex * stepSize;

            while (accumulatedDistance + segmentLengths[currentSegment] < targetDistance &&
                   pointIndex < count - 1)
            {
                accumulatedDistance += segmentLengths[currentSegment];
                currentSegment = (currentSegment + 1) % vertexCount;
            }

            float segmentProgress = (targetDistance - accumulatedDistance) / segmentLengths[currentSegment];
            Vector2 localPoint = Vector2.Lerp(
                verts[currentSegment],
                verts[(currentSegment + 1) % vertexCount],
                segmentProgress);

            Vector3 worldPoint = poly.transform.TransformPoint(localPoint);
            Vector3 direction = (worldPoint - poly.transform.position).normalized;
            points.Add(worldPoint + direction * spotPlacementOffset);
        }

        return points.ToArray();
    }

    Vector3[] GetSquarePerimeterPoints(GameObject island, int count)
    {
        if (count <= 0) return new Vector3[0];

        SpriteRenderer renderer = island.GetComponent<SpriteRenderer>();
        float halfWidth = renderer != null ?
            renderer.bounds.extents.x :
            island.transform.localScale.x * 0.5f;
        float halfHeight = renderer != null ?
            renderer.bounds.extents.y :
            island.transform.localScale.y * 0.5f;

        float width = halfWidth * 2f;
        float height = halfHeight * 2f;
        float perimeter = 2f * (width + height);
        float stepSize = perimeter / count;

        List<Vector3> points = new List<Vector3>();
        Vector3 center = island.transform.position;

        for (int i = 0; i < count; i++)
        {
            float distance = i * stepSize;
            Vector3 point;

            if (distance < width)
            {
                point = new Vector3(
                    center.x - halfWidth + distance,
                    center.y - halfHeight,
                    0);
            }
            else if (distance < width + height)
            {
                point = new Vector3(
                    center.x + halfWidth,
                    center.y - halfHeight + (distance - width),
                    0);
            }
            else if (distance < 2f * width + height)
            {
                point = new Vector3(
                    center.x + halfWidth - (distance - width - height),
                    center.y + halfHeight,
                    0);
            }
            else
            {
                point = new Vector3(
                    center.x - halfWidth,
                    center.y + halfHeight - (distance - 2f * width - height),
                    0);
            }

            Vector3 direction = (point - center).normalized;
            points.Add(point + direction * spotPlacementOffset);
        }

        return points.ToArray();
    }

    float GetColliderPerimeter(GameObject island)
    {
        PolygonCollider2D poly = island.GetComponent<PolygonCollider2D>();
        if (poly != null)
        {
            Vector2[] vertices = poly.points;
            if (vertices.Length == 0) return 0f;

            float perimeter = 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                perimeter += Vector2.Distance(vertices[i], vertices[(i + 1) % vertices.Length]);
            }
            return perimeter;
        }

        SpriteRenderer renderer = island.GetComponent<SpriteRenderer>();
        float halfWidth = renderer != null ?
            renderer.bounds.extents.x :
            island.transform.localScale.x * 0.5f;
        float halfHeight = renderer != null ?
            renderer.bounds.extents.y :
            island.transform.localScale.y * 0.5f;

        return 4f * (halfWidth + halfHeight);
    }

    Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-mapWidth * 0.5f, mapWidth * 0.5f),
            Random.Range(-mapHeight * 0.5f, mapHeight * 0.5f),
            0f
        );
    }

    bool IsPositionValid(Vector3 position, float radius)
    {
        // Check against other islands
        for (int i = 0; i < islandPositions.Count; i++)
        {
            float minDistance = radius + islandRadii[i] + islandGap;
            if (Vector3.Distance(position, islandPositions[i]) < minDistance)
            {
                return false;
            }
        }

        // Check against map boundaries
        float borderX = mapWidth * 0.5f - radius;
        float borderY = mapHeight * 0.5f - radius;

        return Mathf.Abs(position.x) <= borderX &&
               Mathf.Abs(position.y) <= borderY;
    }
}