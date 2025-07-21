// IslandID.cs
using UnityEngine;

public class IslandID : MonoBehaviour
{
    public int islandID;
    public string islandName;

    public void Initialize(int id)
    {
        islandID = id;
        islandName = $"Island_{id}";
        gameObject.name = islandName;
    }
}