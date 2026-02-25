using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutToolSelector : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] List<SOToolItem> availableTools;
    [SerializeField] LoadoutScreen loadoutScreen;

    [Header("Prefabs")]
    [SerializeField] LoadoutToolCard toolCardPrefab;

    [Header("Containers")]
    [SerializeField] Transform selectedContainer;
    [SerializeField] Transform scrollContent;

    [Header("Scrolling")]
    [SerializeField] RectTransform viewport; // visible window
    [SerializeField] RectTransform content;  // ScrollGrid that contains the cards
    [SerializeField] float scrollStep = 120f;
    [SerializeField] UnityEngine.UI.GridLayoutGroup grid;
    [SerializeField] RectTransform scrollArea;

    [Header("Selection")]
    [SerializeField] int maxSelected = 3;
    [SerializeField] TMPro.TMP_Text errorText;
    [SerializeField] Button leftArrow;
    [SerializeField] Button rightArrow;

    readonly List<LoadoutToolCard> selectedTools = new();

    // =========================================================
    // INITIALIZATION
    // =========================================================

    void Start()
    {
        PopulateTools();
        scrollStep = grid.cellSize.x + grid.spacing.x;

        Canvas.ForceUpdateCanvases();
        content.anchoredPosition = Vector2.zero;
        UpdateArrowState();
    }

    void PopulateTools()
    {
        for (int i = 0; i < availableTools.Count; i++)
        {
            var tool = availableTools[i];

            LoadoutToolCard card = Instantiate(toolCardPrefab, content);
            card.Initialize(tool, this);

            card.OriginalIndex = i;   // ⭐ remember position
        }

        //Debug.Log("Children: " + content.childCount);
    }

    // =========================================================
    // TOOL CLICK HANDLING
    // =========================================================

    void UpdateLayoutForSelection()
    {
        float slotWidth = grid.cellSize.x + grid.spacing.x;
        float offset = slotWidth * selectedTools.Count;

        // Move ScrollArea right by adjusting its left inset
        scrollArea.offsetMin = new Vector2(offset, scrollArea.offsetMin.y);

        // Reset scroll position so new items start visible
        content.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)selectedContainer);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollArea);

        UpdateArrowState();
    }

    void EnsureVisible(LoadoutToolCard card)
    {
        Canvas.ForceUpdateCanvases();

        var b = RectTransformUtility.CalculateRelativeRectTransformBounds(
            viewport, (RectTransform)card.transform);

        if (b.min.x < 0)
            MoveScroll(+scrollStep);

        if (b.max.x > scrollArea.rect.width)
            MoveScroll(-scrollStep);
    }

    public void OnToolClicked(LoadoutToolCard card)
    {
        if (selectedTools.Contains(card))
            Deselect(card);
        else
            TrySelect(card);
    }

    void TrySelect(LoadoutToolCard card)
    {
        if (selectedTools.Count >= maxSelected)
            return;

        if (loadoutScreen != null)
        {
            if (!loadoutScreen.ToggleTool(card.ToolData, out string reason))
            {
                if (CardClickLog.Instance != null)
                    CardClickLog.Instance.Log(reason);
                return;
            }
        }

        selectedTools.Add(card);
        card.SetSelected(true);
        card.transform.SetParent(selectedContainer, false);
        UpdateLayoutForSelection();
    }

    void ShowToolError(string message)
    {
        if (errorText)
            errorText.text = message;

        Debug.Log(message);
    }

    void Deselect(LoadoutToolCard card)
    {
        selectedTools.Remove(card);
        card.SetSelected(false);

        card.transform.SetParent(content, false);

        if (loadoutScreen)
            loadoutScreen.ToggleTool(card.ToolData, out _);

        // ⭐ Put it back where it belongs
        card.transform.SetSiblingIndex(card.OriginalIndex);

        EnsureVisible(card);
        UpdateLayoutForSelection();   // or your layout method
    }

    // =========================================================
    // SCROLL BUTTONS
    // =========================================================

    public void ScrollLeft()
    {
        MoveScroll(+scrollStep);
    }

    public void ScrollRight()
    {
        MoveScroll(-scrollStep);
    }

    void MoveScroll(float amount)
    {
        // viewport = visible panel
        // content  = ScrollGrid that holds cards

        var b = RectTransformUtility.CalculateRelativeRectTransformBounds(
            viewport, content);

        float contentWidth = b.size.x;
        float viewportWidth = scrollArea.rect.width;

        float maxScroll = Mathf.Max(0f, contentWidth - viewportWidth);

        Vector2 pos = content.anchoredPosition;

        // Move (negative = left, positive = right for typical setups)
        pos.x = Mathf.Clamp(pos.x + amount, -maxScroll, 0f);

        content.anchoredPosition = pos;

        UpdateArrowState();
    }

    void UpdateArrowState()
    {
        var b = RectTransformUtility.CalculateRelativeRectTransformBounds(
            viewport, content);

        float contentWidth = b.size.x;
        float viewportWidth = scrollArea.rect.width;

        float maxScroll = Mathf.Max(0f, contentWidth - viewportWidth);

        float x = content.anchoredPosition.x;

        bool atStart = x >= -0.01f;          // small tolerance for float error
        bool atEnd = x <= -maxScroll + 0.01f;

        leftArrow.interactable = !atStart;
        rightArrow.interactable = !atEnd;
    }

    // =========================================================
    // ACCESS
    // =========================================================

    public List<SOToolItem> GetSelectedToolData()
    {
        List<SOToolItem> result = new();

        foreach (var card in selectedTools)
            result.Add(card.ToolData);

        return result;
    }
}