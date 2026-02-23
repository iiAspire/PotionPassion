using UnityEngine;
using UnityEngine.UI;

public class RectTransformDebug : MonoBehaviour
{
    public RectTransform rt;
    public bool drawInGameView = true;
    public bool logOnClick = true;

    private Camera uiCam;

    void Awake()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        // If the canvas is Screen Space - Camera/World, we need that camera.
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = canvas.worldCamera;

        if (logOnClick)
        {
            var btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => LogRect("BUTTON CLICK"));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            LogRect("MOUSE DOWN");
    }

    void LogRect(string tag)
    {
        if (rt == null) return;

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // Convert to screen space
        Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);

        Rect screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        Debug.Log(
            $"[{tag}] {name}\n" +
            $"  activeInHierarchy={gameObject.activeInHierarchy}\n" +
            $"  sizeDelta={rt.sizeDelta} localScale={rt.localScale} lossyScale={rt.lossyScale}\n" +
            $"  anchoredPos={rt.anchoredPosition} pivot={rt.pivot} anchors=({rt.anchorMin}..{rt.anchorMax})\n" +
            $"  screenRect={screenRect}\n" +
            $"  mouse={Input.mousePosition} containsMouse={screenRect.Contains(Input.mousePosition)}"
        );
    }

    void OnGUI()
    {
        if (!drawInGameView || rt == null) return;

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 a = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
        Vector2 c = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);

        Rect r = Rect.MinMaxRect(a.x, Screen.height - c.y, c.x, Screen.height - a.y);

        // Draw an outline rectangle in Game view (IMGUI coordinates are y-flipped)
        DrawRectOutline(r, 2);
    }

    void DrawRectOutline(Rect r, int thickness)
    {
        Texture2D tex = Texture2D.whiteTexture;
        GUI.DrawTexture(new Rect(r.xMin, r.yMin, r.width, thickness), tex);
        GUI.DrawTexture(new Rect(r.xMin, r.yMax - thickness, r.width, thickness), tex);
        GUI.DrawTexture(new Rect(r.xMin, r.yMin, thickness, r.height), tex);
        GUI.DrawTexture(new Rect(r.xMax - thickness, r.yMin, thickness, r.height), tex);
    }
}