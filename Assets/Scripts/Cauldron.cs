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
    public RecipeBuilder recipeBuilder;
    public Transform outputParent;
    public GameObject cardPrefab;

    public ToolTimer toolTimer;
    public CauldronWorkbench cauldronWorkbench;

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

    public void OnDrop(PointerEventData eventData)
    {
        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null) return;

        var cardComp = draggedGO.GetComponent<CardComponent>();
        if (cardComp == null) return;

        if (recipeBuilder != null)
        {
            recipeBuilder.AddCard(cardComp);
        }
    }

    public void StartProcessing()
    {
        List<CardComponent> droppedCards = recipeBuilder.ConsumeAll();
        if (droppedCards.Count == 0) return;

        // Convert cards to ingredient names
        List<string> ingredientNames = new List<string>();
        foreach (var card in droppedCards)
        {
            if (card.CardData != null)
            {
                ingredientNames.Add(card.CardData.cardName);
            }
        }

        if (Recipes == null)
        {
            Debug.LogError("❌ Cauldron.recipeDatabase is NULL");
            return;
        }

        // 🔹 SINGLE CARD - Check if it's a processing recipe
        if (droppedCards.Count == 1)
        {
            CardComponent singleCard = droppedCards[0];

            // Check if this card has a cauldron processing recipe
            var recipe = singleCard.CardData.processingRecipes
                ?.Find(r => r.tool == ProcessingTool.Cauldron);

            if (recipe != null)
            {
                // Check if already processed
                if (singleCard.CardData.processedType != ProcessedType.None &&
                    (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
                {
                    Debug.Log($"Cannot reprocess '{singleCard.CardData.cardName}' - already processed");
                    ReturnCardToRecipeHolding(singleCard);
                    return;
                }

                // Check fire requirement
                if (recipe.needsFire &&
                    (cauldronWorkbench.fireController == null ||
                     !cauldronWorkbench.fireController.IsFireOn))
                {
                    Debug.LogWarning("🔥 Fire required for this process - returning card");
                    ReturnCardToRecipeHolding(singleCard);

                    // Optional: show feedback to player
                    if (cauldronWorkbench.failedBrewLogTMP != null)
                    {
                        cauldronWorkbench.failedBrewLogTMP.text = "Fire required for this process!";
                        cauldronWorkbench.failedBrewLogTMP.gameObject.SetActive(true);
                        StartCoroutine(HideTextAfterDelay(cauldronWorkbench.failedBrewLogTMP.gameObject, 2f));
                    }
                    return;
                }

                // Valid single-card processing - move to workbench's recipe holding
                singleCard.transform.SetParent(cauldronWorkbench.recipeHoldingParent, false);
                singleCard.transform.localPosition = Vector3.zero;

                if (cauldronWorkbench != null)
                {
                    cauldronWorkbench.StartBrewing(null, ingredientNames);
                }
                return;
            }
        }

        // 🔹 MULTI-INGREDIENT - Try spell combo lookup
        SpellCombo combo = Recipes.GetComboByIngredients(ingredientNames);

        if (cauldronWorkbench != null)
        {
            cauldronWorkbench.StartBrewing(combo, ingredientNames);
        }
        else
        {
            Debug.LogWarning("No CauldronWorkbench assigned! Cannot start brewing.");
        }
    }

    private void ReturnCardToRecipeHolding(CardComponent card)
    {
        if (recipeBuilder != null && recipeBuilder.transform != null)
        {
            card.transform.SetParent(recipeBuilder.transform, false);
            card.transform.localPosition = Vector3.zero;
        }
    }

    private System.Collections.IEnumerator HideTextAfterDelay(GameObject obj, float seconds)
    {
        yield return new UnityEngine.WaitForSeconds(seconds);
        if (obj != null)
            obj.SetActive(false);
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

        ProcessedType resultType = DetermineProcessedTypeFromCombo(combo);

        CardData tempData = ScriptableObject.CreateInstance<CardData>();
        tempData.cardName = combo.SpellName;
        tempData.processedType = resultType;

        card.SetCardData(tempData, true);
        card.AssignedCombo = combo;

        if (card.typeIconImage != null)
        {
            card.typeIconImage.sprite = null;
            card.typeIconImage.gameObject.SetActive(false);
        }

        return card;
    }

    private ProcessedType DetermineProcessedTypeFromCombo(SpellCombo combo)
    {
        return ProcessedType.Potion;
    }
}