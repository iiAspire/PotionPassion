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
        Debug.Log("🔵 [Cauldron] StartProcessing called");

        List<CardComponent> droppedCards = recipeBuilder.ConsumeAll();
        Debug.Log($"🔵 [Cauldron] Consumed {droppedCards.Count} cards from recipe builder");

        if (droppedCards.Count == 0) return;

        // Convert cards to ingredient names
        List<string> ingredientNames = new List<string>();
        foreach (var card in droppedCards)
        {
            if (card.CardData != null)
            {
                ingredientNames.Add(card.CardData.cardName);
                Debug.Log($"🔵 [Cauldron] Added ingredient: {card.CardData.cardName}");
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
            Debug.Log("🔵 [Cauldron] SINGLE CARD MODE detected");
            CardComponent singleCard = droppedCards[0];

            Debug.Log($"🔵 [Cauldron] Card name: {singleCard.CardData.cardName}");
            Debug.Log($"🔵 [Cauldron] Processing recipes count: {singleCard.CardData.processingRecipes?.Count ?? 0}");

            // Check if this card has a cauldron processing recipe
            ProcessingRecipe recipe = null;
            bool fireIsOn = cauldronWorkbench.fireController != null && cauldronWorkbench.fireController.IsFireOn;

            if (singleCard.CardData.processingRecipes != null)
            {
                Debug.Log($"🔵 [Cauldron] processingRecipes list has {singleCard.CardData.processingRecipes.Count} recipes");
                Debug.Log($"🔵 [Cauldron] Fire is currently: {(fireIsOn ? "ON" : "OFF")}");

                // Find ALL cauldron recipes
                var cauldronRecipes = singleCard.CardData.processingRecipes
                    .FindAll(r => r.tool == ProcessingTool.Cauldron);

                if (cauldronRecipes.Count > 0)
                {
                    Debug.Log($"🔵 [Cauldron] Found {cauldronRecipes.Count} cauldron recipe(s)");

                    // Prioritize recipes that match the current fire state
                    if (fireIsOn)
                    {
                        // Fire is ON - prefer recipes that need fire
                        recipe = cauldronRecipes.Find(r => r.needsFire);
                        if (recipe == null)
                        {
                            // No fire-required recipe, use any available
                            recipe = cauldronRecipes[0];
                        }
                        Debug.Log($"🔵 [Cauldron] Fire ON - selected recipe: needsFire={recipe.needsFire}, processedType={recipe.processedResultType}");
                    }
                    else
                    {
                        // Fire is OFF - only use recipes that DON'T need fire
                        recipe = cauldronRecipes.Find(r => !r.needsFire);
                        if (recipe != null)
                        {
                            Debug.Log($"🔵 [Cauldron] Fire OFF - selected recipe: needsFire={recipe.needsFire}, processedType={recipe.processedResultType}");
                        }
                        else
                        {
                            Debug.Log($"🔵 [Cauldron] Fire OFF - all {cauldronRecipes.Count} cauldron recipe(s) require fire");
                        }
                    }
                }
                else
                {
                    Debug.Log($"🔵 [Cauldron] No cauldron recipes found");
                }
            }

            if (recipe != null)
            {
                Debug.Log($"✅ [Cauldron] Using recipe: needsFire={recipe.needsFire}, processedType={recipe.processedResultType}");

                // Check if already processed
                if (singleCard.CardData.processedType != ProcessedType.None &&
                    (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
                {
                    Debug.Log($"⚠️ [Cauldron] Card already processed: {singleCard.CardData.processedType}");
                    ReturnCardToRecipeHolding(singleCard);
                    return;
                }

                // No need to check fire again - we already selected the right recipe based on fire state

                // Valid single-card processing - move to workbench's recipe holding
                Debug.Log($"✅ [Cauldron] Moving card to workbench recipeHoldingParent");
                Debug.Log($"🔵 [Cauldron] recipeHoldingParent null? {cauldronWorkbench.recipeHoldingParent == null}");

                singleCard.transform.SetParent(cauldronWorkbench.recipeHoldingParent, false);
                singleCard.transform.localPosition = Vector3.zero;

                Debug.Log($"🔵 [Cauldron] Card moved. Now calling StartBrewing with null combo and selected recipe");

                if (cauldronWorkbench != null)
                {
                    // Pass the selected recipe to the workbench
                    cauldronWorkbench.StartBrewingWithRecipe(recipe, ingredientNames);
                }
                else
                {
                    Debug.LogError("❌ [Cauldron] cauldronWorkbench is NULL!");
                }
                return;
            }
            else
            {
                Debug.LogWarning($"⚠️ [Cauldron] No cauldron recipe found for {singleCard.CardData.cardName}");
            }
        }

        // 🔹 MULTI-INGREDIENT - Try spell combo lookup
        Debug.Log("🔵 [Cauldron] MULTI-INGREDIENT MODE - looking up spell combo");
        SpellCombo combo = Recipes.GetComboByIngredients(ingredientNames);

        if (combo != null)
        {
            Debug.Log($"✅ [Cauldron] Found spell combo: {combo.SpellName}");
        }
        else
        {
            Debug.Log("⚠️ [Cauldron] No spell combo found - will be treated as failed brew");
        }

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
        Debug.Log($"🔵 [Cauldron] Returning card to recipe builder");
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