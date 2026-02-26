using UnityEngine;

[System.Serializable]
public class RegionFind
{
    public CardData resource;

    [Range(0f, 1f)]
    public float baseProbability = 0.3f;

    [Header("Tool Requirement")]
    public SOToolItem requiredTool;

    [Header("Seasonal Modifier")]
    public bool seasonal;
    public Season favoredSeason;
    public float seasonMultiplier = 1f;

    [TextArea]
    public string description;   // <-- Add this
}