using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ShelfItemView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private Image shelfContainerImage;
    [SerializeField] private Image shelfLabelImage;
    [SerializeField] private Image storedIcon;

    private Transform originalParent;
    private Sprite cachedJarSprite;
    public string tooltipText;
    private string sourceCardID;
    private StoreroomDisplayController controller;
    private bool isDragging = false;
    private CanvasGroup canvasGroup;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalParent = transform.parent;
        transform.SetParent(originalParent.parent);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    void ResetDragFlag()
    {
        isDragging = false;
    }

    int GetClosestSlot(Transform shelf)
    {
        int closest = shelf.childCount;
        float best = float.MaxValue;

        for (int i = 0; i < shelf.childCount; i++)
        {
            float d = Vector2.Distance(
                shelf.GetChild(i).position,
                transform.position);

            if (d < best)
            {
                best = d;
                closest = i;
            }
        }

        return closest;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Transform closestShelf = null;
        float closestDistance = float.MaxValue;

        foreach (Transform shelf in controller.GetShelves())
        {
            float d = Vector2.Distance(
                shelf.position,
                eventData.position
            );

            if (d < closestDistance)
            {
                closestDistance = d;
                closestShelf = shelf;
            }
        }

        if (closestShelf == null)
            closestShelf = originalParent;

        transform.SetParent(closestShelf);

        int order = GetDropIndex();
        transform.SetSiblingIndex(order);

        var card = GetCard();

        card.storeroomShelfIndex = controller.GetShelfIndex(closestShelf);
        card.storeroomOrderInShelf = order;

        CardPersistenceManager.Instance.SaveAllCards();
    }

    int GetDropIndex()
    {
        int closest = 0;
        float dist = float.MaxValue;

        for (int i = 0; i < originalParent.childCount; i++)
        {
            float d = Vector2.Distance(
                originalParent.GetChild(i).position,
                transform.position);

            if (d < dist)
            {
                dist = d;
                closest = i;
            }
        }

        return closest;
    }

    public void Bind(CardComponent card, StoreroomDisplayController owner)
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        controller = owner;

        if (card == null || card.CardData == null || card.CardData.shelfVisuals == null)
            return;

        sourceCardID = card.runtimeID;

        var visuals = card.CardData.shelfVisuals;

        if (cachedJarSprite == null)
        {
            cachedJarSprite = card.CardData.shelfVisuals.containerSprite;
        }

        shelfContainerImage.sprite = cachedJarSprite;
        shelfContainerImage.enabled = visuals.containerSprite != null;

        if (shelfLabelImage != null)
        {
            shelfLabelImage.sprite = card.CardData.Icon;
            shelfLabelImage.enabled = card.CardData.Icon != null;
        }

        bool stored = card.storedInStoreroom;

        if (storedIcon != null)
            storedIcon.enabled = stored;

        var tooltip = GetComponent<CardTooltip>();
        if (tooltip == null)
            tooltip = gameObject.AddComponent<CardTooltip>();

        tooltip.tooltipText = card.CardData.cardName;
    }

    CardComponent GetCard()
    {
        foreach (var card in FindObjectsOfType<CardComponent>(true))
        {
            if (card.runtimeID == sourceCardID)
                return card;
        }

        return null;
    }

    public CardComponent GetSourceCard()
    {
        return GetCard();
    }

    public void ToggleStored()
    {
        var card = GetCard();

        if (card == null)
            return;

        card.storedInStoreroom = !card.storedInStoreroom;

        card.RefreshVisibility();

        CardPersistenceManager.Instance.SaveAllCards();

        Bind(card, controller);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging)
            return;

        var card = GetCard();
        if (card == null)
            return;

        ToggleStored();
    }
}