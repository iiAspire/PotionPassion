using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameData;

public class CardPersistenceManager : MonoBehaviour
{
    public static CardPersistenceManager Instance;

    [Header("Inventory Parents (direct children = cards)")]
    public Transform playerInventoryParent;
    public Transform ingredientsInventoryParent;
    public Transform recipeHoldingParent;    // the tray near the cauldron
    public Transform cauldronOutputParent;   // where brew result sits
    public Transform saleInventoryParent;

    [Header("Lookup")]
    public CardManager cardManager;

    public bool IsLoaded { get; private set; } = false;
    public bool IsRebinding { get; private set; }
    private bool canSave = false;
    private bool cauldronRestoredThisScene = false;
    public bool CardsRestored { get; private set; } = false;

    [System.Serializable]
    public class SavedPlanterState
    {
        public string planterID;
        public string plantedCardName;
        public float remainingGrowTime;
        public bool isActive;
        public double savedAtGameMinutes;
    }

    private WorkbenchStation FindWorkbenchByTool(string toolName)
    {
        foreach (var station in FindObjectsOfType<WorkbenchStation>())
        {
            if (station.tool.ToString() == toolName)
                return station;
        }
        return null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameData.Instance == null)
            return;

        cauldronRestoredThisScene = false;
        CardsRestored = false;

        // ⚠ Do NOT allow save yet
        canSave = false;

