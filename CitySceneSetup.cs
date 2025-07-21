using UnityEngine;
using System.Collections;
using System;

public class CitySceneSetup : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer cityBackground;
    public Transform buildingsContainer;
    public Camera cityCamera;

    [Header("Settings")]
    public float cameraPadding = 2f;

    // Event to notify when city initialization is complete
    public static event Action<CityID> OnCitySetupComplete;
    public static event Action<Exception> OnSetupFailed;

    private CityID _runtimeCity;

    public void Initialize(CityID sourceCity)
    {
        StartCoroutine(SetupCityAsync(sourceCity));
    }

    private IEnumerator SetupCityAsync(CityID sourceCity)
    {
        if (sourceCity == null)
        {
            sourceCity = CityViewManager.SelectedCity;
            if (sourceCity == null)
            {
                var error = new Exception("[CitySceneSetup] No CityID available to initialize.");
                OnSetupFailed?.Invoke(error);
                yield break;
            }
        }

        try
        {
            Debug.Log($"[CitySceneSetup] Setting up city: {sourceCity.cityName}");

            // === Create runtime City GameObject ===
            GameObject cityGO = new GameObject(sourceCity.cityName);
            cityGO.transform.position = Vector3.zero;

            _runtimeCity = cityGO.AddComponent<CityID>();
            _runtimeCity.parentSpotID = sourceCity.parentSpotID;
            _runtimeCity.parentIslandID = sourceCity.parentIslandID;

            // Initialize resources before anything else
            var newResources = cityGO.AddComponent<CityResources>();
            newResources.Initialize();
            _runtimeCity.resources = newResources;

            // Copy essential data
            _runtimeCity.defaultBuildings = sourceCity.defaultBuildings;
            _runtimeCity.prefabLibrary = sourceCity.prefabLibrary;
            _runtimeCity.buildingsContainer = buildingsContainer;

            // Fully initialize city
            _runtimeCity.Initialize(sourceCity.cityID, sourceCity.parentSpotID, sourceCity.parentIslandID);

            // === Reparent and reinitialize buildings ===
            foreach (var building in sourceCity.GetBuildings())
            {
                if (building == null) continue;

                building.transform.SetParent(buildingsContainer, true);
                building.Initialize(building.GetData(), _runtimeCity);
            }

            // === Camera Setup ===
            if (cityCamera != null)
            {
                CityViewManager.Instance?.SetCityCamera(cityCamera);
                PositionCamera();
            }

            // Notify completion
            OnCitySetupComplete?.Invoke(_runtimeCity);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CitySceneSetup] Setup failed: {ex}");
            OnSetupFailed?.Invoke(ex);
        }
    }

    private void PositionCamera()
    {
        if (cityCamera == null) return;

        cityCamera.transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            cityCamera.transform.position.z
        );

        if (cityCamera.orthographic)
        {
            Bounds bounds = CalculateCityBounds();
            float requiredSize = Mathf.Max(
                bounds.size.x * Screen.height / Screen.width * 0.5f,
                bounds.size.y * 0.5f
            ) + cameraPadding;
            cityCamera.orthographicSize = Mathf.Clamp(requiredSize, 5f, 15f);
        }
    }

    private Bounds CalculateCityBounds()
    {
        var bounds = new Bounds(transform.position, Vector3.zero);
        if (cityBackground != null) bounds.Encapsulate(cityBackground.bounds);
        if (buildingsContainer != null)
        {
            foreach (var renderer in buildingsContainer.GetComponentsInChildren<Renderer>())
                bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    private void OnDestroy()
    {
        // Clean up events
        OnCitySetupComplete = null;
        OnSetupFailed = null;
    }
}