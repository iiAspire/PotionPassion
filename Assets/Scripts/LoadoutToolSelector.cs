using System.Collections.Generic;
using UnityEngine;

public class LoadoutToolSelector : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] List<SOToolItem> availableTools;

    [Header("Prefabs")]
    [SerializeField] LoadoutToolCard toolCardPrefab;

    [Header("Containers")]
    [SerializeField] Transform selectedContainer;
    [SerializeField] Transform scrollContent;

    [Header("Scrolling")]
    [SerializeField] RectTransform scrollRect;
    [SerializeField] float scrollStep = 120f;

    [Header("Selection")]
    [SerializeField] int maxSelected = 3;

    readonly List<LoadoutToolCard> selectedTools = new();

    // =========================================================
    // INITIALIZATION
    // =========================================================

    void Start()
    {
        PopulateTools();
    }

    void PopulateTools()
    {
        foreach (var tool in availableTools)
        {
            LoadoutToolCard card = Instantiate(toolCardPrefab, scrollContent);
            card.Initialize(tool);
        }
    }

    // =========================================================
    // TOOL CLICK HANDLING
    // =========================================================

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

        selectedTools.Add(card);
        card.SetSelected(true);

        card.transform.SetParent(selectedContainer);
    }

    void Deselect(LoadoutToolCard card)
    {
        selectedTools.Remove(card);
        card.SetSelected(false);

        card.transform.SetParent(scrollContent);
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
        Vector2 pos = scrollRect.anchoredPosition;
        pos.x += amount;
        scrollRect.anchoredPosition = pos;
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