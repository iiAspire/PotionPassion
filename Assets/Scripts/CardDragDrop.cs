using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector3 originalPosition;
    private RecipeBuilder previousRecipeBuilder;
    private CardStack originalStack;

    private static List<RaycastResult> raycastResultsCache = new List<RaycastResult>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.position;
        previousRecipeBuilder = originalParent.GetComponentInParent<RecipeBuilder>();

        originalStack = originalParent.GetComponent<CardStack>();
        CardComponent thisCard = GetComponent<CardComponent>();

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        // The dragged card should appear as a single card
        thisCard.SetQuantityNumber(1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos))
        {
            rectTransform.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        CardComponent thisCard = GetComponent<CardComponent>();

        // Remove from previous RecipeBuilder if applicable
        if (previousRecipeBuilder != null)
        {
            previousRecipeBuilder.RemoveCard(thisCard);
            previousRecipeBuilder = null;
        }

        // Cache the original stack, but DO NOT remove yet
        //CardStack originalStack = transform.parent.GetComponent<CardStack>();

        // Raycast to find drop targets
        raycastResultsCache.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResultsCache);

        WorkbenchStation bestStation = null;
        Cauldron bestCauldron = null;
        DropZone bestInventory = null;
        int maxDepth = -1;

        foreach (var result in raycastResultsCache)
        {
            Cauldron cauldron = result.gameObject.GetComponent<Cauldron>();
            if (cauldron != null) { bestCauldron = cauldron; break; }

            WorkbenchStation station = result.gameObject.GetComponent<WorkbenchStation>();
            if (station != null) { bestStation = station; break; }

            DropZone zone = result.gameObject.GetComponent<DropZone>();
            if (zone == null || !zone.AcceptsItem(thisCard)) continue;

            int depth = 0;
            Transform t = zone.transform;
            while (t.parent != null) { depth++; t = t.parent; }
            if (depth > maxDepth) { maxDepth = depth; bestInventory = zone; }
        }

        // 1️⃣ Workbench drop
        if (bestStation != null)
        {
            bool accepted = bestStation.TryStartProcessing(thisCard);

            if (accepted)
            {
                if (originalStack != null)
                    originalStack.RemoveCard(thisCard);
            }
            else
            {
                bestStation.FlashInvalidDrop();
                ReturnToOriginal();
            }
            return;
        }

        // 2️⃣ Cauldron / RecipeHolding drop
        if (bestCauldron != null)
        {
            Transform targetParent = bestCauldron.recipeHolding != null
                ? bestCauldron.recipeHolding.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>()?.transform
                : bestCauldron.recipeHolding.transform;

            if (targetParent == null)
                targetParent = bestCauldron.recipeHolding.transform; // final fallback

            // --- reparent the card properly ---
            transform.SetParent(targetParent, false);
            transform.localPosition = Vector3.zero;

            // Track in recipe builder
            if (bestCauldron.recipeBuilder != null)
                bestCauldron.recipeBuilder.AddCard(thisCard);

            // Remove from old stack
            if (originalStack != null)
                originalStack.RemoveCard(thisCard);

            return;
        }

        // 3️⃣ PlayerInventory or RecipeHolding DropZone
        if (bestInventory != null)
        {
            Transform inventoryParent = bestInventory.transform;
            var gridLayout = bestInventory.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>();
            if (gridLayout != null) inventoryParent = gridLayout.transform;

            bool isRecipeHolding = bestInventory.inventoryZone == DropZone.InventoryZone.RecipeHolding;

            if (!isRecipeHolding)
            {
                // PlayerInventory → handle stacking
                CardStack targetStack = null;
                foreach (Transform child in inventoryParent)
                {
                    CardStack stack = child.GetComponent<CardStack>();
                    if (stack != null && stack.stackName == thisCard.CardData.cardName)
                    {
                        targetStack = stack;
                        break;
                    }
                }

                if (targetStack != null)
                {
                    // 🔥 If dropping back onto the SAME stack, do nothing
                    if (targetStack == originalStack)
                    {
                        // restore parenting only
                        thisCard.transform.SetParent(originalStack.transform, false);

                        RectTransform rt = thisCard.GetComponent<RectTransform>();
                        rt.anchoredPosition = Vector2.zero;
                        rt.localPosition = Vector3.zero;
                        rt.localRotation = Quaternion.identity;

                        originalStack.RefreshStack();
                        return;
                    }

                    // Normal stacking behavior
                    if (originalStack != null)
                        originalStack.RemoveCard(thisCard);

                    targetStack.AddCard(thisCard);
                    return;
                }
                else
                {
                    if (originalStack != null)
                        originalStack.RemoveCard(thisCard);

                    GameObject stackGO = new GameObject("CardStack_" + thisCard.CardData.cardName,
                        typeof(RectTransform), typeof(CardStack));
                    stackGO.transform.SetParent(inventoryParent, false);
                    CardStack newStack = stackGO.GetComponent<CardStack>();
                    newStack.Initialize(thisCard);
                }
            }
            else
            {
                // RecipeHolding → direct drop, no stacking
                transform.SetParent(inventoryParent, false);
                transform.localPosition = Vector3.zero;

                // Track in RecipeBuilder
                RecipeBuilder rb = bestInventory.addToRecipeBuilder;
                if (rb != null)
                {
                    rb.AddCard(thisCard);
                }

                if (originalStack != null)
                    originalStack.RemoveCard(thisCard);
            }

            return;
        }

        // 4️⃣ If no valid drop target, return to original
        transform.SetParent(originalParent, false);
        transform.localPosition = Vector3.zero;

        if (thisCard != null && thisCard.CardData != null)
        {
            thisCard.SetCardData(thisCard.CardData, forceShowProcessed: true);
        }

        return;
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent, false);
        transform.localPosition = Vector3.zero;

        CardComponent cc = GetComponent<CardComponent>();
        if (cc != null)
            cc.SetCardData(cc.CardData, forceShowProcessed: true);
    }
}