        StartCoroutine(EnableSaveNextFrame());
    }

    IEnumerator EnableSaveNextFrame()
    {
        yield return null; // wait one frame
        canSave = true;
        IsLoaded = true;

        Debug.Log("✅ Persistence ready — saving allowed");
    }
    private void LogParent(string label, Transform t)
    {
        if (t == null)
        {
            Debug.Log($"[BIND] {label}: NULL");
            return;
        }

        // Unity-safe checks only
        bool alive = t.gameObject != null;
        bool active = alive && t.gameObject.activeInHierarchy;

        Debug.Log($"[BIND] {label}: alive={alive}, active={active}");
    }

    public void BindSceneParents(
        Transform player,
        Transform ingredients,
        Transform recipe,
        Transform cauldron,
        Transform sale
    )
    {
        // Local helper: Unity-safe destroyed check
        static bool IsAlive(Transform t) => t != null && t.gameObject != null;

        // Log without touching unsafe properties
        Debug.Log("[BindSceneParents] BEGIN");

        LogParent("PlayerInventory", player);
        LogParent("IngredientsInventory", ingredients);
        LogParent("RecipeHolding", recipe);
        LogParent("CauldronOutput", cauldron);
        LogParent("SaleInventory", sale);

        // Assign only if alive
        playerInventoryParent = IsAlive(player) ? player : null;
        ingredientsInventoryParent = IsAlive(ingredients) ? ingredients : null;
        recipeHoldingParent = IsAlive(recipe) ? recipe : null;
        cauldronOutputParent = IsAlive(cauldron) ? cauldron : null;
        saleInventoryParent = IsAlive(sale) ? sale : null;

        // Hard assertions (do NOT crash)
        if (!playerInventoryParent)
            Debug.LogError("❌ PlayerInventory missing or destroyed before binding");

        if (!ingredientsInventoryParent)
            Debug.LogError("❌ IngredientsInventory missing or destroyed before binding");

        if (!saleInventoryParent)
            Debug.LogError("❌ SaleInventory missing or destroyed before binding");

        Debug.Log("[BindSceneParents] END");
    }

    // Call this BEFORE changing scenes
    public void SaveAllCards()
    {
        Debug.Log($"[SAVE] Ingredients parent instanceID = {ingredientsInventoryParent.GetInstanceID()}");
        foreach (Transform child in ingredientsInventoryParent)
        {
            Debug.Log($"  child: {child.name}");
        }

        if (!SceneLoadManager.Instance ||
            SceneLoadManager.Instance.IsTransitioning &&
            !SceneManager.GetActiveScene().isLoaded)
        {
            Debug.LogError("❌ SaveAllCards blocked — parent rebinding already occurred");
            return;
        }

        // Allow save if at least once loaded
        if (!IsLoaded && GameData.Instance.savedCards.Count == 0)
            return;

        if (GameData.Instance == null)
            return;

        // If this scene has no cauldron, preserve any existing CauldronOutput
        // entries from the previous scene so they don't get wiped.
        List<SavedCardState> preservedCauldron = null;

        if (cauldronOutputParent == null)
        {
            preservedCauldron = GameData.Instance.savedCards
                .FindAll(s => s.container == CardContainer.CauldronOutput);
        }

        List<SavedCardState> preservedWorkbenchCards = null;
        if (GameData.Instance.savedCards != null)
        {
            preservedWorkbenchCards = GameData.Instance.savedCards
                .FindAll(s => s.container == CardContainer.Workbench);

            if (preservedWorkbenchCards.Count > 0)
            {
                Debug.Log($"[SAVE] Preserving {preservedWorkbenchCards.Count} workbench cards from PersistIfPaused");
            }
        }

        GameData.Instance.savedCards.Clear();

        SaveFromParent(playerInventoryParent, CardContainer.PlayerInventory);
        SaveFromParent(ingredientsInventoryParent, CardContainer.IngredientsInventory);
        SaveFromParent(recipeHoldingParent, CardContainer.RecipeHolding);
        SaveFromParent(cauldronOutputParent, CardContainer.CauldronOutput);
        SaveFromParent(saleInventoryParent, CardContainer.SaleInventory);

        var cauldron = FindObjectOfType<CauldronWorkbench>();

        if (cauldron != null)
        {
            // We are in a scene that actually HAS a cauldron
            if (cauldron.IsBrewing)
            {
                GameData.Instance.savedCauldron = new SavedCauldronBrew
                {
                    isBrewing = true,
                    spellName = cauldron.ActiveSpellName,
                    finishTimeUtcOa = cauldron.FinishTimeUtcOa,   // absolute UTC finish time
                    fireWasOn = cauldron.FireWasOn,
                    totalBrewTime = cauldron.TotalBrewTime
                };
                Debug.Log($"[SAVE] Cauldron brewing '{cauldron.ActiveSpellName}' until OA={cauldron.FinishTimeUtcOa}");
            }
            else
            {
                // Cauldron exists but isn’t brewing → nothing to resume
                GameData.Instance.savedCauldron = null;
                Debug.Log("[SAVE] Cauldron present but not brewing — cleared savedCauldron");
            }
        }
        else
        {
            // ❗ No cauldron in this scene (e.g. PlanterScene)
            // DO NOT touch savedCauldron – we’re just passing through another room.
            Debug.Log("[SAVE] No cauldron in this scene — keeping existing savedCauldron as-is");
        }

        var plantersInScene = FindObjectsOfType<PlanterSlot>();

        // Only update planter saves if this scene actually HAS planters
        if (plantersInScene.Length > 0)
        {
            Debug.Log($"[SAVE] Scene has {plantersInScene.Length} planters, updating planter saves");

            // Save planter outputs
            foreach (var planter in plantersInScene)
            {
                SavePlanterOutput(planter.outputAnchor, planter.planterID);
            }

            // Clear and rebuild planter states
            GameData.Instance.savedPlanters.Clear();

            foreach (var planter in plantersInScene)
            {
                if (!planter.IsActive)
                    continue;

                Debug.Log($"💾 Saving planter '{planter.planterID}': seed={planter.CurrentSeedName}, remaining={planter.RemainingTime}");

                GameData.Instance.savedPlanters.Add(new SavedPlanterState
                {
                    planterID = planter.planterID,
                    plantedCardName = planter.CurrentSeedName,
                    remainingGrowTime = planter.RemainingTime,
                    isActive = true,
                    savedAtGameMinutes = TimeManager.TotalGameMinutes
                });
            }
        }
        else
        {
            Debug.Log("[SAVE] No planters in this scene, keeping existing planter saves");
        }

        // Re-add preserved cauldron outputs if this scene had none
        if (preservedCauldron != null && preservedCauldron.Count > 0)
        {
            GameData.Instance.savedCards.AddRange(preservedCauldron);
        }

        if (preservedWorkbenchCards != null && preservedWorkbenchCards.Count > 0)
        {
            GameData.Instance.savedCards.AddRange(preservedWorkbenchCards);
            Debug.Log($"[SAVE] Re-added {preservedWorkbenchCards.Count} workbench cards");
        }
    }

    void SavePlanterOutput(Transform outputAnchor, string planterID)
    {
        int order = 0;

        foreach (CardComponent card in outputAnchor.GetComponentsInChildren<CardComponent>(true))
        {
            CardData data = card.CardData;
            if (data == null) continue;

            SavedCardState s = new SavedCardState
            {
                runtimeID = card.runtimeID,
                cardName = data.cardName,
                baseName = data.baseName,

                itemType = data.itemType,
                processedType = data.processedType,
                partType = data.partType,
                quantityType = data.quantityType,

                Toxin = data.Toxin,
                Neutral = data.Neutral,
                Antidote = data.Antidote,
                Sweet = data.Sweet,
                Bitter = data.Bitter,
                Salty = data.Salty,
                Spicy = data.Spicy,
                Flowery = data.Flowery,
                Umami = data.Umami,
                Edible = data.Edible,
                Incinerates = data.Incinerates,
                Smoulders = data.Smoulders,

                container = CardContainer.PlanterOutput,
                planterID = planterID,
                orderInParent = order++
            };

            GameData.Instance.savedCards.Add(s);
        }
    }

    void SaveFromParent(Transform parent, CardContainer container)
    {
        // For each parent, we’ll assign a stable key per stack.
        Dictionary<CardStack, string> stackKeys = new Dictionary<CardStack, string>();
        int stackIndexCounter = 0;

        int order = 0;

        foreach (CardComponent card in parent.GetComponentsInChildren<CardComponent>(true))
        {
            // Skip cards that belong to a different logical container (safety)
            if (card == null || card.CardData == null)
                continue;

            Transform t = card.transform;

            CardData data = card.CardData;

            // 🔽 NEW: figure out if this card is part of a CardStack (for PlayerInventory)
            bool isStacked = false;
            string stackKey = null;
            int indexInStack = 0;

            if (container == CardContainer.PlayerInventory)
            {
                CardStack stack = card.GetComponentInParent<CardStack>();
                if (stack != null && stack.transform.parent == parent)
                {
                    isStacked = true;

                    // assign a unique key to this stack within this parent/container
                    if (!stackKeys.TryGetValue(stack, out stackKey))
                    {
                        // e.g. "PlayerInventory_0", "PlayerInventory_1", etc.
                        stackKey = container.ToString() + "_" + stackIndexCounter++;
                        stackKeys[stack] = stackKey;
                    }

                    // position of this card inside the stack (bottom → top)
                    int idx = stack.cards.IndexOf(card);
                    indexInStack = Mathf.Max(0, idx);
                }
            }

            SavedCardState s = new SavedCardState
            {
                runtimeID = card.runtimeID,

                cardName = data.cardName,
                baseName = data.baseName,

                itemType = data.itemType,
                processedType = data.processedType,
                partType = data.partType,
                quantityType = data.quantityType,

                Toxin = data.Toxin,
                Neutral = data.Neutral,
                Antidote = data.Antidote,
                Sweet = data.Sweet,
                Bitter = data.Bitter,
                Salty = data.Salty,
                Spicy = data.Spicy,
                Flowery = data.Flowery,
                Umami = data.Umami,
                Edible = data.Edible,
                Incinerates = data.Incinerates,
                Smoulders = data.Smoulders,

                container = container,
                orderInParent = order++,

                // 🔽 NEW: stack data
                isStacked = isStacked,
                stackKey = stackKey,
                indexInStack = indexInStack
            };

            GameData.Instance.savedCards.Add(s);
        }

    }

    void SaveSingleCard(CardComponent card, CardContainer container,
                        int order, int stackID, int indexInStack, bool isRoot)
    {
        CardData data = card.CardData;
        if (data == null) return;

        SavedCardState s = new SavedCardState
        {
            runtimeID = card.runtimeID,

            cardName = data.cardName,
            baseName = data.baseName,
            itemType = data.itemType,
            processedType = data.processedType,
            partType = data.partType,
            quantityType = data.quantityType,

            Toxin = data.Toxin,
            Neutral = data.Neutral,
            Antidote = data.Antidote,
            Sweet = data.Sweet,
            Bitter = data.Bitter,
            Salty = data.Salty,
            Spicy = data.Spicy,
            Flowery = data.Flowery,
            Umami = data.Umami,
            Edible = data.Edible,
            Incinerates = data.Incinerates,
            Smoulders = data.Smoulders,

            container = container,
            orderInParent = order,

            // new stacking info
            stackID = stackID,
            indexInStack = indexInStack,
            isStackRoot = isRoot
        };

        GameData.Instance.savedCards.Add(s);
    }

    public void LoadAllCards()
    {
        Debug.Log("🔥 LoadAllCards START");

        if (GameData.Instance == null || cardManager == null)
            return;

        IsLoaded = false;

        // Clear existing children
        ClearParent(playerInventoryParent);
        ClearParent(ingredientsInventoryParent);
        ClearParent(recipeHoldingParent);
        ClearParent(cauldronOutputParent);
        ClearParent(saleInventoryParent);

        var savedCards = GameData.Instance.savedCards;
        if (savedCards == null || savedCards.Count == 0)
        {
            //Debug.Log("[LoadAllCards] No saved cards to load.");
            IsLoaded = true;
            return;
        }

        // 🔽 Group all stacked cards by stackKey
        Dictionary<string, List<SavedCardState>> stackGroups = new Dictionary<string, List<SavedCardState>>();
        foreach (var state in savedCards)
        {
            if (state.isStacked && !string.IsNullOrEmpty(state.stackKey))
            {
                if (!stackGroups.TryGetValue(state.stackKey, out var list))
                {
                    list = new List<SavedCardState>();
                    stackGroups[state.stackKey] = list;
                }
                list.Add(state);
            }
        }

        // 1) Spawn all NON-stacked cards directly
        foreach (var state in savedCards)
        {
            if (state.isStacked && !string.IsNullOrEmpty(state.stackKey))
                continue; // will be handled in stack pass

            Transform parent = GetParentForContainer(state.container, state.planterID);
            if (parent == null) continue;

            //Debug.Log($"xxxLoading card → {state.cardName} in {state.container}");
            SpawnCardFromState(state, parent);
        }

        // 2) Spawn stacks
        foreach (var kvp in stackGroups)
        {
            string stackKey = kvp.Key;
            List<SavedCardState> list = kvp.Value;

            // sort by indexInStack so 0 = bottom, last = top
            list.Sort((a, b) => a.indexInStack.CompareTo(b.indexInStack));

            SavedCardState first = list[0];
            Transform parent = GetParentForContainer(first.container, first.planterID);
            if (parent == null) continue;

            CardStack stack = CreateStackParent(parent, first);

            foreach (var state in list)
            {
                //Debug.Log($"xxxLoading card → {state.cardName} in {state.container} (stack {stackKey}, idx {state.indexInStack})");
                SpawnCardFromState(state, stack.transform, stack);
            }

            stack.RefreshStack();
        }

        // 3) Restore workbench cards
        Debug.Log($"[LoadAllCards] Checking for workbench cards to restore...");

        foreach (var state in savedCards)
        {
            if (state.container != CardContainer.Workbench)
                continue;

            Debug.Log($"[LoadAllCards] Found workbench card to restore: {state.cardName}, tool={state.workbenchTool}, runtimeID={state.runtimeID}");

            var station = FindWorkbenchByTool(state.workbenchTool);
            if (station == null)
            {
                Debug.LogWarning($"[LoadAllCards] No WorkbenchStation found for tool '{state.workbenchTool}'");
                continue;
            }

            Debug.Log($"[LoadAllCards] Spawning card on {state.workbenchTool}...");
            SpawnCardFromState(state, station.transform);
            Debug.Log($"[LoadAllCards] ✅ Card spawned successfully");
        }

        Debug.Log($"[LoadAllCards] Finished restoring workbench cards");

        InventoryDebug.Dump(
            "AFTER LoadAllCards",
            playerInventoryParent,
            ingredientsInventoryParent,
            recipeHoldingParent,
            cauldronOutputParent
        );

        Debug.Log("🔥 Starting cauldron restore coroutine");
        StartCoroutine(RestoreCauldronWhenReady());

        IsLoaded = true;
        RestorePlanters();

        CardsRestored = true;
        Debug.Log("✅ Cards fully restored");
    }

    private IEnumerator RestoreCauldronWhenReady()
    {
        yield return null; // let scene + UI bind

        var saved = GameData.Instance.savedCauldron;
        if (saved == null || !saved.isBrewing)
        {
            yield break;
        }

        var cauldron = FindObjectOfType<CauldronWorkbench>();
        if (cauldron == null)
        {
            yield break;
        }

        // compute remaining time from absolute finish
        double nowOa = System.DateTime.UtcNow.ToOADate();
        float remaining = (float)((saved.finishTimeUtcOa - nowOa) * 86400.0); // days → seconds

        if (remaining <= 0f)
        {
            // brew finished while away
            cauldron.RestoreFromSave(
                saved.spellName,
                saved.finishTimeUtcOa,
                saved.fireWasOn,
                saved.totalBrewTime
                );
        }
        else
        {
            // resume brewing
            cauldron.RestoreFromSave(
                saved.spellName,
                saved.finishTimeUtcOa,
                saved.fireWasOn,
                saved.totalBrewTime
                        );
        }

        Debug.Log($"🔥 Cauldron restored — remaining={remaining:0.00}s");
    }

    void RestorePlanters()
    {
        Debug.Log($"📦 Restoring {GameData.Instance.savedPlanters.Count} planters");

        foreach (var planter in FindObjectsOfType<PlanterSlot>())
        {
            var state = GameData.Instance.savedPlanters
                .Find(p => p.planterID == planter.planterID);

            if (state == null || !state.isActive)
            {
                Debug.Log($"⏭ Planter '{planter.planterID}' has no save state");
                continue;
            }

            // Calculate game time elapsed since save
            double elapsedGameMinutes = TimeManager.TotalGameMinutes - state.savedAtGameMinutes;
            float newRemainingTime = state.remainingGrowTime - (float)elapsedGameMinutes;
            newRemainingTime = Mathf.Max(0f, newRemainingTime); // Clamp to 0

            Debug.Log($"🔄 Restoring planter '{planter.planterID}': seed={state.plantedCardName}, " +
                      $"was={state.remainingGrowTime:F2}min, elapsed={elapsedGameMinutes:F2}min, " +
                      $"now={newRemainingTime:F2}min");

            planter.RestoreFromSave(
                state.plantedCardName,
                newRemainingTime
            );
        }
    }

    void ClearParent(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            // ✅ Only destroy things that are actual card visuals / stacks
            if (child.GetComponent<CardComponent>() != null ||
                child.GetComponent<CardStack>() != null)
            {
                Destroy(child.gameObject);
            }
            // Anything else (timer UI, contents image, bubble FX etc.)
            // is left alone.
        }
    }

    Transform GetParentForContainer(CardContainer container, string planterID = null)
    {
        switch (container)
        {
            case CardContainer.PlayerInventory: return playerInventoryParent;
            case CardContainer.IngredientsInventory: return ingredientsInventoryParent;
            case CardContainer.RecipeHolding: return recipeHoldingParent;
            case CardContainer.CauldronOutput: return cauldronOutputParent;
            case CardContainer.PlanterOutput:
                // Find the planter with matching ID
                if (!string.IsNullOrEmpty(planterID))
                {
                    foreach (var planter in FindObjectsOfType<PlanterSlot>())
                    {
                        if (planter.planterID == planterID)
                            return planter.outputAnchor;
                    }
                }
                Debug.LogWarning($"Could not find planter with ID: {planterID}");
                return null;
            case CardContainer.SaleInventory:
                return saleInventoryParent;
            case CardContainer.Workbench:
                return null;
        }
        return null;
    }

    void SpawnCardFromState(SavedCardState state, Transform parent, CardStack targetStack = null)
    {
        // Find template by baseName first, then cardName
        CardData template = null;

        if (!string.IsNullOrEmpty(state.baseName))
            template = cardManager.GetCardByName(state.baseName);

        if (template == null)
            template = cardManager.GetCardByName(state.cardName);

        if (template == null)
        {
            Debug.LogWarning($"No CardData template found for '{state.cardName}'/'{state.baseName}'");
            return;
        }

        if (template.cardPrefab == null)
        {
            Debug.LogError($"Card prefab missing on template '{template.cardName}'");
            return;
        }

        // Create runtime CardData clone
        CardData runtime = ScriptableObject.CreateInstance<CardData>();
        runtime.CopyFrom(template);
        runtime.cardName = state.cardName;
        runtime.baseName = state.baseName;

        runtime.itemType = state.itemType;
        runtime.processedType = state.processedType;
        runtime.partType = state.partType;
        runtime.quantityType = state.quantityType;

        runtime.Toxin = state.Toxin;
        runtime.Neutral = state.Neutral;
        runtime.Antidote = state.Antidote;
        runtime.Sweet = state.Sweet;
        runtime.Bitter = state.Bitter;
        runtime.Salty = state.Salty;
        runtime.Spicy = state.Spicy;
        runtime.Flowery = state.Flowery;
        runtime.Umami = state.Umami;
        runtime.Edible = state.Edible;
        runtime.Incinerates = state.Incinerates;
        runtime.Smoulders = state.Smoulders;

        // Icons
        runtime.ApplyDefaultColor();

        // 🔥 Clear any icons copied from the template
        runtime.processedIcon = null;
        runtime.partIcon = null;
        runtime.quantityIcon = null;

        if (runtime.processedType != ProcessedType.None)
            runtime.ApplyProcessedIcon();

        if (runtime.partType != PartType.None)
            runtime.ApplyPartIcon();

        if (runtime.quantityType != QuantityType.None)
            runtime.ApplyQuantityIcon();

        // Instantiate visual card
        GameObject cardGO = Instantiate(template.cardPrefab, parent);
        CardComponent comp = cardGO.GetComponent<CardComponent>();
        if (comp == null)
        {
            Debug.LogError("Prefab missing CardComponent!");
            Destroy(cardGO);
            return;
        }

        comp.runtimeID = state.runtimeID;
        comp.SetCardData(runtime, true);

        // 🔽 NEW: if we're spawning into a stack, register it
        if (targetStack != null)
        {
            // This will reparent (to same parent) and update visuals
            targetStack.AddCard(comp);
        }
    }

    private CardStack CreateStackParent(Transform parent, SavedCardState firstState)
    {
        GameObject stackGO = new GameObject("CardStack_" + firstState.cardName);
        RectTransform rt = stackGO.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one;

        CardStack stack = stackGO.AddComponent<CardStack>();
        stack.stackName = firstState.cardName;

        return stack;
    }
}