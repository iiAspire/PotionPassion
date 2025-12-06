using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateBubbleCircle
{
    [MenuItem("Tools/Generate Bubble Circle Sprite")]
    public static void GenerateCircle()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        float radius = size * 0.45f;
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist <= radius)
                    tex.SetPixel(x, y, white);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }

        tex.Apply();

        string path = "Assets/BubbleCircle.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        // Set import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        Debug.Log("Bubble circle created at: " + path);
    }
}