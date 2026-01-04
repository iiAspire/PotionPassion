using System;
using System.Collections.Generic;
using UnityEngine;
using static IngredientDatabase;
using System.Linq;

public enum ModifierType { None, Intimate, Spiritual, Astrological, Element }

public class SpellSetup : MonoBehaviour
{
    public RecipeDatabase recipeDatabase;
    public IngredientDatabase ingredientDatabase;
    public ComboGenerator comboGenerator;

    // Ingredient category lists
    private List<MetalIngredient> Metals;
    private List<BotanicalIngredient> Botanicals;
    private List<AnimalIngredient> Animals;
    private List<string> Waters;
    private List<MineralIngredient> Minerals;
    private List<string> Craftings;

    // Modifier-related ingredient lists
    private List<string> Intimates;
    private List<string> Spirituals;
    private List<string> Astrologicals;
    private List<string> Elements;
    private List<string> Tools;

    [SerializeField]
    private bool generateTestSpell = true; // ✅ toggle in inspector (for now)

    private float GameMinutesToSeconds(float gameMinutes)
    {
        return gameMinutes / TimeManager.MinutesPerRealSecond;
    }

    private float GetBrewTimeInSeconds(string spellLevel, bool failed)
    {
        if (failed)
            return GameMinutesToSeconds(15f); // failed brew

        return spellLevel switch
        {
            "Basic" => GameMinutesToSeconds(60f),
            "Intermediate" => GameMinutesToSeconds(120f),
            "Advanced" => GameMinutesToSeconds(240f),
            _ => GameMinutesToSeconds(60f)
        };
    }

    private void LoadIngredientData()
    {
        if (ingredientDatabase == null)
        {
            Debug.LogError("❌ IngredientDatabase not assigned to SpellSetup!");
            return;
        }

        Botanicals = new List<BotanicalIngredient>(ingredientDatabase.BotanicalIngredients);
        Animals = new List<AnimalIngredient>(ingredientDatabase.AnimalIngredients);
        Minerals = new List<MineralIngredient>(ingredientDatabase.MineralIngredients);
        Waters = new List<string>(ingredientDatabase.GetIngredientsByCategory("Water"));
        Metals = new List<MetalIngredient>(ingredientDatabase.MetalIngredients);
        Craftings = new List<string>(ingredientDatabase.GetIngredientsByCategory("Crafting"));

        Intimates = new List<string>(ingredientDatabase.GetIngredientsByCategory("Intimate"));
        Spirituals = new List<string>(ingredientDatabase.GetIngredientsByCategory("Spiritual"));
        Astrologicals = new List<string>(ingredientDatabase.GetIngredientsByCategory("Astrological"));
        Elements = new List<string>(ingredientDatabase.GetIngredientsByCategory("Element"));
        Tools = new List<string>(ingredientDatabase.GetIngredientsByCategory("Tool"));

        //Debug.Log("✅ SpellSetup ingredient lists loaded from IngredientDatabase.");
    }

    // Track used ingredients per type/category to enforce uniqueness
    private Dictionary<string, HashSet<string>> usedMetals = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<string>> usedBotanicals = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<string>> usedAnimals = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<string>> usedWaters = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<string>> usedMinerals = new Dictionary<string, HashSet<string>>();
    private Dictionary<string, HashSet<string>> usedCraftings = new Dictionary<string, HashSet<string>>();

    private List<SpellCombo> generatedCombos = new List<SpellCombo>();

    private void InitUsedSets()
    {
        usedMetals.Clear();
        usedBotanicals.Clear();
        usedAnimals.Clear();
        usedWaters.Clear();
        usedMinerals.Clear();
        usedCraftings.Clear();
        generatedCombos.Clear();
    }

