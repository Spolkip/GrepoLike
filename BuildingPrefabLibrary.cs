using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingPrefabLibrary", menuName = "City/Building Prefab Library")]
public class BuildingPrefabLibrary : ScriptableObject
{
    [System.Serializable]
    public class BuildingMapping
    {
        public BuildingType type;
        public GameObject prefab;
    }

    public List<BuildingMapping> buildingMappings;

    private Dictionary<BuildingType, GameObject> _cache;

    public GameObject GetPrefab(BuildingType type)
    {
        if (_cache == null)
        {
            _cache = new Dictionary<BuildingType, GameObject>();
            foreach (var map in buildingMappings)
            {
                _cache[map.type] = map.prefab;
            }
        }

        return _cache.TryGetValue(type, out var prefab) ? prefab : null;
    }
}
