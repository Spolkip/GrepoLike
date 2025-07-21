using UnityEngine;

[System.Serializable]
public class CityResources : MonoBehaviour
{
    public int wood;
    public int stone;
    public int silver;
    public int population;
    public int populationCapacity;

    public void Initialize()
    {
        wood = 100;
        stone = 100;
        silver = 50;
        population = 10;
        populationCapacity = 20;
    }

    public bool CanAfford(ResourceCost[] costs)
    {
        foreach (var cost in costs)
        {
            switch (cost.resourceType)
            {
                case ResourceType.Wood when wood < cost.amount:
                case ResourceType.Stone when stone < cost.amount:
                case ResourceType.Silver when silver < cost.amount:
                case ResourceType.Population when population < cost.amount:
                    return false;
            }
        }
        return true;
    }

    public void DeductResources(ResourceCost[] costs)
    {
        foreach (var cost in costs)
        {
            switch (cost.resourceType)
            {
                case ResourceType.Wood: wood -= cost.amount; break;
                case ResourceType.Stone: stone -= cost.amount; break;
                case ResourceType.Silver: silver -= cost.amount; break;
                case ResourceType.Population: population -= cost.amount; break;
            }
        }
    }

    public void AddResources(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: wood += amount; break;
            case ResourceType.Stone: stone += amount; break;
            case ResourceType.Silver: silver += amount; break;
            case ResourceType.Population:
                population = Mathf.Min(population + amount, populationCapacity);
                break;
        }
    }
}