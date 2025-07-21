using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarehousePanelUI : MonoBehaviour
{
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI silverText;
    public TextMeshProUGUI populationText;
    public Button backButton; // <== Add this

    private CityResources currentResources;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false); // Just hides the warehouse UI
            });
        }
    }

    public void Show(CityResources resources)
    {
        if (resources == null)
        {
            Debug.LogError("[WarehousePanelUI] CityResources is null. Cannot show UI.");
            return;
        }

        currentResources = resources;
        UpdateUI();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (currentResources == null)
        {
            Debug.LogError("[WarehousePanelUI] Cannot update UI: currentResources is null.");
            return;
        }

        woodText.text = "Wood: " + currentResources.wood;
        stoneText.text = "Stone: " + currentResources.stone;
        silverText.text = "Silver: " + currentResources.silver;
        populationText.text = $"Pop: {currentResources.population} / {currentResources.populationCapacity}";
    }
}
