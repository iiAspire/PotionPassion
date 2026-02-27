using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Region")]
public class SORegion : ScriptableObject
{
    public int nodeID;

    [Header("Identity")]
    public string locationType;
    public string locationName;
    public string description;

    [Header("Finds")]
    public List<RegionFind> possibleFinds = new();

    [Header("Travel")]
    public List<RegionEdge> edges = new();
}

[System.Serializable]
public class RegionFind
{
    public CardData resource;
    [Range(0f, 1f)] public float chance;

    public SOToolItem requiredTool;
    public bool toolEssential;

    public float winter = 1f;
    public float spring = 1f;
    public float summer = 1f;
    public float autumn = 1f;

    public float harvestMinutes = 0f;

    public string notes;
}

[System.Serializable]
public class RegionEdge
{
    public SORegion destination;
    public float travelTime;
}