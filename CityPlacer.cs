using UnityEngine;
using UnityEngine.EventSystems;

public class CityPlacer : MonoBehaviour
{
    [Header("City Settings")]
    public GameObject cityPrefab;
    public LayerMask spotLayer;

    private Camera mainCamera;
    private EventSystem eventSystem;
    private int currentCityID = 0;

    void Awake()
    {
        mainCamera = Camera.main;
        eventSystem = FindObjectOfType<EventSystem>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            TryPlaceCity();
        }
    }

    private int GetNextCityID()
    {
        return ++currentCityID;
    }

    void TryPlaceCity()
    {
        // Skip if clicking on UI
        if (eventSystem != null && eventSystem.IsPointerOverGameObject())
            return;

        // Early return if we can't get camera reference
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera reference missing!");
            return;
        }

        // Convert mouse position to world coordinates
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Check for existing city first
        Collider2D cityHit = Physics2D.OverlapPoint(mouseWorldPos);
        if (cityHit != null)
        {
            CityID existingCity = cityHit.GetComponent<CityID>();
            if (existingCity != null && CityViewManager.Instance != null)
            {
                CityViewManager.Instance.EnterCity(existingCity);
                return;
            }

        }

        // If no city was clicked, check for empty spots
        Collider2D[] spotHits = Physics2D.OverlapPointAll(mouseWorldPos, spotLayer);
        foreach (Collider2D hit in spotHits)
        {
            if (hit == null) continue;

            SpotID spot = hit.GetComponent<SpotID>();
            if (spot != null && !spot.hasCity)
            {
                if (CanPlaceCityAtSpot(spot))
                {
                    PlaceCity(hit.gameObject, spot);
                    return; // Only place one city per click
                }
            }
        }
    }

    bool CanPlaceCityAtSpot(SpotID spot)
    {
        // Add resource checks or other validation here if needed
        return true;
    }

    void PlaceCity(GameObject spotObject, SpotID spotID)
    {
        try
        {
            if (cityPrefab == null)
            {
                Debug.LogError("City prefab is not assigned!");
                return;
            }

            Transform spotTransform = spotObject.transform;
            Vector3 spawnPosition = spotTransform.position;
            Transform parentTransform = spotTransform.parent;

            GameObject newCity = Instantiate(
                cityPrefab,
                spawnPosition,
                Quaternion.identity,
                parentTransform
            );

            newCity.transform.localScale = spotTransform.localScale;
            newCity.transform.position = new Vector3(
                spawnPosition.x,
                spawnPosition.y,
                spawnPosition.z - 0.1f
            );

            CityID cityID = newCity.GetComponent<CityID>();
            if (cityID == null) cityID = newCity.AddComponent<CityID>();
            cityID.Initialize(GetNextCityID(), spotID.spotID, spotID.parentIslandID);

            spotID.hasCity = true;
            spotID.gameObject.SetActive(false);

            newCity.name = $"City_{spotID.parentIslandID}_{spotID.spotID}";
            Debug.Log($"City placed at Island {spotID.parentIslandID}, Spot {spotID.spotID}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to place city: {e.Message}");
        }
    }
}