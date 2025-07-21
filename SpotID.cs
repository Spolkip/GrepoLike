// SpotID.cs
using UnityEngine;

public class SpotID : MonoBehaviour
{
    public int spotID;
    public int parentIslandID;
    public string spotName;
    public bool hasCity = false;

    public void Initialize(int id, int parentIslandId)
    {
        spotID = id;
        parentIslandID = parentIslandId;
        spotName = $"Spot_{parentIslandId}_{id}";
        gameObject.name = spotName;
    }
}