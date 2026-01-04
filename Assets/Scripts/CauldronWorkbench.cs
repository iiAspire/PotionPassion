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

    [Header("Feedback")]
    public Image cauldronContents;
    public GameObject recipeStatus;
    public TMP_Text failedBrewLogTMP;
    public CauldronBubbleUI bubbleUI;
    public CauldronSmokeUI smokeUI;
    public CauldronFireController fireController;

    private SpellCombo activeCombo;
    // 🔒 Internal cauldron state
    private bool isBrewing = false;
    private float totalBrewTime = 0f;
    private double finishTimeUtcOa = 0;
    private float brewTimeRemaining = 0f;
    public float TotalBrewTime => totalBrewTime;

    public bool IsBrewing => isBrewing;
    public string ActiveSpellName => activeCombo != null ? activeCombo.SpellName : null;
    public double FinishTimeUtcOa => finishTimeUtcOa;
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

    public void StartBrewing(SpellCombo combo, List<string> ingredients = null)
    {
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

        float brewTime = defaultBrewTime;

        totalBrewTime = brewTime;

        finishTimeUtcOa = DateTime.UtcNow.AddSeconds(brewTime).ToOADate();
        brewTimeRemaining = brewTime;

        // Use recipe time if available and valid
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
                        activeCombo = null; // forces failed path
                    }
                    else if (recipe.processingTimeWithFire > 0f)
                    {
                        brewTime = recipe.processingTimeWithFire;
                    }
                }
                else
                {
                    brewTime = recipe.processingTime;
                }
            }
        }

        // record the absolute finish time
        var finishTimeUtc = DateTime.UtcNow.AddSeconds(brewTime);
        finishTimeUtcOa = finishTimeUtc.ToOADate();
        brewTimeRemaining = brewTime;

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
            toolTimerSlider.maxValue = brewTime;
            toolTimerSlider.value = brewTime;
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
        //Debug.Log($"⏱ BrewRoutine started. enabled={enabled}, active={gameObject.activeInHierarchy}");

        while (true)
        {
            // compute remaining from absolute finish time
            var finishTime = DateTime.FromOADate(finishTimeUtcOa);
            var remaining = (float)(finishTime - DateTime.UtcNow).TotalSeconds;

            brewTimeRemaining = Mathf.Max(0f, remaining);

            if (toolTimerSlider != null)
                toolTimerSlider.value = brewTimeRemaining;

            //Debug.Log($"⏳ Remaining: {brewTimeRemaining:F2}");

            if (brewTimeRemaining <= 0f)
                break;

            yield return null;
        }

        CompleteBrewing(ingredients);
    }

    private void CompleteBrewing(List<string> ingredients)
    {
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
                GameData.Instance.failedBrews.Add(new SpellCombo
                {
                    SpellName = "Invalid Combo",
                    Ingredients = new List<string>(ingredients)
                });
            }

            //Debug.LogWarning("❌ Brew failed.");
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
        double finishTimeUtcOaFromSave,
        bool fireWasOn,
        float totalBrewTimeFromSave
    )
    {
        //Debug.Log($"RestoreFromSave called. Recipes count = {Recipes?.SpellCombos?.Count}");

        totalBrewTime = totalBrewTimeFromSave;

        // make sure we’re active
        if (!enabled)
            enabled = true;

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (string.IsNullOrEmpty(spellName))
            return;

        // Find the combo again from runtime database
        SpellCombo combo = GameInitialization.Recipes.GetComboByName(spellName);

        if (combo == null)
        {
            Debug.LogWarning($"⚠️ Could not restore brew for spell '{spellName}'");
            return;
        }

        activeCombo = combo;
        isBrewing = true;

        finishTimeUtcOa = finishTimeUtcOaFromSave;

        var finishTime = DateTime.FromOADate(finishTimeUtcOa);
        var remainingSeconds = (float)(finishTime - DateTime.UtcNow).TotalSeconds;

        // Restore fire state
        if (fireController != null)
            fireController.SetFire(fireWasOn);

        // Restore visuals
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
            toolTimerSlider.value = Mathf.Clamp(brewTimeRemaining, 0f, totalBrewTime);
            toolTimerSlider.gameObject.SetActive(true);
        }

        // If we're already past the finish time → complete instantly
        if (remainingSeconds <= 0f)
        {
            //Debug.Log("⏱ Offline brew finished while away, completing immediately.");
            brewTimeRemaining = 0f;
            CompleteBrewing(null);
            return;
        }

        // otherwise, resume countdown
        brewTimeRemaining = remainingSeconds;
        brewCoroutine = StartCoroutine(BrewRoutine(null));

        //Debug.Log($"🔄 Restored brewing '{spellName}' with {remainingSeconds:F1}s remaining");
    }
}