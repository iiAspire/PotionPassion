using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Spellcraft/Ingredient Database")]
public class IngredientDatabase : ScriptableObject
{
    [Header("Ingredients")]
    public List<string> Waters;
    public List<string> Craftings;

    [Header("Extras")]
    public List<string> Intimates;
    public List<string> Spirituals;
    public List<string> Astrologicals;
    public List<string> Elements;
    public List<string> Tools;

    [Header("Metals with Attributes")]
    public List<MetalIngredient> MetalIngredients;

    [Header("Animals with Attributes")]
    public List<AnimalIngredient> AnimalIngredients;

    [Header("Minerals with Attributes")]
    public List<MineralIngredient> MineralIngredients;

    [Header("Botanicals with Attributes")]
    public List<BotanicalIngredient> BotanicalIngredients;

    [System.Serializable]
    public class MetalIngredient
    {
        public string MetalName;
        public List<string> Parts;
    }

    [System.Serializable]
    public class AnimalIngredient
    {
        public string AnimalName;
        public List<string> Parts;
    }

    [System.Serializable]
    public class MineralIngredient
    {
        public string MineralName;
        public List<string> Parts;
    }

    [System.Serializable]
    public class BotanicalIngredient
    {
        public string BotanicalName;
        public List<string> Parts;
    }

    public List<string> GetIngredientsByCategory(string category)
    {
        switch (category)
        {
            case "Water":
                return Waters ?? new List<string>();

            case "Crafting":
                return Craftings ?? new List<string>();

            case "Metal":
                {
                    List<string> allMetals = new List<string>();
                    if (MetalIngredients != null)
                    {
                        foreach (var metal in MetalIngredients)
                        {
                            if (metal != null && !string.IsNullOrEmpty(metal.MetalName))
                            {
                                allMetals.Add(metal.MetalName);
                                if (metal.Parts != null)
                                    allMetals.AddRange(metal.Parts);
                            }
                        }
                    }
                    return allMetals;
                }

            case "Animal":
                {
                    List<string> allAnimals = new List<string>();
                    if (AnimalIngredients != null)
                    {
                        foreach (var animal in AnimalIngredients)
                        {
                            if (animal != null && !string.IsNullOrEmpty(animal.AnimalName))
                            {
                                allAnimals.Add(animal.AnimalName);
                                if (animal.Parts != null)
                                    allAnimals.AddRange(animal.Parts);
                            }
                        }
                    }
                    return allAnimals;
                }

            case "Mineral":
                {
                    List<string> allMinerals = new List<string>();
                    if (MineralIngredients != null)
                    {
                        foreach (var mineral in MineralIngredients)
                        {
                            if (mineral != null && !string.IsNullOrEmpty(mineral.MineralName))
                            {
                                allMinerals.Add(mineral.MineralName);
                                if (mineral.Parts != null)
                                    allMinerals.AddRange(mineral.Parts);
                            }
                        }
                    }
                    return allMinerals;
                }

            case "Botanical":
                {
                    List<string> allBotanicals = new List<string>();
                    if (BotanicalIngredients != null)
                    {
                        foreach (var botanical in BotanicalIngredients)
                        {
                            if (botanical != null && !string.IsNullOrEmpty(botanical.BotanicalName))
                            {
                                allBotanicals.Add(botanical.BotanicalName);
                                if (botanical.Parts != null)
                                    allBotanicals.AddRange(botanical.Parts);
                            }
                        }
                    }
                    return allBotanicals;
                }

            case "Intimate":
                return Intimates ?? new List<string>();

            case "Spiritual":
                return Spirituals ?? new List<string>();

            case "Astrological":
                return Astrologicals ?? new List<string>();

            case "Element":
                return Elements ?? new List<string>();

            case "Tool":
                return Tools ?? new List<string>();

            default:
                Debug.LogWarning($"Unknown ingredient category requested: {category}");
                return new List<string>();
        }
    }
}