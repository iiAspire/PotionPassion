using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardPersistenceManager;

public class DryingRackTimer : MonoBehaviour
{
    [Header("Testing - DryingRackTimer")]
    [Tooltip("Multiply timer speed for testing. 2 = twice as fast, 0.5 = half speed.")]
    public float timerSpeedMultiplier = 1f;

    [Header("Inventory Target")]
    public Transform playerInventoryParent;

    [Header("UI References")]
    public Image backgroundImage;       // Drying rack sprite
    public List<DryingRackSlot> slots;  // 5 slots assigned in inspector

    private bool hasRestored = false;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float fadedAlpha = 0.5f;     // Alpha when any slot is active

    [Header("Card Manager")]
    public CardManager cardManager;

    private Color originalColor;

    private void Awake()
    {
        if (backgroundImage != null)
            originalColor = backgroundImage.color;

        foreach (var slot in slots)
        {
            if (slot.timerSlider != null)
            {
                slot.timerSlider.value = 0f;
            }

            slot.timerFrame.SetActive(false);
        }
    }

    private void ClearSlot(DryingRackSlot slot)
    {
        slot.card = null;
        slot.recipe = null;
        slot.active = false;
        slot.completed = false;
        slot.elapsedTime = 0f;
        slot.totalTime = 0f;

        if (slot.timerFrame != null)
            slot.timerFrame.SetActive(false);

        if (slot.timerSlider != null)
            slot.timerSlider.gameObject.SetActive(false);

        if (slot.iconImage != null)
            slot.iconImage.gameObject.SetActive(false);
    }

    public void TickByMinutes(float minutes)
    {
        // 🔄 Auto-clear slots whose completed card has been moved away
        foreach (var slot in slots)
        {
            if (!slot.completed)
                continue;

            if (slot.card == null ||
                slot.slotAnchor == null ||
                slot.card.transform.parent != slot.slotAnchor)
            {
                ClearSlot(slot);
            }
        }

        bool anyActive = false;

        foreach (var slot in slots)
        {
            if (!slot.active || slot.card == null)
                continue;

            slot.elapsedTime += minutes;

            // update UI
            if (slot.timerSlider != null)
                slot.timerSlider.value = 1f - Mathf.Clamp01(slot.elapsedTime / slot.totalTime);

            anyActive = true;

            if (slot.elapsedTime >= slot.totalTime)
            {
                slot.elapsedTime = slot.totalTime;
                slot.active = false;
                slot.completed = true;

                // Hide timer UI
                if (slot.timerSlider != null)
                    slot.timerSlider.gameObject.SetActive(false);

                if (slot.timerFrame != null)
                    slot.timerFrame.SetActive(false);

                CardComponent card = slot.card;
                ProcessingRecipe recipe = slot.recipe;

                // Apply output (single-card model)
                if (recipe != null && recipe.visualOutputs != null && recipe.visualOutputs.Count > 0)
                {
                    var output = recipe.visualOutputs[0];
                    CardData template = cardManager.GetCardByName(output.name);

                    if (template != null)
                    {
                        CardData runtimeCopy = ScriptableObject.Instantiate(template);
                        card.SetCardData(runtimeCopy, true);
                    }
                }
                else if (recipe != null)
                {
                    card.CardData.processedType = recipe.processedResultType;
                    card.MarkAsProcessed();
                }

                // Show card on rack
                card.gameObject.SetActive(true);
                card.transform.SetParent(slot.slotAnchor, false);
                card.transform.localPosition = Vector3.zero;

                continue;
            }
        }

        // fade background only once per tick
        if (backgroundImage != null)
        {
            Color c = backgroundImage.color;
            c.a = anyActive ? fadedAlpha : originalColor.a;
            backgroundImage.color = c;
        }


        // Fade background if any slot is active
        if (backgroundImage != null)
        {
            float targetAlpha = anyActive ? fadedAlpha : originalColor.a;
            Color c = backgroundImage.color;
            c.a = targetAlpha;
            backgroundImage.color = c;
        }
    }

