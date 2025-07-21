using System.Collections.Generic;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    public static CityManager Instance { get; private set; }

    private int nextCityID = 1;
    private readonly Dictionary<int, CityID> cityRegistry = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetNextCityID()
    {
        return nextCityID++;
    }

    public void RegisterCity(CityID city)
    {
        if (city != null && !cityRegistry.ContainsKey(city.cityID))
        {
            cityRegistry.Add(city.cityID, city);
        }
    }

    public CityID GetCityByID(int id)
    {
        return cityRegistry.TryGetValue(id, out var city) ? city : null;
    }

    public List<CityID> GetAllCities()
    {
        return new List<CityID>(cityRegistry.Values);
    }
}
