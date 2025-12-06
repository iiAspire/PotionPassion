using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void TickByMinutes(float minutes)
    {
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
                slot.active = false;
                slot.elapsedTime = slot.totalTime;

                if (slot.timerSlider != null)
                    slot.timerSlider.gameObject.SetActive(false);
                if (slot.iconImage != null)
                    slot.iconImage.gameObject.SetActive(false);
                if (slot.timerFrame != null)
                    slot.timerFrame.SetActive(false);

                // Apply processed result + return to inventory
                if (slot.card != null)
                {
                    CardComponent card = slot.card;
                    ProcessingRecipe recipe = slot.recipe;

                    // --- CASE 1: visual output(s), like Chia → Chia Oil ---
                    if (recipe != null && recipe.visualOutputs != null && recipe.visualOutputs.Count > 0)
                    {
                        foreach (var output in recipe.visualOutputs)
                        {
                            for (int i = 0; i < output.quantity; i++)
                            {
                                // get template from your CardManager
                                CardData template = cardManager.GetCardByName(output.name);

                                if (template == null)
                                {
                                    Debug.LogWarning("No card asset for visual output: " + output.name);
                                    continue;
                                }

                                // make runtime copy
                                CardData runtimeCopy = ScriptableObject.Instantiate(template);

                                // spawn in inventory
                                GameObject cardGO = Instantiate(template.cardPrefab, playerInventoryParent);
                                CardComponent newCard = cardGO.GetComponent<CardComponent>();
                                newCard.SetCardData(runtimeCopy, true);

                                newCard.transform.localPosition = Vector3.zero;
                            }
                        }

                        // destroy original (same as workstation)
                        Destroy(card.gameObject);
                    }
                    else
                    {
                        // --- CASE 2: processed icon only (no new card prefab) ---
                        if (recipe != null)
                            card.CardData.processedType = recipe.processedResultType;

                        card.MarkAsProcessed();

                        card.transform.SetParent(playerInventoryParent, false);
                        card.transform.localPosition = Vector3.zero;
                        card.gameObject.SetActive(true);
                    }

                    slot.card = null;
                    slot.recipe = null;
                }
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

            if (!slot.active)
            {
                slot.card = card;
                slot.recipe = recipe;
                slot.totalTime = processingTime;
                slot.elapsedTime = 0f;
                slot.active = true;

                if (slot.iconImage != null)
                {
                    slot.timerFrame.SetActive(true);  // show everything together
                    slot.iconImage.sprite = card.CardData.Icon;
                    slot.timerSlider.value = 1f;
                }
                card.gameObject.SetActive(false);

                return true;
            }
        }
        return false;
    }
}