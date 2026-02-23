using UnityEngine;
using System.Collections.Generic;
using static CardPersistenceManager;

    public enum RecipeStatus
    {
        New,        // learned but not yet brewed
        Brewed      // brewed at least once
    }

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public List<SpellCombo> failedBrews = new List<SpellCombo>();
    public List<SpellCombo> successfulBrews = new List<SpellCombo>();
    public HashSet<string> knownRecipes = new HashSet<string>();

    [System.Serializable]
    public class SavedCauldronBrew
    {
        public bool isBrewing;
        public string spellName;
        public double finishAtGameMinutes;
        public bool fireWasOn;
        public float totalBrewTime;
    }

    // Full runtime card states for persistence
    public List<SavedCardState> savedCards = new List<SavedCardState>();
    public List<CardData> cauldronOutputCards = new List<CardData>();
    public List<SavedPlanterState> savedPlanters = new();
    public List<SavedDryingRackProcess> savedDryingRack = new(); 
    public SavedCauldronBrew savedCauldron;

    public bool testCardsSpawned = false;
    public bool initialRandomCardsSpawned = false;

    [System.Serializable]
    public class RecipeRecord
    {
        public string spellName;
        public RecipeStatus status;
    }

    public List<RecipeRecord> recipeRecords = new List<RecipeRecord>();
    private Dictionary<string, RecipeRecord> recipeMap;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool LearnRecipe(SpellCombo combo, bool markAsNew = true)
    {
        if (Instance == null || combo == null || string.IsNullOrEmpty(combo.SpellName))
            return false;

        Instance.EnsureMap();

        bool isNew = Instance.knownRecipes.Add(combo.SpellName);

        // Ensure a record exists
        if (!Instance.recipeMap.TryGetValue(combo.SpellName, out var rec))
        {
            rec = new RecipeRecord
            {
                spellName = combo.SpellName,
                status = markAsNew ? RecipeStatus.New : RecipeStatus.Brewed
            };

            Instance.recipeRecords.Add(rec);
            Instance.recipeMap[combo.SpellName] = rec;
        }
        else
        {
            if (markAsNew)
            {
                // Don't overwrite Brewed with New
                if (rec.status != RecipeStatus.Brewed)
                    rec.status = RecipeStatus.New;
            }
            else
            {
                // Brewing always upgrades to Brewed
                rec.status = RecipeStatus.Brewed;
            }
        }

        // Keep your “success list” in sync if you still want it
        if (!Instance.successfulBrews.Exists(c => c.SpellName == combo.SpellName))
            Instance.successfulBrews.Add(combo);

        if (isNew)
            Debug.Log($"📖 Learned recipe: {combo.SpellName} ({Instance.GetRecipeStatus(combo.SpellName)})");

        // Refresh book if open
        if (PersistentUIManager.Instance?.successBook != null)
            PersistentUIManager.Instance.successBook.Refresh();

        return isNew;
    }

    public static void MarkRecipeBrewed(string spellName)
    {
        if (Instance == null || string.IsNullOrEmpty(spellName))
            return;

        Instance.EnsureMap();

        // Ensure it’s known + record exists
        if (!Instance.knownRecipes.Contains(spellName))
            Instance.knownRecipes.Add(spellName);

        if (!Instance.recipeMap.TryGetValue(spellName, out var rec))
        {
            rec = new RecipeRecord { spellName = spellName, status = RecipeStatus.Brewed };
            Instance.recipeRecords.Add(rec);
            Instance.recipeMap[spellName] = rec;
        }
        else
        {
            rec.status = RecipeStatus.Brewed;
        }

        if (PersistentUIManager.Instance?.successBook != null)
            PersistentUIManager.Instance.successBook.Refresh();
    }

    private void BuildRecipeMap()
    {
        recipeMap = new Dictionary<string, RecipeRecord>();
        foreach (var r in recipeRecords)
            if (r != null && !string.IsNullOrEmpty(r.spellName))
                recipeMap[r.spellName] = r;
    }

    private void EnsureMap()
    {
        if (recipeMap == null) BuildRecipeMap();
    }

    public RecipeStatus GetRecipeStatus(string spellName)
    {
        EnsureMap();
        if (recipeMap != null && recipeMap.TryGetValue(spellName, out var rec))
            return rec.status;
        return RecipeStatus.New; // default if known but no record yet
    }
}