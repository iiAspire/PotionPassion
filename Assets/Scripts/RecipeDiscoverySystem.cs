using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RecipeDiscoverySystem : MonoBehaviour
{
    public static RecipeDiscoverySystem Instance;

    [Header("Chance Settings")]
    public float baseChance = 0.1f;
    public float chancePerFailure = 0.05f;

    private float storedChance;

    [SerializeField] private LearningRitualUI ritual;
    [SerializeField] private SuccessfulBrewPanelController successPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            var combo = GameInitialization.Recipes.SpellCombos[0];
            StartCoroutine(LearningSequence(combo));
        }
    }

    //#if UNITY_EDITOR
    //[ContextMenu("TEST Discovery")]
    //private void TestDiscovery()
    //{
    //    Debug.Log("TEST Discovery fired");
    //    var combo = GameInitialization.Recipes.SpellCombos[0];
    //    StartCoroutine(LearningSequence(combo));
    //}
    //#endif

    // 🔹 CALLED FROM CAULDRONWORKBENCH
    public void OnSuccessfulBrew(SpellCombo brewedCombo)
    {
        float chance = baseChance + storedChance;

        if (Random.value > chance)
        {
            storedChance += chancePerFailure;
            return;
        }

        storedChance = 0f;

        SpellCombo learnable = FindLearnableRecipe(brewedCombo);
        if (learnable == null)
        {
            storedChance += chancePerFailure;
            return;
        }

        StartCoroutine(LearningSequence(learnable));
    }

    // 🔹 STEP 3: INGREDIENT-SHARED RECIPE SELECTION
    private SpellCombo FindLearnableRecipe(SpellCombo brewed)
    {
        foreach (var combo in GameInitialization.Recipes.AllCombos)
        {
            // Already known → skip
            if (GameData.Instance.knownRecipes.Contains(combo.SpellName))
                continue;

            // Shares ingredient?
            foreach (string ingredient in combo.Ingredients)
            {
                if (brewed.Ingredients.Contains(ingredient))
                    return combo;
            }
        }

        return null;
    }

    private IEnumerator LearningSequence(SpellCombo combo)
    {
        if (ritual == null)
        {
            Debug.LogError("LearningRitualUI not assigned to RecipeDiscoverySystem");
            yield break;
        }

        yield return ritual.Play(combo);

        // 1️⃣ Mark as known
        GameData.Instance.knownRecipes.Add(combo.SpellName);

        // 2️⃣ Add to success list
        if (!GameData.Instance.successfulBrews.Exists(c => c.SpellName == combo.SpellName))
            GameData.Instance.successfulBrews.Add(combo);

        // 3️⃣ Show panel
        if (successPanel == null)
        {
            Debug.LogError("SuccessfulBrewPanelController not assigned");
            yield break;
        }

        successPanel.ShowSuccessfulBrews();
    }
}