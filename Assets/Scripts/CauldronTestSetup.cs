using UnityEngine;
using System.Collections.Generic;

public class CauldronTestSetup : MonoBehaviour
{

    private void Start()
    {
        if (GameInitialization.Recipes == null)
        {
            Debug.LogError("Recipes not initialized yet!");
            return;
        }

        // Ensure RecipeDatabase is cleared
        GameInitialization.Recipes.ClearAll();

        // Load CardManager from Resources
        CardManager cardManager = Resources.Load<CardManager>("CardManager");
        if (cardManager == null)
        {
            Debug.LogError("CardManager asset not found in Resources!");
            return;
        }

        CardData testCard = cardManager.GetCardByName("TestSpell");
        if (testCard == null)
        {
            Debug.LogError("TestSpell CardData missing in CardManager!");
            return;
        }

        SpellCombo testCombo = new SpellCombo
        {
            SpellName = "TestSpell",
            Ingredients = new List<string> { "Corn", "Chia", "Sandstone" },
            ResultCard = testCard
        };

        GameInitialization.Recipes.AddCombo(testCombo);

        Debug.Log("TestSpell combo registered in RecipeDatabase for test.");
        Debug.Log("When brewed, the cauldron will pull the sprite from CardManager by SpellName.");
    }
}