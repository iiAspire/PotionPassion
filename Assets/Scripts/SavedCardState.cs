using System;
using UnityEngine;

[Serializable]
public enum CardContainer
{
    PlayerInventory,
    IngredientsInventory,
    RecipeHolding,
    CauldronOutput,
    PlanterOutput,
    Workbench
}

[Serializable]
public class SavedCardState
{
    // Identity
    public string runtimeID;
    public string cardName;
    public string baseName;
    public int stackID;       // What stack this card belongs to
    public int indexInStack;  // Order inside that stack
    public bool isStackRoot;  // Indicates top visible card
    public bool isStacked;        // true if this card was inside a CardStack
    public string stackKey;       // unique key for the stack within its container
    public string planterID;
    public string workbenchTool;

    // Core types
    public ItemType itemType;
    public ProcessedType processedType;
    public PartType partType;
    public QuantityType quantityType;

    // Traits
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

    // Location
    public CardContainer container;
    public int orderInParent;    // the order of cards within each inventory
}