    /// <summary>
    /// Adds a card to the first available slot.
    /// Returns true if successfully added, false if all slots are full.
    /// </summary>
    public bool AddCard(CardComponent card, float processingTime, ProcessingRecipe recipe)
    {
        foreach (var slot in slots)
        {
            //    card.gameObject.SetActive(false);

            if (!slot.active && !slot.completed)
            {
                slot.card = card;
                slot.recipe = recipe;
                slot.totalTime = processingTime;
                slot.elapsedTime = 0f;
                slot.active = true;

                if (slot.timerFrame != null)
                    slot.timerFrame.SetActive(true);

                if (slot.iconImage != null)
                {
                    slot.iconImage.gameObject.SetActive(true);
                    slot.iconImage.sprite = card.CardData.Icon;
                }

                if (slot.timerSlider != null)
                {
                    slot.timerSlider.gameObject.SetActive(true);
                    slot.timerSlider.value = 1f;
                }
                card.gameObject.SetActive(false);

                return true;
            }
        }
        return false;
    }

    public void Save()
    {
        if (GameData.Instance == null)
            return;

        double now = TimeManager.TotalGameMinutes;

        GameData.Instance.savedDryingRack.Clear();

        foreach (var slot in slots)
        {
            if (slot.card == null)
                continue;

            double finishAt = now + (slot.totalTime - slot.elapsedTime);

            GameData.Instance.savedDryingRack.Add(new SavedDryingRackProcess
            {
                cardRuntimeID = slot.card.RuntimeID,
                cardName = slot.card.CardData.cardName,
                baseName = slot.card.CardData.baseName,
                completed = slot.completed,
                finishAtGameMinutes = slot.completed
                    ? 0
                    : now + (slot.totalTime - slot.elapsedTime)
            });
        }
    }

    public void RestoreFromSave()
    {
        if (hasRestored)
            return;

        hasRestored = true;

        if (GameData.Instance == null)
            return;

        var saved = GameData.Instance.savedDryingRack;
        if (saved == null || saved.Count == 0)
            return;

        double now = TimeManager.TotalGameMinutes;

        foreach (var state in saved)
        {
            // Find a free slot
            DryingRackSlot slot = null;
            foreach (var s in slots)
            {
                if (!s.active && !s.completed)
                {
                    slot = s;
                    break;
                }
            }

            if (slot == null)
            {
                Debug.LogWarning("No free drying rack slot during restore");
                break;
            }

            // Recreate card
            CardData template = cardManager.GetCardByName(state.baseName);
            if (template == null)
            {
                Debug.LogWarning($"No CardData template for '{state.baseName}'");
                continue;
            }

            CardData runtime = ScriptableObject.CreateInstance<CardData>();
            runtime.CopyFrom(template);
            runtime.cardName = state.cardName;
            runtime.baseName = state.baseName;

            GameObject cardGO = Instantiate(template.cardPrefab, transform);
            CardComponent card = cardGO.GetComponent<CardComponent>();
            card.runtimeID = state.cardRuntimeID;
            card.SetCardData(runtime, true);
            card.gameObject.SetActive(false);

            // Find recipe
            ProcessingRecipe recipe =
                runtime.processingRecipes.Find(r => r.tool == ProcessingTool.DryingRack);

            if (recipe == null)
            {
                Debug.LogWarning($"No drying rack recipe for '{runtime.cardName}'");
                Destroy(cardGO);
                continue;
            }

            if (state.completed)
            {
                slot.card = card;
                slot.recipe = recipe;
                slot.active = false;
                slot.completed = true;

                card.gameObject.SetActive(true);
                card.transform.SetParent(slot.slotAnchor, false);
                card.transform.localPosition = Vector3.zero;

                continue; // do NOT run timer logic
            }

            // -------- NEW TIME LOGIC (PASSIVE, GAME-TIME BASED) --------

            double remaining = state.finishAtGameMinutes - now;
            if (remaining < 0)
                remaining = 0;

            slot.card = card;
            slot.recipe = recipe;
            slot.totalTime = recipe.processingTime;

            slot.elapsedTime = slot.totalTime - (float)remaining;
            slot.elapsedTime = Mathf.Clamp(slot.elapsedTime, 0f, slot.totalTime);

            slot.active = true;


            // UI restore (safe)
            if (slot.timerFrame != null)
                slot.timerFrame.SetActive(true);

            if (slot.iconImage != null)
            {
                slot.iconImage.sprite = runtime.Icon;
                slot.iconImage.gameObject.SetActive(true);
            }

            if (slot.timerSlider != null)
            {
                slot.timerSlider.gameObject.SetActive(true);
                if (slot.totalTime > 0f)
                    slot.timerSlider.value = 1f - (slot.elapsedTime / slot.totalTime);
            }
        }

        GameData.Instance.savedDryingRack.Clear();
    }
}