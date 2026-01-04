using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardData;

public enum ProcessingTool
{
    MortarAndPestle,
    Cauldron,
    ChoppingBoard,
    DryingRack,
    BruteForce      // for mallet/axe to break up wood and minerals
}

public class WorkbenchStation : MonoBehaviour
{
    [Header("Testing - BasicWorkstationTimer")]
    [Tooltip("Multiply timer speed for testing. 2 = twice as fast, 0.5 = half speed.")]
    public float timerSpeedMultiplier = 1f; // default 1 = normal speed

    public ProcessingTool tool;

    [Header("UI")]
    public Slider toolTimerSlider;
    public Transform dryingRackUIParent;
    public GameObject dryingRackTimerPrefab;
    public GameObject toolTimerRoot;
    public DryingRackTimer dryingRackTimer;

    private List<ActiveProcess> activeProcesses = new List<ActiveProcess>();

    [System.Serializable]
    public class ActiveProcess
    {
        public CardComponent card;
        public ProcessingRecipe recipe;
        public Coroutine timerCoroutine;
        public Slider timerSlider;
    }

    [Header("Inventory Target")]
    public Transform playerInventoryParent;
    public Transform ingredientsInventoryParent;

    public CardManager CardManagerInstance;
    public RecipeDatabase recipeDatabase;
    public TimeManager timeManager;

    [Header("Feedback")]
    [SerializeField] private Outline invalidOutline;
    public Color invalidFlashColor = Color.red;
    public float invalidFlashDuration = 0.25f;

    float currentElapsed;
    ProcessingRecipe currentRecipe;
    CardComponent currentCard;
    Coroutine activeCoroutine;

    [Header("Pause")]
    public Button pauseButton;
    public TMPro.TMP_Text pauseLabel;

