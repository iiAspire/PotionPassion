using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteBaker : MonoBehaviour
{
    [MenuItem("Tools/Bake Selected To Sprite")]
    public static void BakeSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("Select a GameObject with SpriteRenderers to bake!");
            return;
        }

        var renderers = go.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError("No SpriteRenderers found in selected object.");
            return;
        }

        // Calculate bounds
        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        // Pixels per unit (tweak to your art style)
        int ppu = 64;
        int width = Mathf.CeilToInt(bounds.size.x * ppu);
        int height = Mathf.CeilToInt(bounds.size.y * ppu);

        if (width > 8192 || height > 8192)
        {
            Debug.LogError($"Requested texture too large! {width}x{height}");
            return;
        }

        // Create RenderTexture + Camera
        var rt = new RenderTexture(width, height, 24);
        var camGO = new GameObject("BakeCam");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = bounds.extents.y;
        cam.transform.position = bounds.center + Vector3.back * 10;
        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = Color.clear;

        // Render
        cam.Render();

        // Convert to Texture2D
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Ask user where to save
        string defaultName = go.name + "_Baked.png";
        string path = EditorUtility.SaveFilePanel("Save Baked Sprite", "Assets", defaultName, "png");
        if (string.IsNullOrEmpty(path))
        {
            // User cancelled
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(camGO);
            return;
        }

        // Make path relative to Assets if inside project
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        else
        {
            Debug.LogWarning("Saved outside Assets folder! Unity won’t auto-import.");
        }

        // Save PNG
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log($"Sprite baked to: {path}");

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camGO);

        // Import as Sprite if inside Assets
        if (path.StartsWith("Assets"))
        {
            AssetDatabase.Refresh();
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.SaveAndReimport();
        }
    }
}