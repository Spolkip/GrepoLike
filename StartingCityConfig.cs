// StartingCityConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "StartingCityConfig", menuName = "City/Starting City Config")]
public class StartingCityConfig : ScriptableObject
{
    [System.Serializable]
    public class StartingBuilding
    {
        public BuildingType buildingType;
        public BuildingData buildingData;
        public Vector2 positionInCity;
    }

    public string cityName = "Your Capital";
    public StartingBuilding[] startingBuildings;

    public BuildingData GetBuildingData(BuildingType type)
    {
        foreach (var building in startingBuildings)
        {
            if (building.buildingType == type)
            {
                return building.buildingData;
            }
        }
        return null;
    }
}