    public void GenerateAllSpells()
    {
        LoadIngredientData();
        InitUsedSets();

        // BASIC SPELLS
        AddSpell("Protection", "Basic", 0, 1, 1, 1, 1, 0, ModifierType.Element, "", "Basic");
        AddSpell("Success", "Basic", 0, 0, 1, 1, 1, 1, ModifierType.Spiritual, "", "Basic");
        AddSpell("Insomnia", "Basic", 1, 1, 1, 1, 0, 0, ModifierType.Intimate, "", "Basic");
        AddSpell("Happy Home", "Basic", 1, 0, 1, 1, 0, 1, ModifierType.Spiritual, "", "Basic");
        AddSpell("Health", "Basic", 0, 1, 1, 1, 1, 0, ModifierType.Intimate, "", "Basic");
        AddSpell("Luck", "Basic", 0, 1, 0, 1, 1, 1, ModifierType.Element, "", "Basic");
        AddSpell("Locator", "Basic", 1, 1, 0, 1, 0, 1, ModifierType.Intimate, "", "Basic");

        // INTERMEDIATE SPELLS
        AddSpell("Cleansing", "Intermediate", 0, 2, 1, 1, 1, 0, ModifierType.Element, "Candle", "Intermediate");
        AddSpell("Justice", "Intermediate", 0, 0, 2, 1, 1, 1, ModifierType.Spiritual, "Paper", "Intermediate");
        AddSpell("Divination", "Intermediate", 1, 2, 1, 1, 0, 0, ModifierType.Intimate, "Crystal Ball", "Intermediate");
        AddSpell("Persuasion", "Intermediate", 1, 0, 2, 1, 0, 1, ModifierType.Spiritual, "Horseshoe", "Intermediate");
        AddSpell("Healing", "Intermediate", 0, 1, 2, 1, 0, 1, ModifierType.Intimate, "Ring", "Intermediate");
        AddSpell("Banishing", "Intermediate", 0, 2, 0, 1, 1, 1, ModifierType.Element, "Doll", "Intermediate");
        AddSpell("Weather", "Intermediate", 1, 2, 0, 1, 0, 1, ModifierType.Intimate, "Charm Bag", "Intermediate");

        // ADVANCED SPELLS
        AddSpell("Necromancy", "Advanced", 0, 2, 2, 1, 1, 0, ModifierType.Astrological, "Pentacle", "Advanced");
        AddSpell("Flying", "Advanced", 0, 0, 2, 1, 1, 2, ModifierType.Astrological, "Broomstick", "Advanced");
        AddSpell("Love", "Advanced", 1, 2, 2, 1, 0, 0, ModifierType.Astrological, "Pendant", "Advanced");
        AddSpell("Psychic Power", "Advanced", 1, 0, 2, 1, 0, 2, ModifierType.Astrological, "Mirror", "Advanced");
        AddSpell("Spirit Summoning", "Advanced", 0, 2, 2, 1, 0, 1, ModifierType.Astrological, "Book", "Advanced");
        AddSpell("Youth", "Advanced", 0, 2, 0, 1, 1, 2, ModifierType.Astrological, "Amulet", "Advanced");
        AddSpell("Fertility", "Advanced", 1, 2, 0, 1, 0, 2, ModifierType.Astrological,"Bell", "Advanced");

        //Debug.Log($"SpellSetup: generated {generatedCombos.Count} combos. RecipeDB spells: {recipeDatabase?.SpellCombos?.Count ?? 0}");

        //Debug.Log(
        //    $"🧪 SpellSetup DB instance: {recipeDatabase.GetInstanceID()}, combos: {recipeDatabase.SpellCombos.Count}"
        //);
    }

    public List<SpellCombo> GetAllCombos()
    {
        return generatedCombos; // now actually returns the combos you created
    }

    private void AddSpell(
        string name,
        string category,
        int metalCount, int botanicalCount, int animalCount, int waterCount, int mineralCount, int craftingCount,
        ModifierType modifierType,
        string toolName,
        string spellLevel
        )
    {
        SpellCombo combo = new SpellCombo();
        combo.SpellName = name;
        combo.SpellLevel = Enum.Parse<SpellTier>(spellLevel);
        combo.Ingredients = new List<string>();


        // Pick unique ingredients per category
        combo.Ingredients.AddRange(PickUniqueStrings(Metals, usedMetals, category, metalCount, spellLevel));
        combo.Ingredients.AddRange(PickUniqueStrings(Botanicals, usedBotanicals, category, botanicalCount, spellLevel));
        combo.Ingredients.AddRange(PickUniqueStrings(Animals, usedAnimals, category, animalCount, spellLevel));
        combo.Ingredients.AddRange(PickUniqueStrings(Waters, usedWaters, category, waterCount, spellLevel));
        combo.Ingredients.AddRange(PickUniqueStrings(Minerals, usedMinerals, category, mineralCount, spellLevel));
        combo.Ingredients.AddRange(PickUniqueStrings(Craftings, usedCraftings, category, craftingCount, spellLevel));

        // Assign modifier explicitly
        switch (modifierType)
        {
            case ModifierType.Intimate: combo.Intimate = PickRandom(Intimates); break;
            case ModifierType.Spiritual: combo.Spiritual = PickRandom(Spirituals); break;
            case ModifierType.Astrological: combo.Astrological = PickRandom(Astrologicals); break;
            case ModifierType.Element: combo.Element = PickRandom(Elements); break;
        }

        // Assign tool if provided
        combo.Tool = !string.IsNullOrEmpty(toolName) ? toolName : null;

        // Assign result card
        combo.ResultCard = comboGenerator?.GetResultCardForSpell(name);

        // Add to database
        generatedCombos.Add(combo);
        recipeDatabase.AddCombo(combo);

        //Debug.Log($"Added {category} spell: {name} ({combo.Ingredients.Count} ingredients, Modifier: {modifierType}, Tool: {combo.Tool})");
    }

