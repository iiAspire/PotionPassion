using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SORegionCSVImporter : EditorWindow
{
    TextAsset nodesCSV;
    TextAsset findsCSV;
    TextAsset edgesCSV;

    string outputFolder = "Assets/Resources/Regions";

    HashSet<string> missingCards = new();

    List<SOToolItem> allTools;

    [MenuItem("Tools/Import/SORegion CSV Importer")]
    static void Open()
    {
        GetWindow<SORegionCSVImporter>("Region Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV Files", EditorStyles.boldLabel);

        nodesCSV = (TextAsset)EditorGUILayout.ObjectField("Nodes CSV", nodesCSV, typeof(TextAsset), false);
        findsCSV = (TextAsset)EditorGUILayout.ObjectField("Finds CSV", findsCSV, typeof(TextAsset), false);
        edgesCSV = (TextAsset)EditorGUILayout.ObjectField("Edges CSV", edgesCSV, typeof(TextAsset), false);

        GUILayout.Space(10);

        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("IMPORT CSV DATA", GUILayout.Height(40)))
            Import();
    }

    // =========================================================
    // IMPORT ENTRY
    // =========================================================

    void Import()
    {
        if (nodesCSV == null || findsCSV == null || edgesCSV == null)
        {
            Debug.LogError("Assign all CSV files first.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputFolder))
            AssetDatabase.CreateFolder("Assets", "Regions");

        allTools = FindAllTools();
        missingCards.Clear();

        var nodes = Parse(nodesCSV);
        var finds = Parse(findsCSV);
        var edges = Parse(edgesCSV);

        var regions = LoadOrCreateRegions(nodes);

        ImportFinds(regions, finds);
        ImportEdges(regions, edges);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (missingCards.Count > 0)
        {
            Debug.LogError(
                "Missing CardData assets:\n" +
                string.Join("\n", missingCards));
        }

        Debug.Log("Region import complete.");
    }

    // =========================================================
    // REGION CREATION
    // =========================================================

    Dictionary<int, SORegion> LoadOrCreateRegions(
        List<Dictionary<string, string>> rows)
    {
        var dict = new Dictionary<int, SORegion>();

        foreach (var r in rows)
        {
            int id = int.Parse(r["NodeID"]);

            var region = FindRegionAsset(id);

            string type = r["LocationType"];
            string name = r["LocationName"];

            string desiredName = SanitizeFileName($"{type} {name}");

            if (region == null)
            {
                region = ScriptableObject.CreateInstance<SORegion>();
                region.nodeID = id;

                string path = AssetDatabase.GenerateUniqueAssetPath(
                    $"{outputFolder}/{desiredName}.asset");

                AssetDatabase.CreateAsset(region, path);
            }
            else
            {
                // Rename existing asset if needed
                string assetPath = AssetDatabase.GetAssetPath(region);
                string currentName =
                    System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (currentName != desiredName)
                {
                    AssetDatabase.RenameAsset(assetPath, desiredName);
                }
            }

            region.locationType = type;
            region.locationName = name;
            region.description = r["Description"];
            EditorUtility.SetDirty(region);

            region.possibleFinds.Clear();
            region.edges.Clear();

            dict[id] = region;
        }

        return dict;
    }

    SORegion FindRegionAsset(int id)
    {
        string[] guids = AssetDatabase.FindAssets("t:SORegion");

        foreach (string g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var r = AssetDatabase.LoadAssetAtPath<SORegion>(path);

            if (r != null && r.nodeID == id)
                return r;
        }

        return null;
    }

    // =========================================================
    // FINDS IMPORT
    // =========================================================

    void ImportFinds(
        Dictionary<int, SORegion> regions,
        List<Dictionary<string, string>> rows)
    {
        foreach (var r in rows)
        {
            int id = int.Parse(r["NodeID"]);

            if (!regions.TryGetValue(id, out var region))
                continue;

            var card = FindCardStrict(r["ResourceName"]);

            if (card == null)
            {
                missingCards.Add(r["ResourceName"]);
                continue;
            }

            SOToolItem tool = null;

            if (!string.IsNullOrWhiteSpace(r["ToolRequired"]) &&
                r["ToolRequired"] != "Bare")
            {
                tool = allTools.FirstOrDefault(
                    t => t.displayName == r["ToolRequired"]);

                if (tool == null)
                    Debug.LogWarning($"Missing Tool: {r["ToolRequired"]}");
            }

            var find = new RegionFind
            {
                resource = card,
                chance = float.Parse(r["Chance"]) / 100f,
                requiredTool = tool,
                toolEssential = r["ToolEssential"] == "1",
                winter = ParseFloat(r["Winter"], 1f),
                spring = ParseFloat(r["Spring"], 1f),
                summer = ParseFloat(r["Summer"], 1f),
                autumn = ParseFloat(r["Autumn"], 1f),
                notes = r["Notes"]
            };

            region.possibleFinds.Add(find);
            EditorUtility.SetDirty(region);
        }
    }

    // =========================================================
    // EDGE IMPORT
    // =========================================================

    void ImportEdges(
        Dictionary<int, SORegion> regions,
        List<Dictionary<string, string>> rows)
    {
        foreach (var r in rows)
        {
            int from = int.Parse(r["FromNodeID"]);
            int to = int.Parse(r["ToNodeID"]);
            float time = float.Parse(r["TravelTime"]);

            if (!regions.TryGetValue(from, out var origin)) continue;
            if (!regions.TryGetValue(to, out var dest)) continue;

            origin.edges.Add(new RegionEdge
            {
                destination = dest,
                travelTime = time
            });

            EditorUtility.SetDirty(origin);
        }
    }

    // =========================================================
    // STRICT CARD LOOKUP (EXACT FILE NAME)
    // =========================================================

    CardData FindCardStrict(string resourceName)
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");

        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string fileName =
                System.IO.Path.GetFileNameWithoutExtension(path);

            if (string.Equals(fileName,
                              resourceName,
                              System.StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<CardData>(path);
            }
        }

        Debug.LogError($"CardData NOT FOUND: {resourceName}");
        return null;
    }

    // =========================================================
    // TOOL LOOKUP
    // =========================================================

    List<SOToolItem> FindAllTools()
    {
        string[] guids = AssetDatabase.FindAssets("t:SOToolItem");

        var list = new List<SOToolItem>();

        foreach (string g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<SOToolItem>(path);

            if (asset != null)
                list.Add(asset);
        }

        return list;
    }

    // =========================================================
    // CSV PARSER
    // =========================================================

    List<Dictionary<string, string>> Parse(TextAsset csv)
    {
        var lines = csv.text
            .Split(new[] { '\r', '\n' },
                   System.StringSplitOptions.RemoveEmptyEntries);

        var headers = lines[0]
            .Split(',')
            .Select(h => h.Trim())
            .ToArray();

        var result = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');

            var dict = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length; j++)
            {
                string key = headers[j];
                string value = j < values.Length ? values[j].Trim() : "";

                dict[key] = value;
            }

            result.Add(dict);
        }

        return result;
    }

    // =========================================================
    // UTILITIES
    // =========================================================

    string SanitizeFileName(string input)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');

        return input.Trim();
    }

    float ParseFloat(string s, float def)
    {
        return float.TryParse(s, out var v) ? v : def;
    }
}