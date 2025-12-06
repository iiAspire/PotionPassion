using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CardDataInspectorTools : EditorWindow
{
    private List<CardData> allCards = new List<CardData>();
    private Vector2 scrollPos;
    private string searchFolder = "Assets/Cards"; // Adjust if yours is different

    [MenuItem("Tools/Card Data Checker")]
    public static void OpenWindow()
    {
        GetWindow<CardDataInspectorTools>("Card Data Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("🃏 Card Data Checker", EditorStyles.boldLabel);
        GUILayout.Label("Scans your CardData assets and shows missing or incomplete data.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        searchFolder = EditorGUILayout.TextField("Search Folder", searchFolder);

        if (GUILayout.Button("Scan CardData Assets"))
        {
            ScanForCards();
        }

        if (allCards.Count == 0)
        {
            EditorGUILayout.HelpBox("Click 'Scan CardData Assets' to find cards.", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var card in allCards)
        {
            if (card == null) continue;

            bool hasRecipes = card.processingRecipes != null && card.processingRecipes.Count > 0;
            bool hasPrefab = card.cardPrefab != null;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            if (!hasRecipes || !hasPrefab)
                labelStyle.normal.textColor = Color.red;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(card.cardName, labelStyle);

            if (!hasPrefab)
                EditorGUILayout.LabelField("⚠ Missing Card Prefab!", EditorStyles.miniBoldLabel);

            if (!hasRecipes)
            {
                EditorGUILayout.LabelField("⚠ No Processing Recipes!", EditorStyles.miniBoldLabel);
            }
            else
            {
                for (int i = 0; i < card.processingRecipes.Count; i++)
                {
                    var recipe = card.processingRecipes[i];
                    if (recipe == null) continue;

                    EditorGUILayout.LabelField($"• Tool: {recipe.tool} | Outputs: {recipe.visualOutputs?.Count ?? 0}");

                    if (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0)
                        EditorGUILayout.LabelField("   ⚠ No visual outputs defined", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Select in Project"))
            {
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ScanForCards()
    {
        allCards.Clear();

        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { searchFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
                allCards.Add(card);
        }

        Debug.Log($"Found {allCards.Count} CardData assets in {searchFolder}");
    }
}