using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CityBuilding : MonoBehaviour
{
    [Header("Core Data")]
    public BuildingData data;
    public Sprite iconSprite;

    [Header("Optional Starting Buildings")]
    [SerializeField] private BuildingData[] defaultStartingBuildings;

    private bool isStartingBuilding = false;
    private CityID _city;
    private TextMeshProUGUI _levelLabel;


    // === Initialization ===

    public void Initialize(BuildingData buildingData, CityID city = null)
    {
        data = buildingData;
        _city = city ?? GetComponentInParent<CityID>();

        ApplyBuildingEffects();
        SetupVisuals();
        CreateLevelLabel();
    }

    public void InitializeAsStartingBuilding(BuildingType type)
    {
        foreach (var startingData in defaultStartingBuildings)
        {
            if (startingData.buildingType == type)
            {
                isStartingBuilding = true;
                Initialize(startingData);
                ApplyStartingBonuses();
                return;
            }
        }

        Debug.LogWarning($"Starting building data for {type} not found.");
    }


    private void Awake()
    {
        if (_city == null)
        {
            _city = GetComponentInParent<CityID>();
        }

        if (data != null)
        {
            ApplyBuildingEffects();
            SetupVisuals();
            CreateLevelLabel();
        }
    }

    private void SetupVisuals()
    {
        if (!TryGetComponent(out SpriteRenderer sr))
        {
            Debug.LogWarning("No SpriteRenderer found on building prefab.");
            return;
        }

        if (iconSprite == null && sr.sprite != null)
        {
            iconSprite = sr.sprite;
        }

        if (iconSprite != null)
        {
            sr.sprite = iconSprite;
            sr.enabled = true;
        }
        else
        {
            Debug.LogWarning($"No iconSprite set on {gameObject.name}");
        }

        sr.color = isStartingBuilding ? new Color(1f, 1f, 0.8f) : Color.white;
    }

    private TextMeshPro _levelLabel3D;


    private void CreateLevelLabel()
    {
        if (_levelLabel3D != null) return;

        // Safe collider fallback
        var colliders = GetComponentsInChildren<BoxCollider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning($"{name} has no BoxColliders for sizing label. Using default position.");
        }

        Bounds combinedBounds = new Bounds(transform.position, new Vector3(1f, 1f, 1f)); // default fallback
        if (colliders.Length > 0)
        {
            combinedBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                combinedBounds.Encapsulate(colliders[i].bounds);
        }

        float width = combinedBounds.size.x;
        float height = combinedBounds.size.y;

        GameObject labelObj = new GameObject("BuildingLabel3D");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0, height + 0.4f, 0);
        labelObj.transform.localScale = Vector3.one * 0.5f;

        var text = labelObj.AddComponent<TextMeshPro>();
        text.text = $"{data.buildingType}\nLv {data.level}";
        text.fontSize = 48;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8;
        text.fontSizeMax = 64;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.95f, 0.8f); // soft parchment
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
        text.overflowMode = TextOverflowModes.Overflow;
        text.enableWordWrapping = false;

        var rect = text.rectTransform;
        rect.sizeDelta = new Vector2(width * 10f, 100);
        rect.pivot = new Vector2(0.5f, 0.5f);

        var meshRenderer = text.GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "Buildings";
        meshRenderer.sortingOrder = 10;

        _levelLabel3D = text;
    }



    private void UpdateLevelLabel()
    {
        if (_levelLabel3D != null)
        {
            _levelLabel3D.text = $"{data.buildingType}\nLv {data.level}";
        }
    }

    private void ApplyStartingBonuses()
    {
        if (!isStartingBuilding || data == null || _city?.resources == null) return;

        switch (data.buildingType)
        {
            case BuildingType.Headquarters:
                data.level = Mathf.Min(data.level + 1, data.maxLevel);
                _city.resources.populationCapacity += 20;
                _city.resources.silver += 100;
                break;

            case BuildingType.Warehouse:
                _city.resources.populationCapacity += 10;
                break;
        }
    }

    private void ApplyBuildingEffects()
    {
        if (data == null || _city?.resources == null) return;

        switch (data.buildingType)
        {
            case BuildingType.Warehouse:
                _city.resources.populationCapacity += 5 * data.level;
                break;

            case BuildingType.Headquarters:
                if (isStartingBuilding)
                {
                    _city.resources.populationCapacity += 20;
                    _city.resources.silver += 100;
                }
                break;
        }
    }

    private void ApplyUpgradeEffects()
    {
        if (data == null || _city?.resources == null) return;

        switch (data.buildingType)
        {
            case BuildingType.Warehouse:
                _city.resources.populationCapacity += 5;
                break;

            case BuildingType.Headquarters:
                if (isStartingBuilding && data.level == 2)
                {
                    _city.resources.populationCapacity += 10;
                }
                break;
        }
    }


    public void Upgrade()
    {
        if (!CanUpgrade) return;

        data.level++;
        ApplyUpgradeEffects();
        _city.resources.DeductResources(data.upgradeCosts);
        UpdateLevelLabel(); 
    }
    private void OnMouseDown()
    {
        var city = GetComponentInParent<CityID>();
        if (city != null)
        {
            Debug.Log($"[CityBuilding] Clicked on building: {data.buildingType}");

            if (BuildingUIManager.Instance != null)
            {
                BuildingUIManager.Instance.SetActiveCity(city);
                Debug.Log($"[CityBuilding] SetActiveCity called for: {city.cityName}");

                BuildingUIManager.Instance.ShowUIFor(data.buildingType);
            }
            else
            {
                Debug.LogWarning("[CityBuilding] BuildingUIManager.Instance is null");
            }
        }
        else
        {
            Debug.LogWarning("[CityBuilding] Could not find CityID on parent");
        }
    }




    public bool HasResourcesForUpgrade() => _city?.resources?.CanAfford(data.upgradeCosts) == true;

    // === Properties ===

    public string BuildingName => isStartingBuilding
        ? $"{data.buildingType} (Capital)"
        : data.buildingType.ToString();

    public int CurrentLevel => data?.level ?? 0;
    public bool IsAtMaxLevel => data != null && data.level >= data.maxLevel;
    public bool CanUpgrade => !IsAtMaxLevel && HasResourcesForUpgrade();
    public ResourceCost[] NextUpgradeCost => data?.upgradeCosts;
    public Sprite IconSprite => iconSprite;
    public bool IsStartingBuilding => isStartingBuilding;
    public BuildingData GetData() => data;
}
