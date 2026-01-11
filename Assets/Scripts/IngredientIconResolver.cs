using UnityEngine;

public static class IngredientIconResolver
{
    public static Sprite GetItemTypeIcon(string ingredient)
    {
        ItemType type = ResolveItemType(ingredient);
        return CardIconManager.Instance != null
            ? CardIconManager.Instance.GetIconForType(type)
            : null;
    }

    public static Sprite GetPartIcon(string ingredient)
    {
        PartType part = ResolvePartType(ingredient);
        return CardIconManager.Instance != null
            ? CardIconManager.Instance.GetIconForPart(part)
            : null;
    }

    private static ItemType ResolveItemType(string ingredient)
    {
        ingredient = ingredient.ToLower();

        if (ingredient.Contains("water")) return ItemType.Water;
        if (ingredient.Contains("tool")) return ItemType.Tool;

        // Animal indicators
        if (ingredient.Contains("frog") ||
            ingredient.Contains("rat") ||
            ingredient.Contains("bird"))
            return ItemType.Animal;

        // Botanical indicators
        if (ingredient.Contains("oak") ||
            ingredient.Contains("leaf") ||
            ingredient.Contains("bark"))
            return ItemType.Botanical;

        // Mineral / metal
        if (ingredient.Contains("iron")) return ItemType.Metal;
        if (ingredient.Contains("stone")) return ItemType.Mineral;

        return ItemType.Crafting;
    }

    private static PartType ResolvePartType(string ingredient)
    {
        foreach (PartType part in System.Enum.GetValues(typeof(PartType)))
        {
            if (part == PartType.None) continue;

            if (ingredient.ToLower().Contains(part.ToString().ToLower()))
                return part;
        }

        return PartType.None;
    }
}