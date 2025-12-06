using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlantGrowthEntry
{
    public string seedName;                // e.g. "Carrot Seed"
    public CardData grownPlant;            // e.g. "Carrot"
    public float growTime = 10f;
    public int outputQuantity = 1;

    [Header("Growth Stages")]
    public Sprite stage1;
    public Sprite stage2;
    public Sprite stageFinal;
}

[CreateAssetMenu(fileName = "PlantGrowthDatabase", menuName = "Databases/Plant Growth Database")]
public class PlantGrowthDatabase : ScriptableObject
{
    public List<PlantGrowthEntry> entries = new List<PlantGrowthEntry>();

    public PlantGrowthEntry GetEntry(string seedName)
    {
        return entries.Find(e => e.seedName == seedName);
    }
}