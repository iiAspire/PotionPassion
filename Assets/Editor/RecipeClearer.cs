using UnityEngine;
using UnityEditor;

public class RecipeClearer : EditorWindow
{
    [MenuItem("Tools/Clear Imported Recipes")]
    public static void ShowWindow()
    {
        GetWindow<RecipeClearer>("Clear Recipes");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Clear All Card Processing Recipes"))
        {
            ClearAllRecipes();
        }
    }

    private static void ClearAllRecipes()
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null && card.processingRecipes != null && card.processingRecipes.Count > 0)
            {
                Undo.RecordObject(card, "Clear Processing Recipes");
                card.processingRecipes.Clear();
                EditorUtility.SetDirty(card);
                Debug.Log($"Cleared recipes for {card.cardName}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ All card recipes cleared.");
    }
}