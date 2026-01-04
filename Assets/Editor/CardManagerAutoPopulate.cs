using UnityEngine;
using UnityEditor;

public class CardManagerAutoPopulate : MonoBehaviour
{
    [MenuItem("Tools/Populate CardManager")]
    public static void PopulateCardManager()
    {
        // Load the CardManager asset
        string managerPath = "Assets/Resources/allCards.asset"; // adjust path if needed
        CardManager manager = AssetDatabase.LoadAssetAtPath<CardManager>(managerPath);

        if (manager == null)
        {
            Debug.LogError("CardManager asset not found at " + managerPath);
            return;
        }

        // Find all CardData assets in the project
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        CardData[] allCards = new CardData[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            allCards[i] = AssetDatabase.LoadAssetAtPath<CardData>(path);
        }

        // Assign to CardManager
        manager.allCards = allCards;

        // Mark dirty so changes are saved
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();

        //Debug.Log($"CardManager populated with {allCards.Length} cards.");
    }
}