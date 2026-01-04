using UnityEngine;


public class GameInitialization : MonoBehaviour
{
    private static bool initialized;

    public static RecipeDatabase Recipes { get; private set; }
    public static ComboGenerator Combos { get; private set; }
    public static bool RecipesReady { get; private set; }

    [SerializeField] RecipeDatabase recipeDatabase;
    [SerializeField] ComboGenerator comboGenerator;
    [SerializeField] SpellSetup spellSetup;

    void Awake()
    {
        if (initialized)
            return;

        initialized = true;
        DontDestroyOnLoad(gameObject);

        Recipes = recipeDatabase;
        Combos = comboGenerator;

        // ✅ THIS is the critical order
        Recipes.ClearAll();
        spellSetup.GenerateAllSpells();
        RecipesReady = true;

        //Debug.Log($"✅ Game initialized. Recipes: {Recipes.SpellCombos.Count}");
    }
}