using UnityEngine;
using System.Collections.Generic;

public class DebugRecipeInjector : MonoBehaviour
{
    [SerializeField] RecipeDatabase recipeDatabase;
    [SerializeField] CardManager cardManager;

    void Awake()
    {
        if (recipeDatabase.GetComboByName("Test Spell") != null)
            return; // already injected

        CardData testCard = cardManager.GetCardByName("Test Spell");
        if (testCard == null)
        {
            Debug.LogError("Test Spell CardData missing!");
            return;
        }

        SpellCombo testCombo = new SpellCombo
        {
            SpellName = "Test Spell",
            Ingredients = new List<string>
            {
                "Corn",
                "Chia",
                "Sandstone"
            },
            ResultCard = testCard
        };

        recipeDatabase.AddCombo(testCombo);

        Debug.Log("🧪 Deterministic Test Spell injected (Corn + Chia + Sandstone)");
    }
}