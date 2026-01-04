using UnityEngine;
using UnityEngine.EventSystems;

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
}