using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellCombo
{
    [Header("Core Info")]
    public string SpellName;
    public List<string> Ingredients = new List<string>();
    public SpellTier SpellLevel;

    [Header("Optional Modifiers")]
    public string Intimate;
    public string Spiritual;
    public string Astrological;
    public string Element;
    public string Tool;

    [Header("Result Data")]
    public CardData ResultCard;       // used if the combo points to a specific card
    public Sprite ResultSprite;       // used by CauldronWorkbench for visuals
    public float BrewTime = 5f;       // used by CauldronWorkbench for timing
    public string Description;        // optional flavor text

    [Header("Processing Result Type")]
    public ProcessedType ResultType = ProcessedType.Potion;  // e.g. Potion, Poison, Paste, etc.
}

public class ComboGenerator : MonoBehaviour
{
    public IngredientDatabase ingredientDatabase;
    public RecipeDatabase recipeDatabase;
    public List<SpellCombo> spellCombos = new List<SpellCombo>();

    [Header("Result Card Mapping")]
    public List<CardData> resultCards; // assign in inspector in same order as spell names
    public List<string> spellNames;    // assign matching spell names

    private Dictionary<string, CardData> spellToCardMap;

    List<string> FilterOutUnknowns(List<string> source)
    {
        return source.FindAll(item => !item.ToLower().Contains("unknown"));
    }

    private void Awake()
    {
        // Build the dictionary at runtime
        spellToCardMap = new Dictionary<string, CardData>();
        for (int i = 0; i < Mathf.Min(spellNames.Count, resultCards.Count); i++)
        {
            if (!string.IsNullOrEmpty(spellNames[i]) && resultCards[i] != null)
                spellToCardMap[spellNames[i]] = resultCards[i];
        }
    }

    public CardData GetResultCardForSpell(string spellName)
    {
        if (spellToCardMap != null && spellToCardMap.TryGetValue(spellName, out CardData card))
            return card;
        return null;
    }

    public SpellCombo GenerateCombo(
        string spellName,
        int ingredientCount,
        bool needsIntimate,
        bool needsSpiritual,
        bool needsAstrological,
        bool needsElement,
        bool needsTool,
        string fixedTool = null
        )
    {
        if (recipeDatabase.SpellCombos.Exists(c => c.SpellName == spellName))
        {
            //Debug.Log($"Spell '{spellName}' already exists, skipping generation.");
            return recipeDatabase.GetComboByName(spellName); // optional: return the existing combo
        }

        SpellCombo combo = new SpellCombo();
        combo.SpellName = spellName;

        // 1) Gather all base ingredients into one list
        List<string> allIngredients = new List<string>();
        allIngredients.AddRange(FilterOutUnknowns(ingredientDatabase.Waters));
        allIngredients.AddRange(FilterOutUnknowns(ingredientDatabase.Craftings));

        // 2a) Add random animal ingredients
        List<string> animalChoices = GenerateAnimalOptions();
        allIngredients.AddRange(animalChoices);

        // 2b) Add random mineral ingredients
        List<string> mineralChoices = GenerateMineralOptions();
        allIngredients.AddRange(mineralChoices);

        // 2c) Add random metal ingredients
        List<string> metalChoices = GenerateMetalOptions();
        allIngredients.AddRange(metalChoices);

        // 2d) Add random botanical ingredients
        List<string> botanicalChoices = GenerateBotanicalOptions();
        allIngredients.AddRange(botanicalChoices);

        // 3) Pick N unique random ingredients
        combo.Ingredients = GetUniqueRandomItems(allIngredients, ingredientCount);

        // 4) Extras
        if (needsIntimate && ingredientDatabase.Intimates.Count > 0)
            combo.Intimate = GetRandomItem(FilterOutUnknowns(ingredientDatabase.Intimates));

        if (needsSpiritual && ingredientDatabase.Spirituals.Count > 0)
            combo.Spiritual = GetRandomItem(FilterOutUnknowns(ingredientDatabase.Spirituals));

        if (needsAstrological && ingredientDatabase.Astrologicals.Count > 0)
            combo.Astrological = GetRandomItem(FilterOutUnknowns(ingredientDatabase.Astrologicals));

        if (needsElement && ingredientDatabase.Elements.Count > 0)
            combo.Element = GetRandomItem(FilterOutUnknowns(ingredientDatabase.Elements));

        if (needsTool)
        {
            combo.Tool = !string.IsNullOrEmpty(fixedTool)
                ? fixedTool
                : GetRandomItem(FilterOutUnknowns(ingredientDatabase.Tools));
        }

        //Debug.Log(
        //    $"SpellSetup DB instance: {recipeDatabase.GetInstanceID()}, combos: {recipeDatabase.SpellCombos.Count}"
        //);

        // Add generated combo to runtime database
        //recipeDatabase.AddCombo(combo);   // removed to avoid duplication with action in cauldron.start()
        return combo;

    }

    List<string> GenerateAnimalOptions()
    {
        List<string> animalOptions = new List<string>();
        foreach (var animal in ingredientDatabase.AnimalIngredients)
        {
            if (!animal.AnimalName.ToLower().Contains("unknown"))
            {
                animalOptions.Add(animal.AnimalName);
                foreach (var part in animal.Parts)
                {
                    animalOptions.Add($"{animal.AnimalName} {part}");
                }
            }
        }
        return animalOptions;
    }

    List<string> GenerateMineralOptions()
    {
        List<string> mineralOptions = new List<string>();
        foreach (var mineral in ingredientDatabase.MineralIngredients)
        {
            if (!mineral.MineralName.ToLower().Contains("unknown"))
            {
                mineralOptions.Add(mineral.MineralName);
                foreach (var part in mineral.Parts)
                {
                    mineralOptions.Add($"{mineral.MineralName} {part}");
                }
            }
        }
        return mineralOptions;
    }

    List<string> GenerateMetalOptions()
    {
        List<string> metalOptions = new List<string>();
        foreach (var metal in ingredientDatabase.MetalIngredients)
        {
            if (!metal.MetalName.ToLower().Contains("unknown"))
            {
                metalOptions.Add(metal.MetalName);
                foreach (var part in metal.Parts)
                {
                    metalOptions.Add($"{metal.MetalName} {part}");
                }
            }
        }
        return metalOptions;
    }

    List<string> GenerateBotanicalOptions()
    {
        List<string> botanicalOptions = new List<string>();
        foreach (var botanical in ingredientDatabase.BotanicalIngredients)
        {
            if (!botanical.BotanicalName.ToLower().Contains("unknown"))
            {
                botanicalOptions.Add(botanical.BotanicalName);
                foreach (var part in botanical.Parts)
                {
                    botanicalOptions.Add($"{botanical.BotanicalName} {part}");
                }
            }
        }
        return botanicalOptions;
    }

    List<string> GetUniqueRandomItems(List<string> source, int count)
    {
        List<string> copy = new List<string>(source);
        List<string> result = new List<string>();
        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }
        return result;
    }

    string GetRandomItem(List<string> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public bool IsValidRecipe(List<string> ingredients)
    {
        foreach (var combo in spellCombos)
        {
            if (combo.Ingredients.Count != ingredients.Count) continue;

            // Count ingredients in both lists
            var recipeCount = new Dictionary<string, int>();
            foreach (var ing in combo.Ingredients)
            {
                if (!recipeCount.ContainsKey(ing)) recipeCount[ing] = 0;
                recipeCount[ing]++;
            }

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

            if (matches) return true;
        }

        return false;
    }
}