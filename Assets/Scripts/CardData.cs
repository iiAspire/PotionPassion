using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Botanical,
    Animal,
    Water,
    Mineral,
    Metal,
    Intimate,
    Astrological,
    Spiritual,
    Element,
    Tool,
    Crafting
}

public enum ProcessingType
{
    None,
    Crush,
    Steep,
    Chop,
    Dry
}

public enum ProcessedType
{
    None,
    Paste,
    Powder,
    Chopped,
    Poison,
    Potion,
    Dried,
    Steeped,
    Brewed
}

public enum PartType
{ 
    None,
    Oil, Eye, Leg, Blood, Wing, Beak, Talon, Fur, Tail, Feather, Skin, Tooth, Shell,
    Stem, Cap, Gills, Wart,
    Pollen, Head, Nectar, Petal, Leaf, Thorn,
    Bark, Woodchips, Shavings,
    Lump, Chips, Dust,
    Branch, Seed, Kernels, Needle
}

public enum QuantityType
{
    None, One, Two, Three, Four, Five,
}

[System.Serializable]
public class ProcessingVisualOutput
{
    public string name;           // e.g. "Legs"
    public Sprite icon;           // visual sprite shown on card
    public int quantity = 1;      // e.g. 4
    [TextArea] public string note; // optional flavor text
}

[System.Serializable]
public class ProcessingRecipe
{
    public bool foldout;
    public ProcessingTool tool;
    public CardData resultCard;
    public float processingTime;
    public ProcessedType processedResultType = ProcessedType.None;
    public bool needsFire = false;
    public float processingTimeWithFire = 0f;

    [Header("Additional Visual Outputs")]
    public List<ProcessingVisualOutput> visualOutputs = new List<ProcessingVisualOutput>();
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string baseName;
    public Sprite Icon;
    public Sprite cauldronContentsSprite;
    public Sprite typeIcon;
    public Color cardColor;
    public string description;
    public GameObject cardPrefab;
    public Sprite processedIcon;
    public ItemType itemType;                // Shared across all types
    public ProcessedType processedType = ProcessedType.None;
    public PartType partType;                // Shared across all types
    public Sprite partIcon;
    public QuantityType quantityType;
    public Sprite quantityIcon;

    public bool Toxin;
    public bool Neutral;
    public bool Antidote;
    public bool Sweet;
    public bool Bitter;
    public bool Salty;
    public bool Spicy;
    public bool Flowery;
    public bool Umami;
    public bool Edible;
    public bool Incinerates;
    public bool Smoulders;

    public Sprite topHalfSprite;
    public Sprite bottomHalfSprite;

    [Header("Processing Recipes")]
    public List<ProcessingRecipe> processingRecipes = new List<ProcessingRecipe>();

    public static class OutputNames
    {
        public static readonly Dictionary<ItemType, string[]> NamesByType = new Dictionary<ItemType, string[]>()
    {
        { ItemType.Metal, new string[] { "Dust", "Chips", "Lump", "Hunk" } },
        { ItemType.Animal, new string[] { "Oil", "Eye", "Leg", "Blood", "Wing", "Beak", "Talon", "Fur", "Tail", "Feather", "Skin", "Tooth", "Shell" } },
        { ItemType.Mineral, new string[] { "Dust", "Chips", "Lump", "Hunk", "Footprint", "Farm", "Graveyard", "Crossroad" } },
        { ItemType.Botanical, new string[] { "Shavings", "Woodchips", "Branch", "Leaf", "Bark", "Petal", "Nectar", "Pollen", "Thorn", "Needle", "Berry", "Stem", "Head", "Cap", "Gills", "Kernels", "Seed", "Oil", "Wart" } }
    };
    }

    private void OnValidate()
    {
        ApplyDefaultColor();
    }

    public void ApplyDefaultColor()
    {
        switch (itemType)
        {
            case ItemType.Botanical: cardColor = new Color(0.3f, 0.8f, 0.3f, 1f); break; // green
            case ItemType.Animal: cardColor = new Color(0.9f, 0.3f, 0.3f, 1f); break; // red
            case ItemType.Tool: cardColor = new Color(0.9f, 0.7f, 0.2f, 1f); break; // yellow
            case ItemType.Crafting: cardColor = new Color(0.9f, 0.5f, 0.1f, 1f); break; // orange
            case ItemType.Water: cardColor = new Color(0.2f, 0.6f, 0.9f, 1f); break; // sky blue
            case ItemType.Spiritual: cardColor = new Color(0.6f, 0.3f, 0.8f, 1f); break; // purple
            case ItemType.Mineral: cardColor = new Color(0.3f, 0.5f, 0.9f, 1f); break; // blue
            case ItemType.Metal: cardColor = new Color(0.4f, 0.4f, 0.4f, 1f); break; // gray
            case ItemType.Intimate: cardColor = new Color(0.9f, 0.3f, 0.6f, 1f); break; // pink
            case ItemType.Astrological: cardColor = new Color(0.6f, 0.9f, 0.3f, 1f); break; // lime
            case ItemType.Element: cardColor = new Color(0.8f, 0.8f, 0.3f, 1f); break; // olive
        }
    }

    public void CopyFrom(CardData other)
    {
        cardPrefab = other.cardPrefab;
        cardColor = other.cardColor;
        Icon = other.Icon;
        typeIcon = other.typeIcon;
        processedIcon = other.processedIcon;
        partIcon = other.partIcon;
        //quantityIcon = other.quantityIcon;

        itemType = other.itemType;
        processedType = other.processedType;
        partType = other.partType;
        //quantityType = other.quantityType;

        this.processingRecipes = new List<ProcessingRecipe>();
        foreach (var r in other.processingRecipes)
        {
            this.processingRecipes.Add(r);  // shallow copy OK because recipes are assets
        }
    }

    private void ApplyTypeIcon()
    {
        if (CardIconManager.Instance != null)
        {
            typeIcon = CardIconManager.Instance.GetIconForType(itemType);
        }
    }

    public void ApplyProcessedIcon(ProcessedType typeOverride = ProcessedType.None)
    {
        ProcessedType typeToUse = typeOverride != ProcessedType.None
            ? typeOverride
            : processedType;

        if (typeToUse == ProcessedType.None)
        {
            processedIcon = null;
            return;
        }

        if (CardIconManager.Instance != null)
        {
            processedIcon = CardIconManager.Instance.GetIconForProcessed(typeToUse);
        }

        if (!Application.isPlaying)
            return;

#if UNITY_EDITOR
        if (UnityEditor.AssetDatabase.Contains(this))
        {
            Debug.LogError(
                $"🚨 MUTATING CARD DATA ASSET: {name}",
            this
        );
    }
#endif
    }

    public void ApplyPartIcon(PartType typeOverride = PartType.None)
    {
        if (CardIconManager.Instance != null)
        {
            // If typeOverride is None, use the card's current partType
            PartType typeToUse = typeOverride != PartType.None ? typeOverride : partType;
            partIcon = CardIconManager.Instance.GetIconForPart(typeToUse);
        }
    }

    public void ApplyQuantityIcon(QuantityType typeOverride = QuantityType.None)
    {
        if (CardIconManager.Instance != null)
        {
            // If typeOverride is None, use the card's current quantityType
            QuantityType typeToUse = typeOverride != QuantityType.None ? typeOverride : quantityType;
            quantityIcon = CardIconManager.Instance.GetIconForQuantity(typeToUse);
        }
    }
}