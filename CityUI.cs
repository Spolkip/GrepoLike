using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text cityNameText;
    public Button returnToMapButton;
    public ResourceDisplay resourceDisplay;

    private CityID currentCity;

    void Start()
    {
        if (returnToMapButton != null)
        {
            returnToMapButton.onClick.AddListener(() =>
            {
                CityViewManager.Instance?.ExitCity();
            });
        }
    }

    public void Initialize(CityID city)
    {
        if (city == null)
        {
            Debug.LogError("[CityUI] Initialize failed: city is null.");
            return;
        }

        currentCity = city;

        if (cityNameText != null)
        {
            cityNameText.text = city.cityName;
        }

        UpdateResourceDisplay();
        gameObject.SetActive(true);

        Debug.Log($"[CityUI] Initialized for city: {city.cityName}");
    }

    void UpdateResourceDisplay()
    {
        if (resourceDisplay != null && currentCity?.resources != null)
        {
            resourceDisplay.UpdateResources(currentCity.resources);
        }
    }
}
