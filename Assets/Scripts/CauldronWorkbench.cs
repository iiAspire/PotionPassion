using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CauldronWorkbench : WorkbenchStation
{
    ComboGenerator Combos => GameInitialization.Combos;
    RecipeDatabase Recipes => GameInitialization.Recipes;

    [Header("Cauldron Specific")]
    public Transform outputParent;
    public GameObject cardPrefab;
    public float defaultBrewTime = 5f;

    [Header("Recipe Holding")]
    public Transform recipeHoldingParent;

    [Header("Feedback")]
    public Image cauldronContents;
    public GameObject recipeStatus;
    public TMP_Text failedBrewLogTMP;
    public CauldronBubbleUI bubbleUI;
    public CauldronSmokeUI smokeUI;
    public CauldronFireController fireController;

    private SpellCombo activeCombo;
    private CardComponent processingSingleCard;
    private bool isBrewing = false;
    private double finishAtGameMinutes;
    private float brewTimeRemaining;
    private float totalBrewTime;

    public float TotalBrewTime => totalBrewTime;
    public double FinishAtGameMinutes => finishAtGameMinutes;
    public bool IsBrewing => isBrewing;
    public string ActiveSpellName => activeCombo != null ? activeCombo.SpellName : null;
    public bool FireWasOn => fireController != null && fireController.IsFireOn;

    private Coroutine brewCoroutine;

    private ProcessingRecipe singleCardRecipe; // Store the recipe selected by Cauldron

    void OnDisable()
    {
        if (isBrewing)
        {
            StopAllCoroutines();
        }
    }

    public void ToggleFireUI()
    {
        if (fireController != null)
            fireController.ToggleFire();
    }

    public void StartBrewingWithRecipe(ProcessingRecipe recipe, List<string> ingredients)
    {
        Debug.Log($"🟢 [CauldronWorkbench] StartBrewingWithRecipe called with recipe: {recipe.processedResultType}");

        if (ingredients != null && ingredients.Count == 1 && recipe != null)
        {
            singleCardRecipe = recipe;
            StartSingleIngredientProcess(ingredients[0]);
        }
        else
        {
            Debug.LogWarning("StartBrewingWithRecipe called with invalid parameters");
        }
    }

    private void StartSingleIngredientProcess(string ingredientName)
    {
        Debug.Log("🟢 [CauldronWorkbench] StartSingleIngredientProcess called");
        Debug.Log($"🟢 [CauldronWorkbench] recipeHoldingParent null? {recipeHoldingParent == null}");

        // Find the single card in recipeHoldingParent
        CardComponent card = null;

        if (recipeHoldingParent != null && recipeHoldingParent.childCount > 0)
        {
            Debug.Log($"🟢 [CauldronWorkbench] recipeHoldingParent has {recipeHoldingParent.childCount} children");
            card = recipeHoldingParent.GetChild(0).GetComponent<CardComponent>();
        }

        if (card == null)
        {
            Debug.LogWarning("⚠️ [CauldronWorkbench] No card found in recipeHoldingParent for single ingredient processing");
            return;
        }

        Debug.Log($"🟢 [CauldronWorkbench] Found card: {card.CardData.cardName}");

        // Use the recipe passed from Cauldron (which already selected based on fire state)
        ProcessingRecipe recipe = singleCardRecipe;

        if (recipe == null)
        {
            Debug.LogWarning("⚠️ [CauldronWorkbench] No recipe was passed from Cauldron!");
            return;
        }

        Debug.Log($"🟢 [CauldronWorkbench] Using passed recipe: processedType={recipe.processedResultType}, time={recipe.processingTime}, needsFire={recipe.needsFire}");

        // Check if already processed (prevent reprocessing)
        if (card.CardData.processedType != ProcessedType.None &&
            (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
        {
            Debug.Log($"⚠️ [CauldronWorkbench] Cannot reprocess '{card.CardData.cardName}' - already processed");
            ReturnCardToRecipeHolding(card);
            return;
        }

        if (recipe.needsFire && (fireController == null || !fireController.IsFireOn))
        {
            Debug.LogWarning("🔥 [CauldronWorkbench] Fire required for this process");
            return;
        }

        Debug.Log("✅ [CauldronWorkbench] Starting single ingredient process");

        activeCombo = null;   // 🔴 IMPORTANT: marks this as NOT a spell
        isBrewing = true;

        double now = TimeManager.TotalGameMinutes;
        double minutes = recipe.processingTime * TimeManager.MinutesPerRealSecond;

        finishAtGameMinutes = now + minutes;
        totalBrewTime = (float)minutes;
        brewTimeRemaining = (float)minutes;

        processingSingleCard = card;

        Debug.Log($"🟢 [CauldronWorkbench] Process time: {recipe.processingTime}s real-time = {minutes} game minutes");
        Debug.Log($"🟢 [CauldronWorkbench] Stored card reference: {processingSingleCard != null}");
        Debug.Log($"🟢 [CauldronWorkbench] Card name before hiding: {processingSingleCard.CardData.cardName}");
        Debug.Log($"🟢 [CauldronWorkbench] Hiding card and starting brew routine");

        // Hide card during processing
        card.gameObject.SetActive(false);

        Debug.Log($"🟢 [CauldronWorkbench] After hiding, processingSingleCard null? {processingSingleCard == null}");

        // Show timer UI
        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(true);

        if (toolTimerSlider != null)
        {
            toolTimerSlider.maxValue = totalBrewTime;
            toolTimerSlider.value = totalBrewTime;
            toolTimerSlider.gameObject.SetActive(true);
        }

        // Show processing visuals
        if (bubbleUI != null)
        {
            bubbleUI.enabled = true;
            bubbleUI.gameObject.SetActive(true);
        }

        if (cauldronContents != null)
        {
            cauldronContents.gameObject.SetActive(true);
        }

        brewCoroutine = StartCoroutine(BrewRoutine(null, card, singleCardRecipe));

    }

    private void ReturnCardToRecipeHolding(CardComponent card)
    {
        if (recipeHoldingParent == null)
        {
            Debug.LogWarning("Recipe holding parent not assigned!");
            return;
        }

        card.transform.SetParent(recipeHoldingParent, false);
        card.transform.localPosition = Vector3.zero;
        card.gameObject.SetActive(true);
    }

    public void StartBrewing(SpellCombo combo, List<string> ingredients = null)
    {
        Debug.Log($"🟢 [CauldronWorkbench] StartBrewing called: combo={(combo != null ? combo.SpellName : "NULL")}, ingredients={ingredients?.Count ?? 0}");

        // 🔹 SINGLE INGREDIENT MODE - handled by StartBrewingWithRecipe instead
        if (ingredients != null && ingredients.Count == 1)
        {
            Debug.LogWarning("⚠️ [CauldronWorkbench] StartBrewing called for single ingredient - should use StartBrewingWithRecipe");
            return;
        }

        if (isBrewing)
        {
            Debug.Log("⚠️ [CauldronWorkbench] Cauldron is already brewing!");
            return;
        }

        // If an old output card is still on the cauldron, move it to player inventory first
        if (outputParent != null && playerInventoryParent != null)
        {
            for (int i = outputParent.childCount - 1; i >= 0; i--)
            {
                Transform child = outputParent.GetChild(i);
                CardComponent cardComp = child.GetComponent<CardComponent>();
                if (cardComp == null) continue;

                child.SetParent(playerInventoryParent, false);
                child.localPosition = Vector3.zero;
            }
        }

        activeCombo = combo;
        isBrewing = true;

        bool failed = (activeCombo == null);
        Debug.Log($"🟢 [CauldronWorkbench] Multi-ingredient brew: failed={failed}");

        // 1️⃣ Base time from RecipeDatabase (tier-based)
        float brewTime = Recipes != null
            ? Recipes.GetBrewTimeSeconds(activeCombo, failed)
            : defaultBrewTime;

        // 2️⃣ Optional override from ProcessingRecipe
        if (activeCombo != null && activeCombo.ResultCard != null)
        {
            var recipe = activeCombo.ResultCard.processingRecipes
                .Find(r => r.tool == ProcessingTool.Cauldron);

            if (recipe != null)
            {
                if (recipe.needsFire)
                {
                    if (fireController == null || !fireController.IsFireOn)
                    {
                        Debug.LogWarning("🔥 Recipe requires fire, but fire is OFF.");
                        activeCombo = null;
                    }
                    else if (recipe.processingTimeWithFire > 0f)
                    {
                        brewTime = recipe.processingTimeWithFire;
                    }
                }
                else if (recipe.processingTime > 0f)
                {
                    brewTime = recipe.processingTime;
                }
            }
        }

        double now = TimeManager.TotalGameMinutes;
        double brewMinutes = brewTime * TimeManager.MinutesPerRealSecond;

        finishAtGameMinutes = now + brewMinutes;
        brewTimeRemaining = (float)brewMinutes;
        totalBrewTime = (float)brewMinutes;

        if (cauldronContents != null)
        {
            cauldronContents.gameObject.SetActive(true);

            Sprite liquidSprite = (combo != null && combo.ResultCard != null)
                ? combo.ResultCard.cauldronContentsSprite
                : null;

            if (liquidSprite != null)
                cauldronContents.sprite = liquidSprite;
        }

        if (bubbleUI != null)
            bubbleUI.enabled = true;

        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(true);

        if (toolTimerSlider != null)
        {
            toolTimerSlider.maxValue = totalBrewTime;
            toolTimerSlider.value = totalBrewTime;
            toolTimerSlider.gameObject.SetActive(true);
        }

        if (!enabled)
            enabled = true;

        if (bubbleUI != null)
            bubbleUI.gameObject.SetActive(true);

        if (fireController != null)
            fireController.gameObject.SetActive(true);

        brewCoroutine = StartCoroutine(BrewRoutine(ingredients, null, null));
    }

    private IEnumerator BrewRoutine(List<string> ingredients, CardComponent singleCard = null, ProcessingRecipe singleRecipe = null)
    {
        Debug.Log("🟢 [CauldronWorkbench] BrewRoutine started");
        Debug.Log($"🟢 [CauldronWorkbench] At start of routine, singleCard param null? {singleCard == null}");
        Debug.Log($"🟢 [CauldronWorkbench] At start of routine, processingSingleCard field null? {processingSingleCard == null}");

        int frameCount = 0;
        while (true)
        {
            double now = TimeManager.TotalGameMinutes;
            double remaining = finishAtGameMinutes - now;

            brewTimeRemaining = Mathf.Max(0f, (float)remaining);

            if (toolTimerSlider != null)
                toolTimerSlider.value = brewTimeRemaining;

            // Periodic check
            //if (frameCount % 60 == 0 && singleCard != null)
            //{
            //    bool cardStillExists = singleCard != null;
            //    bool gameObjectExists = singleCard != null && singleCard.gameObject != null;
            //    Debug.Log($"🟢 [CauldronWorkbench] Frame {frameCount}: card={cardStillExists}, gameObject={gameObjectExists}");
            //}
            //frameCount++;

            if (brewTimeRemaining <= 0f)
            {
                Debug.Log("🟢 [CauldronWorkbench] Brew time complete!");
                break;
            }

            yield return null;
        }

        CompleteBrewing(ingredients, singleCard, singleRecipe);
    }

    private void CompleteBrewing(List<string> ingredients, CardComponent singleCard = null, ProcessingRecipe singleRecipe = null)
    {
        Debug.Log("🟢 [CauldronWorkbench] CompleteBrewing called");
        Debug.Log($"🟢 [CauldronWorkbench] singleCard param null? {singleCard == null}");
        Debug.Log($"🟢 [CauldronWorkbench] processingSingleCard field null? {processingSingleCard == null}");

        if (singleCard != null)
        {
            Debug.Log($"🟢 [CauldronWorkbench] Card reference exists via parameter, checking GameObject...");
            Debug.Log($"🟢 [CauldronWorkbench] Card GameObject null? {singleCard.gameObject == null}");
            Debug.Log($"🟢 [CauldronWorkbench] Card name: {(singleCard.gameObject != null ? singleCard.name : "DESTROYED")}");
        }

        // 🔹 SINGLE INGREDIENT COMPLETE
        if (singleCard != null && singleRecipe != null)
        {
            Debug.Log($"🟢 [CauldronWorkbench] Processing single card: {singleCard.CardData.cardName}");

            Debug.Log($"🟢 [CauldronWorkbench] Recipe processedType: {singleRecipe.processedResultType}");

            // Mark as processed with the correct type
            singleCard.CardData.processedType = singleRecipe.processedResultType;
            Debug.Log($"🟢 [CauldronWorkbench] Set CardData.processedType to {singleRecipe.processedResultType}");

            singleCard.MarkAsProcessed();
            Debug.Log("🟢 [CauldronWorkbench] Called MarkAsProcessed()");

            // Update the card name to include the processed type
            string processedSuffix = singleRecipe.processedResultType.ToString();
            string currentName = singleCard.CardData.cardName;

            if (!currentName.EndsWith(" " + processedSuffix))
            {
                singleCard.CardData.cardName = currentName + " " + processedSuffix;
                Debug.Log($"🟢 [CauldronWorkbench] Renamed card: '{currentName}' → '{singleCard.CardData.cardName}'");
            }

            // Refresh the card visuals
            singleCard.SetCardData(singleCard.CardData, true);
            Debug.Log("🟢 [CauldronWorkbench] Called SetCardData with forceRefresh=true");

            // Move to output parent
            Debug.Log($"🟢 [CauldronWorkbench] Moving to outputParent (null? {outputParent == null})");
            singleCard.transform.SetParent(outputParent, false);
            singleCard.transform.localPosition = Vector3.zero;
            singleCard.gameObject.SetActive(true);

            Debug.Log("✅ [CauldronWorkbench] Single card processing COMPLETE");

            // Clean up
            processingSingleCard = null;
            singleCardRecipe = null;
            isBrewing = false;

            // Hide visuals
            if (toolTimerRoot != null)
                toolTimerRoot.SetActive(false);
            if (bubbleUI != null)
                bubbleUI.enabled = false;
            if (cauldronContents != null)
                cauldronContents.gameObject.SetActive(false);

            return;
        }

        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(false);
        isBrewing = false;
        brewCoroutine = null;

        if (bubbleUI != null)
            bubbleUI.enabled = false;
        if (cauldronContents != null)
            cauldronContents.gameObject.SetActive(false);

        // INVALID RECIPE
        if (activeCombo == null)
        {
            Debug.Log("⚠️ [CauldronWorkbench] Invalid recipe - showing failed brew");

            if (recipeStatus != null)
            {
                recipeStatus.SetActive(true);
                StartCoroutine(HideAfterDelay(recipeStatus, 3f));
            }

            if (failedBrewLogTMP != null && ingredients != null)
            {
                failedBrewLogTMP.text = "Brew complete; not a valid recipe. The ingredients were lost.";
            }

            if (smokeUI != null)
                smokeUI.PlaySmokeBurst();

            if (Recipes != null && ingredients != null)
            {
                GameData.Instance.failedBrews.Add(new SpellCombo
                {
                    SpellName = "Invalid Combo",
                    Ingredients = new List<string>(ingredients)
                });
            }

            activeCombo = null;
            return;
        }

        // VALID RECIPE → spawn result card on cauldron
        Debug.Log($"✅ [CauldronWorkbench] Valid recipe complete: {activeCombo.SpellName}");

        if (cardPrefab != null && outputParent != null)
        {
            GameObject cardGO = Instantiate(cardPrefab, outputParent);
            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;
            cardGO.transform.localScale = Vector3.one;

            CardComponent cardComp = cardGO.GetComponent<CardComponent>();
            if (cardComp != null)
            {
                CardData resultData = activeCombo.ResultCard != null
                    ? ScriptableObject.Instantiate(activeCombo.ResultCard)
                    : ScriptableObject.CreateInstance<CardData>();

                if (activeCombo.ResultCard == null)
                {
                    resultData.cardName = activeCombo.SpellName;
                    resultData.processedType = ProcessedType.Potion;
                }

                if (string.IsNullOrEmpty(cardComp.runtimeID))
                    cardComp.runtimeID = System.Guid.NewGuid().ToString();

                cardComp.SetCardData(resultData, true);
                cardComp.AssignedCombo = activeCombo;
            }

            if (Recipes != null)
            {
                GameData.Instance.successfulBrews.Add(activeCombo);
            }
        }

        RecipeDiscoverySystem.Instance.OnSuccessfulBrew(activeCombo);
        activeCombo = null;
    }

    private IEnumerator HideAfterDelay(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
            obj.SetActive(false);
    }

    public void RestoreFromSave(
        string spellName,
        double savedFinishAtGameMinutes,
        bool fireWasOn,
        float totalBrewTimeFromSave
    )
    {
        if (string.IsNullOrEmpty(spellName))
            return;

        SpellCombo combo = GameInitialization.Recipes.GetComboByName(spellName);
        if (combo == null)
        {
            Debug.LogWarning($"⚠️ Could not restore brew for spell '{spellName}'");
            return;
        }

        activeCombo = combo;
        isBrewing = true;

        totalBrewTime = totalBrewTimeFromSave;
        finishAtGameMinutes = savedFinishAtGameMinutes;

        double remaining = finishAtGameMinutes - TimeManager.TotalGameMinutes;

        if (remaining <= 0)
        {
            CompleteBrewing(null);
            return;
        }

        brewTimeRemaining = (float)remaining;

        if (fireController != null)
            fireController.SetFire(fireWasOn);

        if (cauldronContents != null)
        {
            cauldronContents.gameObject.SetActive(true);
            cauldronContents.sprite = combo.ResultCard?.cauldronContentsSprite;
        }

        if (bubbleUI != null)
            bubbleUI.gameObject.SetActive(true);

        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(true);

        if (toolTimerSlider != null)
        {
            toolTimerSlider.maxValue = totalBrewTime;
            toolTimerSlider.value = brewTimeRemaining;
            toolTimerSlider.gameObject.SetActive(true);
        }

        brewCoroutine = StartCoroutine(BrewRoutine(null));
    }
}