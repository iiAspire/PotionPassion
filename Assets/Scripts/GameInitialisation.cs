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

        Debug.Log($"ComboGenerator instance ID: {comboGenerator.GetInstanceID()}");
        Debug.Log($"spellNames count: {comboGenerator.spellNames.Count}");
        Debug.Log($"resultCards count: {comboGenerator.resultCards.Count}");

        initialized = true;
        DontDestroyOnLoad(gameObject);

        Recipes = recipeDatabase;
        Combos = comboGenerator;

        // ✅ THIS is the critical order
        Recipes.ClearAll();
        Combos.BuildResultMap();
        spellSetup.GenerateAllSpells();
        RecipesReady = true;

        //Debug.Log($"✅ Game initialized. Recipes: {Recipes.SpellCombos.Count}");
    }
}