    private bool isBusy = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (tool == ProcessingTool.DryingRack && dryingRackTimer == null)
        {
            // Look in the prefab instance under the parent
            if (dryingRackUIParent != null)
            {
                dryingRackTimer = dryingRackUIParent.GetComponentInChildren<DryingRackTimer>();
            }

            if (dryingRackTimer == null)
            {
                Debug.LogError("DryingRackTimer not found under dryingRackUIParent!");
            }
        }
    }

    void Start()
    {
        StartCoroutine(TryRestoreAfterLoad());
    }

    void OnDisable()
    {
        PersistIfPaused();
    }

    IEnumerator TryRestoreAfterLoad()
    {
        //Debug.Log($"[{tool}] TryRestoreAfterLoad starting...");

        yield return null;
        yield return null;

        if (!ManualProcessPersistence.Instance.HasSavedProcess(tool)) // 👈 Pass tool
        {
            //Debug.Log($"[{tool}] No saved process found");
            yield break;
        }

        var state = ManualProcessPersistence.Instance.Peek(tool); // 👈 Pass tool

        //Debug.Log($"[{tool}] Peeked at saved state: tool={state.tool}, cardID={state.cardRuntimeID}, remaining={state.remainingTime}");

        // No need to check if tool matches - we already filtered by tool

        //Debug.Log($"[{tool}] Looking for card with runtimeID: {state.cardRuntimeID}");

        CardComponent card = FindCardInSceneByRuntimeID(state.cardRuntimeID);

        if (card == null)
        {
            Debug.LogWarning($"[{tool}] Card {state.cardRuntimeID} not found after load.");
            yield break;
        }

        //Debug.Log($"✅ Found card '{card.CardData.cardName}'");

        ManualProcessPersistence.Instance.Consume(tool); // 👈 Pass tool

        //Debug.Log($"✅ Restoring paused {tool} with {state.remainingTime:F2}s");

        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;

        StartCoroutine(ResumeProcess(card, state));
    }

    IEnumerator ResumeProcess(CardComponent card, ManualProcessState state)
    {
        toolTimerRoot.SetActive(true);
        //toolTimerSlider.maxValue = state.recipe.processingTime;
        //toolTimerSlider.value = state.remainingTime;

        currentCard = card;
        currentRecipe = state.recipe;
        currentElapsed = state.recipe.processingTime - state.remainingTime;

        isBusy = true;
        isPaused = true;

        if (toolTimerSlider != null)
        {
            toolTimerSlider.maxValue = state.recipe.processingTime;
            toolTimerSlider.value = state.remainingTime; // 👈 Should be remainingTime, not elapsed
            toolTimerSlider.gameObject.SetActive(true);
        }

        ShowPauseButton();
        pauseLabel.text = "Resume";

        activeCoroutine = StartCoroutine(
            ProcessTimer(card, state.recipe, toolTimerSlider)
        );

        yield break;
    }

    public void StartProcessing(CardComponent card)
    {
        if (card == null || card.CardData == null)
            return;

        // Debug: see exactly what this card thinks it is
        //Debug.Log($"[StartProcessing] Card = {card.CardData.cardName}, " +
        //          $"recipes = {card.CardData.processingRecipes?.Count ?? 0}, tool = {tool}");

        CardData runtimeData = card.CardData;   // ← use the instance attached to this card

        // Find the recipe ON THIS CARD
        ProcessingRecipe recipe = runtimeData.processingRecipes
            .Find(r => r.tool == tool);

        // ------------------------------------------------------------
        // PREVENT REPROCESSING of cards already processed
        // (unless the recipe itself explicitly produces visualOutputs)
        // ------------------------------------------------------------
        if (runtimeData.processedType != ProcessedType.None &&
            (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
        {
            Debug.Log($"[StartProcessing] Cannot reprocess '{runtimeData.cardName}' " +
                      $"because it already has processedType={runtimeData.processedType}");

            ReturnCardToIngredients(card);
            return;
        }

        if (recipe == null)
        {
            Debug.Log($"[StartProcessing] No recipe for {runtimeData.cardName} on tool {tool}, returning card.");
            ReturnCardToIngredients(card);
            return;
        }

        isBusy = true;

        //Debug.Log($"[StartProcessing] Using recipe on {card.CardData.cardName}: " +
        //          $"tool={recipe.tool}, processedType={recipe.processedResultType}, " +
        //          $"outputs={recipe.visualOutputs?.Count ?? 0}");

        // Snap onto tool visually
        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;

        // Refresh visuals based on this card's current data
        card.SetCardData(runtimeData);

        // Drying rack: handled by its own timer
        if (tool == ProcessingTool.DryingRack)
        {
            if (dryingRackTimer == null && dryingRackUIParent != null)
                dryingRackTimer = dryingRackUIParent.GetComponentInChildren<DryingRackTimer>();

            if (dryingRackTimer == null)
            {
                Debug.LogError("DryingRackTimer not found!");
                return;
            }

            dryingRackTimer.playerInventoryParent = playerInventoryParent;
            dryingRackTimer.cardManager = CardManagerInstance;
            bool added = dryingRackTimer.AddCard(card, recipe.processingTime, recipe);
            if (!added)
                ReturnCardToIngredients(card);

            return;
        }

        // Single-slider for other tools
        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(true);

        ShowPauseButton();

        toolTimerSlider.maxValue = recipe.processingTime;
        toolTimerSlider.value = recipe.processingTime;
        toolTimerSlider.gameObject.SetActive(true);
        toolTimerSlider.transform.SetAsLastSibling();

        currentRecipe = recipe;
        currentCard = card;
        currentElapsed = 0f;
        activeCoroutine = StartCoroutine(ProcessTimer(card, recipe, toolTimerSlider));
    }

    private void ReturnCardToIngredients(CardComponent card)
    {
        if (ingredientsInventoryParent == null)
        {
            Debug.LogWarning("Ingredients inventory parent not assigned!");
            return;
        }

        card.transform.SetParent(ingredientsInventoryParent, false);
        card.transform.localPosition = Vector3.zero;

        // Optional: reset any visual states
        card.SetCardData(card.CardData);
    }

    private IEnumerator ProcessTimer(CardComponent card, ProcessingRecipe recipe, Slider slider)
    {
        float totalTime = recipe.processingTime; // real-time seconds


        if (slider != null)
        {
            slider.maxValue = totalTime;
            slider.value = totalTime - currentElapsed;
        }

        while (currentElapsed < totalTime)
        {
            if (!isPaused)
            {
                currentElapsed += timerSpeedMultiplier * Time.deltaTime;

                if (currentElapsed > totalTime)
                    currentElapsed = totalTime;

                if (slider != null)
                    slider.value = totalTime - currentElapsed;
            }

            yield return null;
        }

        if (slider != null)
            slider.gameObject.SetActive(false);
        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(false);

        // Processing complete
        card.CardData.processedType = recipe.processedResultType;
        card.MarkAsProcessed();

        // ------------------------------------------------------
        // PROCESSED NAME UPDATE (no visual outputs case)
        // e.g. "Corn Stem" → "Corn Stem Chopped"
        // ------------------------------------------------------
        if (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0)
        {
            string processedSuffix = recipe.processedResultType.ToString(); // e.g. Chopped, Powder, Dried
            string currentName = card.CardData.cardName;

            // Avoid doubling the suffix if already present
            if (!currentName.EndsWith(" " + processedSuffix))
            {
                card.CardData.cardName = currentName + " " + processedSuffix;
            }

            //Debug.Log($"[Processed Rename] '{currentName}' → '{card.CardData.cardName}'");

            // Apply immediately to the visuals
            card.SetCardData(card.CardData, true);
        }

        isBusy = false;
        HidePauseButton();

        RemoveSavedCard();

        // Spawn outputs
        if (recipe.visualOutputs != null && recipe.visualOutputs.Count > 0)
        {
            SpawnOutputCards(recipe.visualOutputs, card.CardData);
            Destroy(card.gameObject);
        }
        else
        {
            if (playerInventoryParent != null)
            {
                card.transform.SetParent(playerInventoryParent, false);
                card.transform.localPosition = Vector3.zero;
            }
        }
    }

    private void RemoveSavedCard()
    {
        if (currentCard == null)
            return;

        // Remove from CardPersistenceManager's saved cards list
        if (GameData.Instance != null && GameData.Instance.savedCards != null)
        {
            var toRemove = GameData.Instance.savedCards
                .FindAll(s => s.container == CardContainer.Workbench &&
                              s.workbenchTool == tool.ToString());

            foreach (var card in toRemove)
            {
                GameData.Instance.savedCards.Remove(card);
                //Debug.Log($"🗑️ Removed completed card from save: {card.cardName}");
            }
        }

        // Remove from ManualProcessPersistence
        if (ManualProcessPersistence.Instance.HasSavedProcess(tool))
        {
            ManualProcessPersistence.Instance.Consume(tool);
            //Debug.Log($"🗑️ Removed completed process from persistence: {tool}");
        }
    }


    private void SpawnOutputCards(List<ProcessingVisualOutput> outputs, CardData sourceCardData)
    { 
        if (outputs == null || outputs.Count == 0)
            return;

        foreach (var output in outputs)
        {
            for (int i = 0; i < output.quantity; i++)
            {
                SpawnSingleOutput(output, sourceCardData);
            }
        }
    }
    private void SpawnSingleOutput(ProcessingVisualOutput output, CardData sourceCardData)
    {
        if (playerInventoryParent == null || sourceCardData == null)
        {
            Debug.LogError("PlayerInventoryParent or sourceCardData not assigned!");
            return;
        }

        //-----------------------------------------------------
        // 1. Compute the correct final name using baseName
        //-----------------------------------------------------

        // --- Decide the base name (e.g. Corn, Death Cap, Shaggy Inkcap) ---
        string baseName = !string.IsNullOrEmpty(sourceCardData.baseName)
            ? sourceCardData.baseName
            : sourceCardData.cardName;

        // This is what comes from the recipe (CSV → ProcessingVisualOutput.name)
        string outputName = output.name;

        // CASE A: second-step processing, e.g. "Corn Head" → "Corn Kernels"
        // sourceCardData.cardName = "Corn Head"
        // baseName                 = "Corn"
        // output.name              = "Corn Head Kernels"
        if (!string.IsNullOrEmpty(sourceCardData.baseName) &&
            sourceCardData.baseName != sourceCardData.cardName &&
            outputName.StartsWith(sourceCardData.cardName))
        {
            // Remove the "Corn Head" part and keep whatever comes after
            string suffix = outputName.Substring(sourceCardData.cardName.Length);  // " Kernels"
            outputName = baseName + suffix;                                       // "Corn Kernels"
        }
        else
        {
            // CASE B: normal case – make sure it at least starts with the base name.
            // e.g. baseName = "Corn", output.name = "Stem" → "Corn Stem"
            if (!outputName.StartsWith(baseName))
                outputName = baseName + " " + outputName;
        }

        //Debug.Log($"[Output Naming] {sourceCardData.cardName} + '{output.name}' => '{outputName}'");


        //-----------------------------------------------------
        // 2. Look for an existing stack with that final name
        //-----------------------------------------------------

        CardStack existingStack = null;
        foreach (Transform child in playerInventoryParent)
        {
            CardStack stack = child.GetComponent<CardStack>();
            if (stack != null && stack.stackName == outputName)
            {
                existingStack = stack;
                //Debug.Log($"Found existing stack: {stack.stackName}");
                break;
            }
        }


        //-----------------------------------------------------
        // 3. Look up the correct CardData asset
        //-----------------------------------------------------

        CardData template = null;

        if (CardManagerInstance != null)
            template = CardManagerInstance.GetCardByName(outputName);

        if (template == null)
        {
            //Debug.LogWarning($"No CardData asset found for '{outputName}', cloning from {sourceCardData.cardName}");
            template = sourceCardData;
        }


        //-----------------------------------------------------
        // 4. Create a SAFE runtime copy
        //-----------------------------------------------------

        CardData runtimeData = ScriptableObject.CreateInstance<CardData>();
        runtimeData.CopyFrom(template);
        runtimeData.cardName = outputName;     // ensure correct name remains
        runtimeData.baseName = baseName;       // preserve base chain


        //-----------------------------------------------------
        // 5. Make sure we have a prefab to spawn visually
        //-----------------------------------------------------

        GameObject prefab = template.cardPrefab != null
                            ? template.cardPrefab
                            : sourceCardData.cardPrefab;

        if (prefab == null)
        {
            Debug.LogError($"Card prefab missing for '{template.cardName}'");
            return;
        }


        //-----------------------------------------------------
        // 6. Add to existing stack
        //-----------------------------------------------------

        if (existingStack != null)
        {
            GameObject cardGO = Instantiate(prefab, existingStack.transform);
            CardComponent comp = cardGO.GetComponent<CardComponent>();

            if (comp == null)
            {
                Debug.LogError("Prefab missing CardComponent!");
                Destroy(cardGO);
                return;
            }

            comp.SetCardData(runtimeData);
            comp.SetVisualOutputCard(output);
            existingStack.AddCard(comp);

            return;
        }


        //-----------------------------------------------------
        // 7. Create a NEW stack
        //-----------------------------------------------------

        GameObject stackObj = new GameObject("CardStack_" + outputName,
            typeof(RectTransform), typeof(CardStack));

        stackObj.transform.SetParent(playerInventoryParent, false);

        CardStack newStack = stackObj.GetComponent<CardStack>();
        newStack.stackName = outputName;

        GameObject newCardGO = Instantiate(prefab, stackObj.transform);
        CardComponent newCard = newCardGO.GetComponent<CardComponent>();

        if (newCard == null)
        {
            Debug.LogError("Prefab missing CardComponent!");
            Destroy(stackObj);
            Destroy(newCardGO);
            return;
        }

        newCard.SetCardData(runtimeData);
        newCard.SetVisualOutputCard(output);

        newStack.Initialize(newCard);

        //Debug.Log($"Created new stack for '{outputName}'");
    }

    public bool TryStartProcessing(CardComponent card)
    {
        CardData runtimeData = card.CardData;

        // Find appropriate recipe
        ProcessingRecipe recipe = runtimeData.processingRecipes
            .Find(r => r.tool == tool);

        // ❌ No recipe → invalid
        if (recipe == null)
            return false;

        // ❌ Already processed → invalid
        if (runtimeData.processedType != ProcessedType.None &&
            (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
            return false;

        // Otherwise use normal flow
        StartProcessing(card);
        return true;
    }

    public void FlashInvalidDrop()
    {
        if (invalidOutline == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashOutlineRoutine());
    }

    private IEnumerator FlashOutlineRoutine()
    {
        Color start = invalidOutline.effectColor;
        Color flash = invalidFlashColor;
        flash.a = 1f;

        invalidOutline.effectColor = flash;

        yield return new WaitForSeconds(invalidFlashDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            invalidOutline.effectColor = Color.Lerp(flash, start, t);
            yield return null;
        }
    }

    public void TogglePause()
    {
        if (!isBusy)
            return;

        isPaused = !isPaused;

        if (pauseLabel != null)
            pauseLabel.text = isPaused ? "Resume" : "Pause";

        // Check if ANY workbench is busy
        bool anyWorkbenchBusy = false;
        foreach (var station in FindObjectsOfType<WorkbenchStation>())
        {
            if (station.isBusy && !station.isPaused)
            {
                anyWorkbenchBusy = true;
                break;
            }
        }

        ExitButtonsController.SetEnabled(!anyWorkbenchBusy);

        Debug.Log(isPaused
            ? $"[{tool}] Process paused"
            : $"[{tool}] Process resumed");
    }

    void ShowPauseButton()
    {
        //Debug.Log($"PauseButton activeSelf={pauseButton.gameObject.activeSelf}, activeInHierarchy={pauseButton.gameObject.activeInHierarchy}");
        pauseButton.gameObject.SetActive(true);

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        if (pauseLabel != null)
            pauseLabel.text = "Pause";
    }

    void HidePauseButton()
    {
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
    }

    public void PersistIfPaused()
    {
        if (!isBusy)
            return;

        if (!isPaused)
            return;

        if (currentCard == null || currentRecipe == null)
            return;

        float remaining = currentRecipe.processingTime - currentElapsed;
        if (remaining <= 0f)
            return;

        // 👇 SAVE THE CARD to CardPersistenceManager FIRST
        if (CardPersistenceManager.Instance != null && currentCard != null)
        {
            // Remove any existing saved card for this workbench first
            var existing = GameData.Instance.savedCards
                .FindAll(s => s.container == CardContainer.Workbench &&
                             s.workbenchTool == tool.ToString());

            foreach (var old in existing)
            {
                GameData.Instance.savedCards.Remove(old);
                //Debug.Log($"🗑️ Removed old workbench card before re-saving: {old.cardName}");
            }

            CardData data = currentCard.CardData;

            var savedCard = new SavedCardState
            {
                runtimeID = currentCard.runtimeID,
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

                container = CardContainer.Workbench,
                workbenchTool = tool.ToString(),
                orderInParent = 0,

                isStacked = false,
                stackKey = null,
                indexInStack = 0
            };

            // Add directly to the saved cards list
            GameData.Instance.savedCards.Add(savedCard);
            //Debug.Log($"💾 Saved workbench card: {data.cardName}, runtimeID={currentCard.runtimeID}");
        }

        ManualProcessPersistence.Instance.Save(
            new ManualProcessState
            {
                tool = tool,
                recipe = currentRecipe,
                remainingTime = remaining,
                cardRuntimeID = currentCard.RuntimeID
            }
        );

        //Debug.Log($"💾 Persisted paused {tool} with {remaining:F2}s remaining");
    }

    CardComponent FindCardInSceneByRuntimeID(string runtimeID)
    {
        //Debug.Log($"[{tool}] Searching for card with runtimeID: {runtimeID}");

        var allCards = FindObjectsOfType<CardComponent>();
        //Debug.Log($"[{tool}] Found {allCards.Length} total cards in scene");

        foreach (var card in allCards)
        {
            //Debug.Log($"[{tool}]   Checking card: name={card.CardData?.cardName}, runtimeID={card.RuntimeID}, parent={card.transform.parent?.name}");

            if (card.RuntimeID == runtimeID)
            {
                //Debug.Log($"[{tool}] ✅ MATCH FOUND!");
                return card;
            }
        }

        Debug.LogWarning($"[{tool}] ❌ No card found with runtimeID={runtimeID}");
        return null;
    }
}