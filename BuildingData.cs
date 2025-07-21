using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public BuildingType buildingType;
    public int level = 1;
    public int maxLevel = 5;
    public ResourceCost[] constructionCosts;
    public ResourceCost[] upgradeCosts;
    public BuildingEffect[] effects;

    [System.Serializable]
    public class BuildingEffect
    {
        public ResourceType affectedResource;
        public int amount;
        public bool isCapacityEffect;
        public bool isProductionEffect;
    }
}

public enum BuildingType
{
    Headquarters,
    Barracks,
    Warehouse,
    Temple,
    Market,
    Wall
}

[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int amount;
}

public enum ResourceType
{
    Wood,
    Stone,
    Silver,
    Population
}