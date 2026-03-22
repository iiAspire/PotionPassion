using UnityEngine;
using UnityEditor;

public class AutoAssignShelfVisuals
{
    static ShelfVisualData openPowder;
    static ShelfVisualData closedPowder;
    static ShelfVisualData liquidBotanical;
    static ShelfVisualData wholeMushroom;
    static ShelfVisualData closedBotanical;
    static ShelfVisualData openBotanical;
    static ShelfVisualData closedAnimal;
    static ShelfVisualData liquidAnimal;

    [MenuItem("Tools/Assign Shelf Visuals By Rules")]
    public static void Run()
    {
        LoadVisualAssets();

        string[] guids = AssetDatabase.FindAssets("t:CardData");

        int assigned = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (card == null)
                continue;

            ShelfVisualData visual = DetermineVisual(card);

            if (visual == null)
                continue;

            card.shelfVisuals = visual;
            EditorUtility.SetDirty(card);

            assigned++;
        }

        AssetDatabase.SaveAssets();

        Debug.Log("Shelf visuals assigned to " + assigned + " cards.");
    }

    static void LoadVisualAssets()
    {
        openPowder = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Open Powder.asset");
        closedPowder = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Closed Powder.asset");
        liquidBotanical = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Liquid Botanical.asset");
        wholeMushroom = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Whole Mushroom.asset");
        closedBotanical = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Closed Botanical.asset");
        openBotanical = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Open Botanical.asset");
        closedAnimal = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Closed Animal.asset");
        liquidAnimal = AssetDatabase.LoadAssetAtPath<ShelfVisualData>("Assets/Storeroom/Liquid Animal.asset");
    }

    static ShelfVisualData DetermineVisual(CardData card)
    {
        string name = card.cardName.ToLower();
        bool baseMatch = card.baseName == card.cardName;

        if (card.itemType == ItemType.Mineral)
        {
            if (baseMatch)
                return openPowder;

            if (name.Contains("dust"))
                return closedPowder;

            if (name.Contains("chips"))
                return openPowder;
        }

        if (card.itemType == ItemType.Botanical)
        {
            if (name.Contains("oil") || name.Contains("nectar"))
                return liquidBotanical;

            if (baseMatch)
                return wholeMushroom;

            if (name.Contains("cap") || name.Contains("stem") || name.Contains("head") ||
                name.Contains("gills") || name.Contains("petal") || name.Contains("leaf") ||
                name.Contains("bark") || name.Contains("woodchips"))
                return closedBotanical;

            if (name.Contains("pollen") || name.Contains("shavings") || name.Contains("kernels") || name.Contains("seed"))
                return closedPowder;

            if (name.Contains("thorn") || name.Contains("wart"))
                return openPowder;

            if (name.Contains("branch"))
                return openBotanical;
        }

        if (card.itemType == ItemType.Animal)
        {
            if (baseMatch)
                return closedAnimal;

            if (name.Contains("fur"))
                return closedAnimal;

            if (name.Contains("blood") || name.Contains("eye") || name.Contains("oil"))
                return liquidAnimal;

            if (name.Contains("leg") || name.Contains("skin") || name.Contains("wing") ||
                name.Contains("tail") || name.Contains("feather"))
                return closedAnimal;

            if (name.Contains("talon") || name.Contains("tooth") || name.Contains("shell") ||
                name.Contains("beak") || name.Contains("thorn") || name.Contains("needle") ||
                name.Contains("wart"))
                return openPowder;
        }

        if (card.itemType == ItemType.Metal)
        {

            if (name.Contains("dust"))
                return closedPowder;
        }

    return null;
    }
}