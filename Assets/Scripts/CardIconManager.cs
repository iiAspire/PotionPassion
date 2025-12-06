using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardIconManager", menuName = "Cards/Card Icon Manager")]

public class CardIconManager : ScriptableObject
{ 
    public static CardIconManager Instance;
    [Header("Type Icons")]
    public List<IconEntry<ItemType>> typeIcons = new
        List<IconEntry<ItemType>>();
    [Header("Processed Icons")]
    public List<IconEntry<ProcessedType>> processedIcons = new
        List<IconEntry<ProcessedType>>();
    [Header("Part Icons")]
    public List<IconEntry<PartType>> partIcons = new
        List<IconEntry<PartType>>();
    [Header("Quantity Icons")]
    public List<IconEntry<QuantityType>> quantityIcons = new
        List<IconEntry<QuantityType>>();


    private void OnEnable()
    { if (Instance == null) Instance = this;
        else if (Instance != this)
            Debug.LogWarning("Multiple CardIconManager instances detected!");
    }
    
    // Helper to get icon for an ItemType
    public Sprite GetIconForType(ItemType type)
    { var entry = typeIcons.Find(e => e.key.Equals(type));
        return entry != null ? entry.icon : null; }
    
    // Helper to get icon for a ProcessedType
    public Sprite GetIconForProcessed(ProcessedType type)
    { var entry = processedIcons.Find(e => e.key.Equals(type));
        return entry != null ? entry.icon : null;
    
    }

    // Helper to get icon for a PartType
    public Sprite GetIconForPart(PartType type)
    {
        var entry = partIcons.Find(e => e.key.Equals(type));
        return entry != null ? entry.icon : null;

    }

    //Helper to get icon for a QuantityType
    public Sprite GetIconForQuantity(QuantityType type)
    {
        var entry = quantityIcons.Find(e => e.key.Equals(type));
        return entry != null ? entry.icon : null;

    }
}
[Serializable]
public class IconEntry<T>
{
    public T key;
    public Sprite icon;
}