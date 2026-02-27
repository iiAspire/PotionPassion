using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SORegionCSVImporter : EditorWindow
{
    TextAsset nodesCSV;
    TextAsset findsCSV;
    TextAsset edgesCSV;

    List<CardData> allCards;
    List<SOToolItem> allTools;

    string outputFolder = "Assets/Regions";

    [MenuItem("Tools/Import/SORegion CSV Importer")]
    static void Open()
    {
        GetWindow<SORegionCSVImporter>("Region Importer");
    }

    HashSet<string> missingCards = new();

    void OnGUI()
    {
        GUILayout.Label("CSV Files", EditorStyles.boldLabel);

        nodesCSV = (TextAsset)EditorGUILayout.ObjectField("Nodes", nodesCSV, typeof(TextAsset), false);
        findsCSV = (TextAsset)EditorGUILayout.ObjectField("Finds", findsCSV, typeof(TextAsset), false);
        edgesCSV = (TextAsset)EditorGUILayout.ObjectField("Edges", edgesCSV, typeof(TextAsset), false);

        GUILayout.Space(10);

        GUILayout.Label("Lookups", EditorStyles.boldLabel);

        SerializedObject so = new SerializedObject(this);
        EditorGUILayout.PropertyField(so.FindProperty("allCards"), true);
        EditorGUILayout.PropertyField(so.FindProperty("allTools"), true);
        so.ApplyModifiedProperties();

        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Import"))
            Import();
    }

    CardData FindCardStrict(string resourceName)
    {
        string[] guids = AssetDatabase.FindAssets("t:CardData");

        foreach (string g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<CardData>(path);

            if (asset != null &&
                asset.cardName == resourceName)
            {
                return asset;
            }
        }

        Debug.LogError($"CardData NOT FOUND for resource: {resourceName}");
        return null;
    }

    // ---------------------------------------------------------

    string SanitizeFileName(string input)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');

        return input.Trim();
    }

    void Import()
    {
        if (!AssetDatabase.IsValidFolder(outputFolder))
            AssetDatabase.CreateFolder("Assets", "Regions");

        var nodes = Parse(nodesCSV);
        var finds = Parse(findsCSV);
        var edges = Parse(edgesCSV);

        var regionByID = LoadOrCreateRegions(nodes);

        ImportFinds(regionByID, finds);
        ImportEdges(regionByID, edges);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Region import complete.");
    }

    // ---------------------------------------------------------
    // NODES
    // ---------------------------------------------------------

    Dictionary<int, SORegion> LoadOrCreateRegions(List<Dictionary<string, string>> rows)
    {
        var dict = new Dictionary<int, SORegion>();

        foreach (var r in rows)
        {
            int id = int.Parse(r["NodeID"]);

            var region = FindRegionAsset(id);

            if (region == null)
            {
                region = ScriptableObject.CreateInstance<SORegion>();
                region.nodeID = id;

                string type = r["LocationType"];
                string name = r["LocationName"];

                string assetName = $"{type} {name}";
                string safeName = SanitizeFileName(assetName);

                string path = AssetDatabase.GenerateUniqueAssetPath(
                    $"{outputFolder}/{safeName}.asset");

                AssetDatabase.CreateAsset(region, path);
            }

            region.locationType = r["LocationType"];
            region.locationName = r["LocationName"];
            region.description = r["Description"];

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

    // ---------------------------------------------------------
    // FINDS
    // ---------------------------------------------------------

    void ImportFinds(Dictionary<int, SORegion> regions,
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
                tool = allTools
                    .FirstOrDefault(t => t.displayName == r["ToolRequired"]);

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
        }

        if (missingCards.Count > 0)
        {
            Debug.LogError(
                $"Missing CardData assets:\n" +
                string.Join("\n", missingCards));
        }
    }

    // ---------------------------------------------------------
    // EDGES
    // ---------------------------------------------------------

    void ImportEdges(Dictionary<int, SORegion> regions,
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
        }
    }

    // ---------------------------------------------------------
    // CSV PARSER
    // ---------------------------------------------------------

    List<Dictionary<string, string>> Parse(TextAsset csv)
    {
        var lines = csv.text.Split('\n');
        var headers = lines[0].Trim().Split(',');

        var result = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var values = lines[i].Split(',');

            var dict = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length; j++)
                dict[headers[j]] = j < values.Length ? values[j].Trim() : "";

            result.Add(dict);
        }

        return result;
    }

    float ParseFloat(string s, float def)
    {
        return float.TryParse(s, out var v) ? v : def;
    }
}