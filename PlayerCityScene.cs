using UnityEngine;

public class PlayerCitySceneSetup : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer cityBackground;
    public Transform buildingsContainer;
    public CityUI cityUI;

    [Header("Settings")]
    public Color playerCityColor = new Color(0.1f, 0.5f, 0.8f);

    private CityID playerCity;

    // Add this method to fix the error
    public void InitializeWithConfig(StartingCityConfig config)
    {
        if (config == null)
        {
            Debug.LogError("StartingCityConfig is null!");
            return;
        }

        // Clear existing city if any
        if (playerCity != null)
        {
            Destroy(playerCity.gameObject);
        }

        // Create new city
        CreatePlayerCity(config);
        SetupScene();
    }

    void CreatePlayerCity(StartingCityConfig config)
    {
        GameObject cityObj = new GameObject(config.cityName);
        playerCity = cityObj.AddComponent<CityID>();
        playerCity.Initialize(0, 0, 0);
        playerCity.cityName = config.cityName;

        // Create starting buildings
        foreach (var buildingConfig in config.startingBuildings)
        {
            if (buildingConfig.buildingData != null)
            {
                var building = playerCity.CreateBuilding(buildingConfig.buildingData);
                building.InitializeAsStartingBuilding(buildingConfig.buildingType);

                // Position the building
                building.transform.localPosition = buildingConfig.positionInCity;
            }
        }
    }

    void SetupScene()
    {
        // Setup background
        if (cityBackground != null)
        {
            cityBackground.color = playerCityColor;
            cityBackground.sortingOrder = -10;
        }

        // Initialize UI
        if (cityUI != null && playerCity != null)
        {
            cityUI.Initialize(playerCity);
        }

        // Position camera
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
        }
    }
}