using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildingButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image buildingIcon;
    [SerializeField] private Text buildingNameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text upgradeCostText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject upgradeAvailableIndicator;
    [SerializeField] private GameObject maxLevelIndicator;

    [Header("Settings")]
    [SerializeField] private Color affordableColor = Color.green;
    [SerializeField] private Color unaffordableColor = Color.red;

    private CityBuilding _building;
    private System.Action<CityBuilding> _onUpgradeCallback;

    public void Initialize(CityBuilding cityBuilding, System.Action<CityBuilding> onUpgradeCallback)
    {
        _building = cityBuilding;
        _onUpgradeCallback = onUpgradeCallback;

        UpdateVisuals(_building.CurrentLevel);  // Pass the current level here
        upgradeButton.onClick.AddListener(HandleUpgradeClicked);
    }

    public void UpdateVisuals(int level)
    {
        if (_building == null) return;

        buildingNameText.text = _building.BuildingName;
        levelText.text = $"Level {level}";

        if (_building.IconSprite != null)
        {
            buildingIcon.sprite = _building.IconSprite;
            buildingIcon.gameObject.SetActive(true);
        }

        if (_building.IsAtMaxLevel)
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "MAX";
            maxLevelIndicator.SetActive(true);
            upgradeAvailableIndicator.SetActive(false);
        }
        else
        {
            maxLevelIndicator.SetActive(false);
            upgradeAvailableIndicator.SetActive(_building.CanUpgrade);

            var costs = _building.NextUpgradeCost;
            upgradeCostText.text = FormatCost(costs);
            upgradeCostText.color = _building.HasResourcesForUpgrade() ? affordableColor : unaffordableColor;
            upgradeButton.interactable = _building.CanUpgrade;
        }
    }

    private string FormatCost(ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0) return "Free";

        string result = "";
        foreach (var cost in costs)
        {
            result += $"{cost.resourceType}:{cost.amount}\n";
        }
        return result.Trim();
    }

    private void HandleUpgradeClicked()
    {
        if (_building == null || !_building.CanUpgrade) return;

        _building.Upgrade();
        UpdateVisuals(_building.CurrentLevel);
        _onUpgradeCallback?.Invoke(_building);
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
        }
    }
}