using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    public string tooltipText;
    public Vector2 offset = new Vector2(0, 20);

    private GameObject tooltipObject;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (string.IsNullOrEmpty(tooltipText))
            return;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        // Create tooltip object
        tooltipObject = new GameObject("Tooltip");
        tooltipObject.transform.SetParent(canvas.transform, false);

        // Add image background
        Image bg = tooltipObject.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Add outline
        Outline outline = tooltipObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        outline.effectDistance = new Vector2(1, 1);

        // Setup RectTransform
        RectTransform bgRect = tooltipObject.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(200, 40);

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltipObject.transform, false);

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = tooltipText;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-10, -10);

        // Position tooltip above the card
        PositionTooltip();

        // Bring to front
        tooltipObject.transform.SetAsLastSibling();
    }

    private void PositionTooltip()
    {
        if (tooltipObject == null)
            return;

        RectTransform tooltipRect = tooltipObject.GetComponent<RectTransform>();
        RectTransform cardRect = GetComponent<RectTransform>();

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Position above the card
            Vector3 cardWorldPos = cardRect.position;
            tooltipRect.position = cardWorldPos + (Vector3)offset;
        }
        else
        {
            // For world space or camera space canvases
            Vector2 cardPos = cardRect.anchoredPosition;
            tooltipRect.anchoredPosition = cardPos + offset;
        }
    }

    private void HideTooltip()
    {
        if (tooltipObject != null)
        {
            Destroy(tooltipObject);
        }
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void OnDestroy()
    {
        HideTooltip();
    }
}