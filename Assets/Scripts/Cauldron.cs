using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cauldron : MonoBehaviour, IDropHandler
{
    private static bool initializedThisSession = false;

    RecipeDatabase Recipes => GameInitialization.Recipes;
    ComboGenerator Combos => GameInitialization.Combos;

    public DropZone recipeHolding;
    public RecipeBuilder recipeBuilder;     // reference to the RecipeBuilder linked to recipeHolding
    public Transform outputParent;          // where new cards should appear
    public GameObject cardPrefab;

    public ToolTimer toolTimer;
    //public CardData testSpellCard;          // to permit adding test receipe 'Spell Test' to the RecipeDatabase

    public CauldronWorkbench cauldronWorkbench;

    /// Helper to create a pre-determined SpellCombo and add it to the runtime database (for learning new spells mid-game)
    private void AddPreDeterminedSpell(string spellName, List<string> ingredients, string tool)
    {
        if (Recipes == null) return;

        SpellCombo combo = new SpellCombo
        {
            SpellName = spellName,
            Ingredients = new List<string>(ingredients),
            Tool = tool,
            ResultCard = Combos != null
                ? Combos.GetResultCardForSpell(spellName)
                : null
        };

        Recipes.AddCombo(combo);
    }

    // test code start - TESTSPELL
    //public void SpawnTestCards()  // for adding a test recipe to the database
    //{
    //    if (Recipes == null)
    //    {
    //        Debug.LogError("RecipeDatabase not assigned!");
    //        return;
    //    }

    //    // Example test ingredients
    //    string[] testIngredients = { "Corn", "Chia", "Sandstone" };

    //    // Create a test SpellCombo for these ingredients
    //    SpellCombo testCombo = new SpellCombo
    //    {
    //        SpellName = "Spell Test",
    //        Ingredients = new List<string>(testIngredients),
    //        ResultCard = testSpellCard   // ✅ this line links the sprite
    //    };

    //    // Register it in the runtime RecipeDatabase
    //    Recipes.AddCombo(testCombo);

    //    Debug.Log($"✅ Test combo '{testCombo.SpellName}' registered with ResultCard '{testSpellCard?.name ?? "null"}'.");
    //}
    // testing code end

    public void OnDrop(PointerEventData eventData)
    {
        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null) return;

        var cardComp = draggedGO.GetComponent<CardComponent>();
        if (cardComp == null) return;

        // ✅ Forward the card to the RecipeBuilder, letting it handle the slot parent
        if (recipeBuilder != null)
        {
            recipeBuilder.AddCard(cardComp);
        }
    }

    public void StartProcessing()
    { 
        // Consume ingredients from the recipe holding
        List<CardComponent> droppedCards = recipeBuilder.ConsumeAll();
        if (droppedCards.Count == 0) return;

        // Convert the CardComponents into their ingredient names
        List<string> ingredientNames = new List<string>();
        foreach (var card in droppedCards)
        {
            if (card.CardData != null)
            {
                ingredientNames.Add(card.CardData.cardName);
            }
            else
            {
                Debug.LogWarning("Card has no name assigned!");
            }
        }

        if (Recipes == null)
        {
            Debug.LogError("❌ Cauldron.recipeDatabase is NULL");
            return;
        }

        if (Recipes.SpellCombos == null)
        {
            Debug.LogError("❌ Recipes.SpellCombos is NULL");
            return;
        }

        // Look up the combo in the runtime database for this playthrough
        SpellCombo combo =
            Recipes.GetComboByIngredients(ingredientNames);

        //Debug.Log(
        //    $"Cauldron DB instance: {Recipes.GetInstanceID()}, " +
        //    $"combos: {Recipes.SpellCombos.Count}"
        //);

        if (cauldronWorkbench != null)
        {
            cauldronWorkbench.StartBrewing(combo, ingredientNames); // ✅ Let workbench handle the rest
        }
        else
        {
            Debug.LogWarning("No CauldronWorkbench assigned! Cannot start brewing.");
        }
    }

    private CardComponent InstantiateCardFromCombo(SpellCombo combo, Transform parent)
    {
        GameObject cardGO = Instantiate(cardPrefab, parent);

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localRotation = Quaternion.identity;

        CardComponent card = cardGO.GetComponent<CardComponent>();
        if (card == null)
        {
            Debug.LogError("CardComponent missing on cardPrefab!");
            return null;
        }

        // Determine the processed type
        ProcessedType resultType = DetermineProcessedTypeFromCombo(combo);

        // Create a temporary CardData for this new card
        CardData tempData = ScriptableObject.CreateInstance<CardData>();
        tempData.cardName = combo.SpellName;
        tempData.processedType = resultType;
        //tempData.itemType = ItemType.Tool;

        // Assign CardData and force show processed icon
        card.SetCardData(tempData, true);

        // Optional: store the combo itself for reference on the card
        card.AssignedCombo = combo; // add this field to CardComponent

        if (card.typeIconImage != null)
        {
            card.typeIconImage.sprite = null;
            card.typeIconImage.gameObject.SetActive(false);
        }

        return card;
    }

    private ProcessedType DetermineProcessedTypeFromCombo(SpellCombo combo)
    {
        // Example logic: could be based on combo properties
        // For now, just return Potion as a default, you can replace with your rules
        return ProcessedType.Potion;
    }
}