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
    public Transform outputParent;       // where brew result card appears
    public GameObject cardPrefab;        // prefab with CardComponent
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
    // 🔒 Internal cauldron state
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

    private void StartSingleIngredientProcess(string ingredientName)
    {
        CardComponent card = null;
        int count = 0;

        foreach (Transform child in recipeHoldingParent)
        {
            var c = child.GetComponent<CardComponent>();
            if (c != null)
            {
                card = c;
                count++;
            }
        }

        if (count != 1)
            return;

        var recipe = card.CardData.processingRecipes
            .Find(r => r.tool == ProcessingTool.Cauldron);

        if (recipe == null)
        {
            Debug.LogWarning("No cauldron recipe for card");
            return;
        }

        // Check if already processed (prevent reprocessing)
        if (card.CardData.processedType != ProcessedType.None &&
            (recipe.visualOutputs == null || recipe.visualOutputs.Count == 0))
        {
            Debug.Log($"Cannot reprocess '{card.CardData.cardName}' - already processed");
            ReturnCardToRecipeHolding(card);
            return;
        }

        if (recipe.needsFire && (fireController == null || !fireController.IsFireOn))
        {
            Debug.LogWarning("🔥 Fire required for this process");
            return;
        }

        activeCombo = null;   // 🔴 IMPORTANT: marks this as NOT a spell
        isBrewing = true;

        double now = TimeManager.TotalGameMinutes;
        double minutes = recipe.processingTime * TimeManager.MinutesPerRealSecond;

        finishAtGameMinutes = now + minutes;
        totalBrewTime = (float)minutes;
        brewTimeRemaining = (float)minutes;

        processingSingleCard = card;

        // Hide card during processing
        card.gameObject.SetActive(false);

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
            // Could set a default processing sprite here if you have one
        }

        brewCoroutine = StartCoroutine(BrewRoutine(null));
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
        // 🔹 SINGLE INGREDIENT MODE
        if (ingredients != null && ingredients.Count == 1)
        {
            StartSingleIngredientProcess(ingredients[0]);
            return;
        }

        if (isBrewing)
        {
            Debug.Log("Cauldron is already brewing!");
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

        // record the absolute finish time
        double now = TimeManager.TotalGameMinutes;

        // brewTime is in seconds → convert to game minutes
        double brewMinutes = brewTime * TimeManager.MinutesPerRealSecond;

        finishAtGameMinutes = now + brewMinutes;
        brewTimeRemaining = (float)brewMinutes;
        totalBrewTime = (float)brewMinutes;

        // Show cauldron contents
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

        // Show timer
        if (toolTimerRoot != null)
            toolTimerRoot.SetActive(true);

        if (toolTimerSlider != null)
        {
            toolTimerSlider.maxValue = totalBrewTime;
            toolTimerSlider.value = totalBrewTime;
            toolTimerSlider.gameObject.SetActive(true);
        }

        // Ensure the cauldron visuals are alive
        if (!enabled)
            enabled = true;

        if (bubbleUI != null)
            bubbleUI.gameObject.SetActive(true);

        if (fireController != null)
            fireController.gameObject.SetActive(true);

        brewCoroutine = StartCoroutine(BrewRoutine(ingredients));
    }

    private IEnumerator BrewRoutine(List<string> ingredients)
    {
        while (true)
        {
            double now = TimeManager.TotalGameMinutes;
            double remaining = finishAtGameMinutes - now;

            brewTimeRemaining = Mathf.Max(0f, (float)remaining);

            if (toolTimerSlider != null)
                toolTimerSlider.value = brewTimeRemaining;

            if (brewTimeRemaining <= 0f)
                break;

            yield return null;
        }

        CompleteBrewing(ingredients);
    }

    private void CompleteBrewing(List<string> ingredients)
    {
        // 🔹 SINGLE INGREDIENT COMPLETE
        if (processingSingleCard != null)
        {
            var recipe = processingSingleCard.CardData.processingRecipes
                .Find(r => r.tool == ProcessingTool.Cauldron);

            // Mark as processed with the correct type
            processingSingleCard.CardData.processedType = recipe.processedResultType;
            processingSingleCard.MarkAsProcessed(); // This should add the visual icon

            // Update the card name to include the processed type (like WorkbenchStation does)
            string processedSuffix = recipe.processedResultType.ToString();
            string currentName = processingSingleCard.CardData.cardName;

            if (!currentName.EndsWith(" " + processedSuffix))
            {
                processingSingleCard.CardData.cardName = currentName + " " + processedSuffix;
            }

            // Refresh the card visuals to show the new name and icon
            processingSingleCard.SetCardData(processingSingleCard.CardData, true);

            // Move to output parent
            processingSingleCard.transform.SetParent(outputParent, false);
            processingSingleCard.transform.localPosition = Vector3.zero;
            processingSingleCard.gameObject.SetActive(true);

            // Clean up
            processingSingleCard = null;
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
                // Store failed brew with just ingredient names
                // The FailedBrewPanelController will look up the CardData when displaying
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

        //Debug.Log($"✅ Brew complete: {activeCombo.SpellName}");
        activeCombo = null;
    }

    private IEnumerator HideAfterDelay(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null)
            obj.SetActive(false);
    }

    // NOTE: second parameter is now *finishTimeUtcOa*, not remaining seconds
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

        // Restore visuals
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