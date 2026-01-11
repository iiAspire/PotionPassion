using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DropZone : MonoBehaviour, IDropHandler
{
    public enum InventoryZone
    {
        IngredientInventory,  // top-left
        SaleInventory,        // bottom-left
        PlayerInventory,      // top-right
        RecipeHolding,         // bottom-right
        Planter
    }

    public InventoryZone inventoryZone;

    // ✅ Assign these ONLY where needed in the Inspector
    [SerializeField] public RecipeBuilder addToRecipeBuilder;     // set on the RecipeHolding drop zone
    [SerializeField] public RecipeBuilder removeFromRecipeBuilder; // set on zones that should remove from recipe when card is dropped here

    [SerializeField] public PlanterSlot planterSlot;

    [Header("Planter Feedback")]
    [SerializeField] private Outline invalidOutline; // 👈 Add this
    public Color invalidFlashColor = Color.red;
    public float invalidFlashDuration = 0.25f;

    public bool AcceptsItem(CardComponent card)
    {
        if (card == null || card.CardData == null) return false;

        switch (inventoryZone)
        {
            case InventoryZone.IngredientInventory:
                return card.CardData.itemType != ItemType.Tool && card.CardData.itemType != ItemType.Crafting;
            case InventoryZone.SaleInventory:
                return card.CardData.IsSellable;
            case InventoryZone.PlayerInventory:
            case InventoryZone.RecipeHolding:
                return true;
            case InventoryZone.Planter:
                // 👇 Validate for planters
                if (planterSlot == null || planterSlot.growthDatabase == null)
                    return false;
                return planterSlot.growthDatabase.GetEntry(card.CardData.cardName) != null;
            default:
                return false;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        CardComponent card = eventData.pointerDrag.GetComponent<CardComponent>();
        if (card == null)
            return;

        // 🌱 If this drop zone is a planter and the card is a seed/spore
        if (inventoryZone == InventoryZone.Planter && planterSlot != null)
        {
            // 👇 ADD VALIDATION: Check if the planter can accept this seed
            if (planterSlot.growthDatabase == null)
            {
                Debug.LogWarning($"[{name}] Planter has no growth database assigned");
                FlashInvalidDrop();
                return; // Card will return to original location
            }

            var entry = planterSlot.growthDatabase.GetEntry(card.CardData.cardName);
            if (entry == null)
            {
                Debug.LogWarning($"[{name}] Invalid seed for planter: {card.CardData.cardName}");
                FlashInvalidDrop();
                return; // Card will return to original location via OnEndDrag
            }

            // Valid seed - plant it
            planterSlot.PlantSeed(card);
            return;
        }

        // ✔ Handle normal inventory zones (Ingredient, Tool, PlayerInventory, RecipeHolding)
        if (!AcceptsItem(card))
            return;

        // Add to recipe builder if needed
        if (addToRecipeBuilder != null)
            addToRecipeBuilder.AddCard(card);

        // Remove from recipe builder if moved out
        if (removeFromRecipeBuilder != null)
            removeFromRecipeBuilder.RemoveCard(card);

        // Reparent card visually
        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
    }

    public void FlashInvalidDrop()
    {
        if (invalidOutline == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashOutlineRoutine());
    }

    private IEnumerator FlashOutlineRoutine()
    {
        Color start = invalidOutline.effectColor;
        Color flash = invalidFlashColor;
        flash.a = 1f;

        invalidOutline.effectColor = flash;

        yield return new WaitForSeconds(invalidFlashDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            invalidOutline.effectColor = Color.Lerp(flash, start, t);
            yield return null;
        }
    }
}