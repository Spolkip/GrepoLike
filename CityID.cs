using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System;

[RequireComponent(typeof(CityResources))]
public class CityID : MonoBehaviour
{
    [Header("City Identity")]
    public int cityID;
    public int parentSpotID;
    public int parentIslandID;
    public string cityName;

    [Header("Setup References")]
    public Transform buildingsContainer;
    public BuildingPrefabLibrary prefabLibrary;

    [Header("Building Configuration")]
    public List<BuildingData> defaultBuildings = new();

    private List<CityBuilding> _buildings = new();
    public CityResources resources { get; set; }
    private bool initialized = false;

    private void Awake()
    {
        // Ensure CityResources is present and initialized
        if (resources == null)
        {
            resources = GetComponent<CityResources>() ?? gameObject.AddComponent<CityResources>();
            resources.Initialize();
        }

        // Auto-generate if not already initialized
        if (!initialized)
        {
            if (cityID == 0 && CityManager.Instance != null)
            {
                cityID = CityManager.Instance.GetNextCityID();
                cityName = GenerateCityName();
                gameObject.name = cityName;

                Debug.Log($"[CityID] Auto-assigned: {cityName}");
            }

            CityManager.Instance?.RegisterCity(this);
            initialized = true;
        }
    }

    public void Initialize(int id, int spotId, int islandId)
    {
        cityID = id;
        parentSpotID = spotId;
        parentIslandID = islandId;
        cityName = GenerateCityName();
        gameObject.name = cityName;

        InitializeResources();
        SpawnDefaultBuildings();

        CityManager.Instance?.RegisterCity(this);
        initialized = true;

        Debug.Log($"[CityID] City '{cityName}' initialized.");
    }

    private string GenerateCityName()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        return $"{playerName} City #{cityID}";
    }

    private void InitializeResources()
    {
        resources = GetComponent<CityResources>() ?? gameObject.AddComponent<CityResources>();
        resources.Initialize();
    }

    private void SpawnDefaultBuildings()
    {
        foreach (var existing in GetComponentsInChildren<CityBuilding>())
        {
            Destroy(existing.gameObject);
        }
        _buildings.Clear();

        foreach (var data in defaultBuildings)
        {
            CreateBuilding(data);
        }
    }

    public CityBuilding CreateBuilding(BuildingData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[CityID] Tried to create a building with null data.");
            return null;
        }

        GameObject prefab = prefabLibrary?.GetPrefab(data.buildingType);
        Transform parent = buildingsContainer != null ? buildingsContainer : transform;

        GameObject buildingObj = prefab != null
            ? Instantiate(prefab, parent)
            : CreateFallbackBuilding(data.buildingType, parent);

        buildingObj.transform.localPosition = GetBuildingPosition(data.buildingType);
        buildingObj.transform.localScale = Vector3.one * 2f;

        CityBuilding building = buildingObj.GetComponent<CityBuilding>() ??
                                buildingObj.AddComponent<CityBuilding>();

        building.Initialize(data, this);

        AddFloatingLevelLabel(buildingObj, data.level);

        _buildings.Add(building);
        return building;
    }

    private GameObject CreateFallbackBuilding(BuildingType type, Transform parent)
    {
        Debug.LogWarning($"[CityID] No prefab for {type}. Creating fallback.");
        var go = new GameObject(type.ToString());
        go.transform.SetParent(parent);
        go.AddComponent<SpriteRenderer>();
        return go;
    }

    private void AddFloatingLevelLabel(GameObject buildingObj, int level)
    {
        GameObject label = new GameObject("LevelText");
        label.transform.SetParent(buildingObj.transform);
        label.transform.localPosition = new Vector3(0, 1.2f, 0);

        var text = label.AddComponent<TextMeshPro>();
        text.text = $"Lv {level}";
        text.fontSize = 4;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.sortingOrder = 10;
    }

    private Vector3 GetBuildingPosition(BuildingType type)
    {
        return type switch
        {
            BuildingType.Headquarters => new Vector3(0, 2),
            BuildingType.Barracks => new Vector3(-3, 0),
            BuildingType.Warehouse => new Vector3(3, 0),
            BuildingType.Market => new Vector3(-2, -2),
            BuildingType.Temple => new Vector3(2, -2),
            BuildingType.Wall => new Vector3(0, -3),
            _ => Vector3.zero,
        };
    }

    private void OnMouseDown()
    {
        if (!initialized)
        {
            Debug.LogWarning($"[CityID] '{gameObject.name}' clicked before initialization.");
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (CityViewManager.Instance == null)
        {
            Debug.LogError("[CityID] CityViewManager.Instance is null.");
            return;
        }

        CityViewManager.Instance.EnterCity(this);
    }

    public List<CityBuilding> GetBuildings() => _buildings;
    public CityBuilding GetBuilding(BuildingType type) => _buildings.Find(b => b.data.buildingType == type);

    public void SetVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = visible;
    }
}