    private List<string> PickUniqueStrings<T>(
    List<T> source,
    Dictionary<string, HashSet<string>> used,
    string category,
    int count,
    string spellLevel = "Basic")
    {
        if (!used.ContainsKey(category))
            used[category] = new HashSet<string>();

        List<string> picked = new();
        List<T> available = new();

        // Filter available items
        foreach (var item in source)
        {
            string name = item switch
            {
                MetalIngredient m => m.MetalName,
                MineralIngredient mi => mi.MineralName,
                AnimalIngredient a => a.AnimalName,
                BotanicalIngredient b => b.BotanicalName,
                string s => s,
                _ => item.ToString()
            };

            if (!used[category].Contains(name) && !name.ToLower().StartsWith("unknown"))
                available.Add(item);
        }

        //Debug.Log($"[{category}] Requesting {count} from {typeof(T).Name} → available: {available.Count}");

        // Pick up to requested count
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var chosen = available[idx];
            available.RemoveAt(idx);

            string ingredientName = chosen switch
            {
                MetalIngredient m => PickMetalOrMineralPart(m, spellLevel),
                MineralIngredient mi => PickMetalOrMineralPart(mi, spellLevel),
                AnimalIngredient a => PickAnimalPart(a),
                BotanicalIngredient b => PickBotanicalPart(b),
                string s => s,
                _ => chosen.ToString()
            };

            picked.Add(ingredientName);

            string baseName = chosen switch
            {
                MetalIngredient m => m.MetalName,
                MineralIngredient mi => mi.MineralName,
                AnimalIngredient a => a.AnimalName,
                BotanicalIngredient b => b.BotanicalName,
                string s => s,
                _ => chosen.ToString()
            };
            used[category].Add(baseName);
        }

        return picked;
    }

    private string PickMetalOrMineralPart<T>(T ingredient, string spellLevel) where T : class
    {
        List<string> parts = ingredient switch
        {
            MetalIngredient m => m.Parts,
            MineralIngredient mi => mi.Parts,
            _ => null
        };

        if (parts == null || parts.Count == 0)
            return ingredient switch
            {
                MetalIngredient m => m.MetalName,
                MineralIngredient mi => mi.MineralName,
                _ => ingredient.ToString()
            };

        List<string> allowedParts = spellLevel switch
        {
            "Basic" => parts.FindAll(p => p.Contains("Chip") || p.Contains("Dust")),
            "Intermediate" => parts.FindAll(p => p.Contains("Lump") || p.Contains("Hunk")),
            _ => parts
        };

        if (allowedParts.Count == 0) allowedParts = parts;

        string chosenPart = allowedParts[UnityEngine.Random.Range(0, allowedParts.Count)];

        string baseName = ingredient switch
        {
            MetalIngredient m => m.MetalName,
            MineralIngredient mi => mi.MineralName,
            _ => ingredient.ToString()
        };

        if (baseName.ToLower() != "dirt")
        {
            return $"{chosenPart} of {baseName}";
        }

        else
            return $"{chosenPart} {baseName}";
    }

    

    private string PickAnimalPart(AnimalIngredient animal)
    {
        if (animal.Parts == null || animal.Parts.Count == 0)
            return animal.AnimalName;

        string part = animal.Parts[UnityEngine.Random.Range(0, animal.Parts.Count)];
        return $"{part} of {animal.AnimalName}";
    }

    private string PickBotanicalPart(BotanicalIngredient botanical)
    {
        if (botanical.Parts == null || botanical.Parts.Count == 0)
            return botanical.BotanicalName;

        string part = botanical.Parts[UnityEngine.Random.Range(0, botanical.Parts.Count)];
        return $"{part} of {botanical.BotanicalName}";
    }

    private string PickRandom(List<string> source)
    {
        List<string> filtered = source.FindAll(x => !x.ToLower().StartsWith("unknown"));
        if (filtered.Count == 0) return null;
        return filtered[UnityEngine.Random.Range(0, filtered.Count)];
    }

    //public void SpawnTestCards()
    //{
    //    //Debug.Log("⚠ Spawning test spells");

    //    // Example: ensure database exists
    //    if (recipeDatabase == null)
    //    {
    //        Debug.LogError("RecipeDatabase missing in SpellSetup");
    //        return;
    //    }

    //    // Add whatever test combos you want here
    //    // (or call GenerateAllSpells if that's your intention)
    //}
}
