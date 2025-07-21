using UnityEngine;

public class BuildingUIManager : MonoBehaviour
{
    public GameObject barracksUI;
    public GameObject warehouseUI;
    public GameObject headquartersUI;

    public static BuildingUIManager Instance;
    private CityID currentCity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HideAll();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAll();
        }
    }

    public void SetActiveCity(CityID city)
    {
        currentCity = city;
        Debug.Log("[BuildingUIManager] Active city set to: " + city?.cityName);
    }

    public void ShowUIFor(BuildingType type)
    {
        HideAll();

        if (currentCity == null)
        {
            currentCity = CityViewManager.Instance?.CurrentCity;
            Debug.LogWarning("[BuildingUIManager] currentCity was null. Retrieved from CityViewManager: " + currentCity);
        }

        Debug.Log("[BuildingUIManager] Showing UI for: " + type);

        switch (type)
        {
            case BuildingType.Barracks:
                if (barracksUI) barracksUI.SetActive(true);
                break;

            case BuildingType.Warehouse:
                if (warehouseUI)
                {
                    warehouseUI.SetActive(true);

                    if (warehouseUI.TryGetComponent(out WarehousePanelUI panel))
                    {
                        var res = currentCity?.resources;
                        Debug.Log("[BuildingUIManager] Passing resources to warehouse panel: " + res);
                        panel.Show(res);
                    }
                    else
                    {
                        Debug.LogError("[BuildingUIManager] WarehousePanelUI not found on warehouseUI object.");
                    }
                }
                break;

            case BuildingType.Headquarters:
                if (headquartersUI) headquartersUI.SetActive(true);
                break;

            default:
                Debug.LogWarning("[BuildingUIManager] No UI defined for building type: " + type);
                break;
        }
    }

    public void HideAll()
    {
        if (barracksUI) barracksUI.SetActive(false);
        if (warehouseUI) warehouseUI.SetActive(false);
        if (headquartersUI) headquartersUI.SetActive(false);
    }
}
