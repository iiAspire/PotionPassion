using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBuilder : MonoBehaviour
{
    public Transform slotParent; // parent holding the ingredient card slots
    public GameObject validRecipeIndicator; // e.g., a glow or icon

    private List<CardComponent> currentCards = new List<CardComponent>();
    public ComboGenerator comboGenerator;


    public bool AddCard(CardComponent card)
    {
        if (card == null) return false;
        if (currentCards.Count >= 7) return false;
        if (currentCards.Contains(card)) { UpdateRecipeIndicator(); return true; } // already tracked

        currentCards.Add(card);

        // Reparent into the recipe UI
        card.transform.SetParent(slotParent, false);
        card.transform.localPosition = Vector3.zero;

        UpdateRecipeIndicator();
        return true;
    }

    public void RemoveCard(CardComponent card)
    {
        if (card == null) return;
        if (currentCards.Remove(card))
        {
            UpdateRecipeIndicator();
        }
    }

    private void UpdateRecipeIndicator()
    {
        if (validRecipeIndicator == null) return;
        if (GameInitialization.Recipes == null) return;

        var ingredientNames = currentCards.ConvertAll(c => c.CardData.cardName);

        bool isValid =
            GameInitialization.Recipes.GetComboByIngredients(ingredientNames) != null;

        Debug.Log($"Recipe valid: {isValid} | Ingredients: {string.Join(", ", ingredientNames)}");

        validRecipeIndicator.SetActive(isValid);
        if (isValid)
        {
            var animator = validRecipeIndicator.GetComponent<Animator>();
            if (animator != null)
            {
                animator.ResetTrigger("Glow");
                animator.SetTrigger("Glow");
            }
        }
    }

    public List<CardComponent> ConsumeAll()
    {
        var consumed = new List<CardComponent>(currentCards);

        foreach (var card in consumed)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        currentCards.Clear();

        if (validRecipeIndicator != null)
            validRecipeIndicator.SetActive(false); // 👈 hard off

        return consumed;
    }
}
