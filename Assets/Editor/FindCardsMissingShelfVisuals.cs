using UnityEngine;
using UnityEditor;

public class FindCardsMissingShelfVisuals
{
    [MenuItem("Tools/Find Cards Missing Shelf Visuals")]
    public static void Find()
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");

        int missing = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (card == null)
                continue;

            if (card.shelfVisuals == null)
            {
                Debug.Log($"Missing shelf visual: {card.cardName}", card);
                missing++;
            }
        }

        Debug.Log($"Total cards missing shelf visuals: {missing}");
    }
}