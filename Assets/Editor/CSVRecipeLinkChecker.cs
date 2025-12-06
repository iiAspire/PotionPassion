using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CSVRecipeLinkChecker : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Check CSV Recipe Links")]
    public static void ShowWindow()
    {
        GetWindow<CSVRecipeLinkChecker>("CSV Recipe Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Check Recipe Link Consistency", EditorStyles.boldLabel);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Check CSV for Missing or Mismatched Card Links"))
        {
            if (csvFile == null)
            {
                Debug.LogError("Please assign a CSV file first!");
                return;
            }

            CheckLinks(csvFile);
        }
    }

    private void CheckLinks(TextAsset csv)
    {
        string[] lines = csv.text.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV file appears empty or missing headers.");
            return;
        }

        // Load all card assets
        Dictionary<string, CardData> allCards = new Dictionary<string, CardData>();
        foreach (CardData card in Resources.FindObjectsOfTypeAll<CardData>())
        {
            if (!allCards.ContainsKey(card.cardName.Trim().ToLower()))
                allCards.Add(card.cardName.Trim().ToLower(), card);
        }

        int headerIndex = 0;
        string[] headers = lines[0].Split(',');
        int nameIndex = System.Array.IndexOf(headers, "CardName");
        int resultIndex = System.Array.IndexOf(headers, "ResultCard");
        int toolIndex = System.Array.IndexOf(headers, "Tool");

        if (nameIndex < 0 || resultIndex < 0)
        {
            Debug.LogError("CSV must have 'CardName' and 'ResultCard' columns!");
            return;
        }

        int missingInputCount = 0;
        int missingOutputCount = 0;
        int total = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if (cols.Length <= Mathf.Max(nameIndex, resultIndex)) continue;

            string inputName = cols[nameIndex].Trim().ToLower();
            string outputName = cols[resultIndex].Trim().ToLower();
            string tool = (toolIndex >= 0 && toolIndex < cols.Length) ? cols[toolIndex].Trim() : "(unknown)";

            total++;

            if (!allCards.ContainsKey(inputName))
            {
                Debug.LogWarning($"❌ Input card '{cols[nameIndex].Trim()}' not found for tool {tool}");
                missingInputCount++;
            }

            if (!allCards.ContainsKey(outputName))
            {
                Debug.LogWarning($"❌ Result card '{cols[resultIndex].Trim()}' not found for tool {tool}");
                missingOutputCount++;
            }
        }

        Debug.Log($"✅ Checked {total} CSV rows. Missing Inputs: {missingInputCount}, Missing Outputs: {missingOutputCount}");
    }
}