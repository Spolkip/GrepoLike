using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{
    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text silverText;
    public TMP_Text populationText;
    public TMP_Text populationCapacityText;

    public void UpdateResources(CityResources resources)
    {
        woodText.text = resources.wood.ToString();
        stoneText.text = resources.stone.ToString();
        silverText.text = resources.silver.ToString();
        populationText.text = resources.population.ToString();
        populationCapacityText.text = $"{resources.population}/{resources.populationCapacity}";
    }
}