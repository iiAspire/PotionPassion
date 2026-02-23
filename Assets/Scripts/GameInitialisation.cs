using UnityEngine;
using System.Collections;


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

        StartCoroutine(InitializeAfterGameData());
    }

    private IEnumerator InitializeAfterGameData()
    {
        // Wait until GameData singleton exists
        while (GameData.Instance == null)
            yield return null;

        spellSetup.GenerateAllSpells();
        spellSetup.InitializeCookbookWithBasicRecipes();

        RecipesReady = true;

        Debug.Log("✅ Game initialized with cookbook.");
    }
}