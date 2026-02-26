using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Region")]
public class SORegion : ScriptableObject
{
    public string displayName;
    public Vector2 mapPosition;

    [Header("Possible Finds")]
    public List<RegionFind> possibleFinds = new();
}