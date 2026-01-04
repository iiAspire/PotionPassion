using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

public class CardCSVImporter : EditorWindow
{
    public TextAsset csvFile;
    public string savePath = "Assets/Cards/";

    [MenuItem("Tools/Import Cards from CSV")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CardCSVImporter), false, "CSV Card Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Card Import Settings", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Import CSV"))
        {
            if (csvFile == null)
            {
                Debug.LogError("No CSV file assigned!");
                return;
            }

            ImportCards(csvFile.text);
        }
    }

    private void ImportCards(string csvText)
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string[] lines = csvText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            Debug.LogWarning("CSV file is empty or invalid.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            // --- Load or create card asset ---
            string cardName = cols[0].Trim();
            string baseName = cols[45].Trim();
            string cardPath = $"{savePath}{cardName}.asset";
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(cardPath);
            bool isNew = false;
            if (card == null)
            {
                card = ScriptableObject.CreateInstance<CardData>();
                isNew = true;
            }

            // --- Basic properties ---
            card.cardName = cardName;

            card.baseName = baseName;

            // --- Main Icon (supports .png, .jpg, .jpeg, .tif, etc.) ---
            string baseSpritePath = cols[1].Trim();
            string[] possibleExtensions = { ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".webp" };

            Sprite icon = null;

            foreach (var ext in possibleExtensions)
            {
                string fullPath = $"{baseSpritePath}{ext}";
                icon = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);

                if (icon != null)
                {
                    card.Icon = icon;
                    break;
                }
            }

            if (icon == null)
            {
                Debug.LogWarning($"No icon found for '{cardName}' at any extension (png/jpg/jpeg/etc). Base path: {baseSpritePath}");
            }

            card.description = cols[2].Trim();

            string prefabPath = cols[3].Trim();  // e.g. "CardPrefabs/Sandstone"
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            card.cardPrefab = prefab;

            if (System.Enum.TryParse(cols[4].Trim(), true, out ItemType itemType))
                card.itemType = itemType;

            // --- Boolean flags ---
            card.Incinerates = ParseBool(cols[5]);
            card.Smoulders = ParseBool(cols[6]);
            card.Edible = ParseBool(cols[7]);
            card.Toxin = ParseBool(cols[8]);
            card.Neutral = ParseBool(cols[9]);
            card.Antidote = ParseBool(cols[10]);
            card.Umami = ParseBool(cols[11]);
            card.Sweet = ParseBool(cols[12]);
            card.Bitter = ParseBool(cols[13]);
            card.Salty = ParseBool(cols[14]);
            card.Spicy = ParseBool(cols[15]);
            card.Flowery = ParseBool(cols[16]);

            // --- Processing Recipe ---
            if (card.processingRecipes == null)
                card.processingRecipes = new List<ProcessingRecipe>();

            ProcessingRecipe recipe = new ProcessingRecipe();

            // Tool
            if (System.Enum.TryParse(cols[17].Trim(), true, out ProcessingTool tool))
                recipe.tool = tool;

            // Result card reference
            string baseResultName = cols[18].Trim(); // ResultCard column
            CardData baseResultCard = null;

            if (!string.IsNullOrEmpty(baseResultName))
            {
                string resultPath = $"{savePath}{baseResultName}.asset";
                baseResultCard = AssetDatabase.LoadAssetAtPath<CardData>(resultPath);
                recipe.resultCard = baseResultCard;
            }

            // Processing time
            if (float.TryParse(cols[19], out float processingTime))
                recipe.processingTime = processingTime;

            // Processed type
            if (System.Enum.TryParse(cols[20].Trim(), true, out ProcessedType processedType))
                recipe.processedResultType = processedType;

            // --- Card PartType (column 44) ---
            if (cols.Length > 44)
            {
                string partKey = cols[44].Trim();

                // CASE 1 — Blank → clear part data
                if (string.IsNullOrWhiteSpace(partKey))
                {
                    card.partType = PartType.None;
                    card.partIcon = null;
                }
                // CASE 2 — Valid part type → apply icon
                else if (Enum.TryParse(partKey, true, out PartType partType))
                {
                    card.partType = partType;

                    if (CardIconManager.Instance != null)
                        card.partIcon = CardIconManager.Instance.GetIconForPart(partType);
                    else
                        card.partIcon = null;
                }
                // CASE 3 — Invalid text → clear & warn
                else
                {
                    Debug.LogWarning($"Unknown PartType '{partKey}' for card {cardName}");
                    card.partType = PartType.None;
                    card.partIcon = null;
                }
            }

            if (!string.IsNullOrEmpty(baseResultName))
            {
                string resultPath = $"{savePath}{baseResultName}.asset";
                baseResultCard = AssetDatabase.LoadAssetAtPath<CardData>(resultPath);
                recipe.resultCard = baseResultCard;
            }

            // NeedsFire & alternative time
            if (cols.Length > 21)
            {
                recipe.needsFire = ParseBool(cols[21]);
                if (recipe.needsFire && cols.Length > 22 && float.TryParse(cols[22], out float timeWithFire))
                    recipe.processingTimeWithFire = timeWithFire;
            }

            // --- Visual Outputs (columns 24–33) ---
            recipe.visualOutputs = null;
            bool hasOutput = false;

            // Check if any output exists
            for (int j = 24; j <= 33 && j < cols.Length; j++)
            {
                if (!string.IsNullOrWhiteSpace(cols[j]))
                {
                    hasOutput = true;
                    break;
                }
            }

            if (hasOutput)
            {
                recipe.visualOutputs = new List<ProcessingVisualOutput>();

                for (int j = 24; j <= 33 && j < cols.Length; j++)
                {
                    string rawOutput = cols[j].Trim();
                    if (string.IsNullOrEmpty(rawOutput)) continue;

                    ProcessingVisualOutput vo = new ProcessingVisualOutput();

                    // Full name: "Angel's Bonnet Gills"
                    vo.name = baseResultName + " " + rawOutput;

                    // Try to convert to PartType
                    if (Enum.TryParse(rawOutput, true, out PartType outputPartType))
                    {
                        if (CardIconManager.Instance != null)
                        {
                            vo.icon = CardIconManager.Instance.GetIconForPart(outputPartType);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unknown output PartType '{rawOutput}' in {cardName}");
                    }

                    // Quantity column
                    int qtyCol = j + 10;
                    if (qtyCol < cols.Length && int.TryParse(cols[qtyCol], out int qty))
                        vo.quantity = qty;
                    else
                        vo.quantity = 1;

                    recipe.visualOutputs.Add(vo);
                }
            }
            // --- Save recipe into card ---
            bool recipeIsEmpty =
                string.IsNullOrWhiteSpace(cols[17]) &&    // Tool
                string.IsNullOrWhiteSpace(cols[18]) &&    // ResultCard
                string.IsNullOrWhiteSpace(cols[19]) &&    // ProcessingTime
                string.IsNullOrWhiteSpace(cols[20]) &&    // ProcessedResultType
                !hasOutput;                               // No visual outputs

            if (!recipeIsEmpty)
            {
                card.processingRecipes.Add(recipe);
            }

            // --- Save card asset ---
            if (isNew)
            {
                AssetDatabase.CreateAsset(card, cardPath);
                //Debug.Log($"Created new card: {cardName}");
            }
            else
            {
                EditorUtility.SetDirty(card);
                //Debug.Log($"Updated card: {cardName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        //Debug.Log("CSV Import complete.");
    }
            

    private bool ParseBool(string val)
    {
        val = val.Trim().ToLower();
        return val == "1" || val == "true" || val == "yes";
    }
}