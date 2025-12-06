using System.Collections.Generic;
using UnityEngine;

public enum SpellTier
{
    Basic,
    Intermediate,
    Advanced
}

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Spellcraft/RecipeDatabase")]
public class RecipeDatabase : ScriptableObject
{
  
    // Stores all combos generated at the start of the game
    public List<SpellCombo> SpellCombos = new List<SpellCombo>();

    // Stores failed or invalid combos encountered during gameplay
    public List<SpellCombo> FailedCombos = new List<SpellCombo>();

    // Stores successful or valid combos encountered during gameplay
    public List<SpellCombo> SuccessfulCombos = new List<SpellCombo>();

    // Expose FailedCombos publicly
    public List<SpellCombo> FailedRecipes => FailedCombos;

    [Header("Cauldron Brew Times (IN-GAME MINUTES)")]
    public float failedBrewMinutes = 15f;
    public float basicBrewMinutes = 60f;
    public float intermediateBrewMinutes = 120f;
    public float advancedBrewMinutes = 240f;

    public float GameMinutesToSeconds(float gameMinutes)
    {
        return gameMinutes / TimeManager.MinutesPerRealSecond;
    }

    public float GetBrewTimeSeconds(SpellCombo combo, bool failed)
    {
        if (failed || combo == null)
            return GameMinutesToSeconds(failedBrewMinutes);

        return combo.SpellLevel switch
        {
            SpellTier.Basic => GameMinutesToSeconds(basicBrewMinutes),
            SpellTier.Intermediate => GameMinutesToSeconds(intermediateBrewMinutes),
            SpellTier.Advanced => GameMinutesToSeconds(advancedBrewMinutes),
            _ => GameMinutesToSeconds(basicBrewMinutes)
        };
    }

    // Add a new combo to the database
    public void AddCombo(SpellCombo combo)
    {
        if (!SpellCombos.Contains(combo))
            SpellCombos.Add(combo);
    }

    // Add a failed combo for later review
    public void AddFailedCombo(SpellCombo combo)
    {
        if (combo == null) return;

        FailedCombos.Add(combo);
        Debug.Log($"❌ Logged failed combo: {combo.SpellName}");
    }

    public void AddSuccessfulCombo(SpellCombo combo)
    {
        if (combo == null) return;
        SuccessfulCombos.Add(combo);
        Debug.Log($"✅ Logged successful combo: {combo.SpellName}");
    }

    // Find a combo by matching ingredients
    public SpellCombo GetComboByIngredients(List<string> ingredients)
    {
        foreach (var combo in SpellCombos)
        {
            if (combo.Ingredients.Count != ingredients.Count)
                continue;

            // Count ingredients in the recipe
            var recipeCount = new Dictionary<string, int>();
            foreach (var ing in combo.Ingredients)
            {
                if (!recipeCount.ContainsKey(ing)) recipeCount[ing] = 0;
                recipeCount[ing]++;
            }

            // Count ingredients in the input
            var inputCount = new Dictionary<string, int>();
            foreach (var ing in ingredients)
            {
                if (!inputCount.ContainsKey(ing)) inputCount[ing] = 0;
                inputCount[ing]++;
            }

            bool matches = true;
            foreach (var kvp in recipeCount)
            {
                if (!inputCount.ContainsKey(kvp.Key) || inputCount[kvp.Key] != kvp.Value)
                {
                    matches = false;
                    break;
                }
            }

            if (matches) return combo;
        }

        return null; // no match found
    }

    // Lookup by name
    public SpellCombo GetComboByName(string spellName)
    {
        return SpellCombos.Find(c => c.SpellName == spellName);
    }

    public CardData GetCardDataByName(string spellName)
    {
        foreach (var combo in SpellCombos)
        {
            if (combo.SpellName == spellName && combo.ResultCard != null)
                return combo.ResultCard;
        }
        return null;
    }

    // Clear the recipe database at new game start
    public void ClearAll()
    {
        SpellCombos.Clear();
        SuccessfulCombos.Clear();
        FailedCombos.Clear();
    }
}