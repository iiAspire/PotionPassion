using System.Collections.Generic;
using UnityEngine;
using TMPro; // Remove if not using TextMeshPro
using System;

public class LoadoutScreen : MonoBehaviour
{
    [Header("UI (Optional)")]
    public TMP_Text summaryText; // Can be null if you just log

    [Header("Current Selection")]
    public SOCarryItem selectedCarry;

    public event Action OnLoadoutChanged;

    private readonly List<SOToolItem> selectedTools = new();
    public IReadOnlyList<SOToolItem> SelectedTools => selectedTools;

    private bool handOccupied = false;
    private int usedSlots = 0;
    public float MaxTravelDistance => selectedCarry ? selectedCarry.travelDistance : 0f;

    private SOToolItem handTool = null;
    private readonly List<SOToolItem> slotTools = new();
    public SOToolItem HandTool => handTool;
    public IReadOnlyList<SOToolItem> SlotTools => slotTools;

    // =========================================================
    // CARRY SELECTION
    // =========================================================

    public void SelectCarry(SOCarryItem carry)
    {
        selectedCarry = carry;

        // Re-evaluate tools against new carry
        Recalculate();

        UpdateSummary();
        OnLoadoutChanged?.Invoke();
    }

    // =========================================================
    // TOOL TOGGLE (hook to tool buttons)
    // =========================================================

    public bool ToggleTool(SOToolItem tool, out string reason)
    {
        reason = "";

        if (selectedCarry == null)
        {
            reason = "Select a carry item first.";
            return false;
        }

        // Remove if already selected
        if (selectedTools.Contains(tool))
        {
            selectedTools.Remove(tool);
            Recalculate();
            UpdateSummary();
            OnLoadoutChanged?.Invoke();
            return true;
        }

        // Try to add
        if (TryAddTool(tool, out reason))
        {
            UpdateSummary();
            OnLoadoutChanged?.Invoke();
            return true;
        }

        return false;
    }

    // =========================================================
    // CORE RULES
    // =========================================================

    private bool TryAddTool(SOToolItem tool, out string reason)
    {
        reason = "";

        // --- Try hand ---
        if (tool.requiresHand)
        {
            if (selectedCarry.handsFree && handTool == null)
            {
                selectedTools.Add(tool);
                handTool = tool;
                handOccupied = true;
                return true;
            }
        }

        // --- Try storage ---
        if (tool.canBeStored)
        {
            bool storageAllowed = true;

            if (tool.restrictedStorage)
            {
                storageAllowed =
                    tool.allowedCarries != null &&
                    tool.allowedCarries.Contains(selectedCarry);

                if (!storageAllowed)
                {
                    reason = "Too bulky for this pack.";
                    return false;
                }
            }

            if (usedSlots >= selectedCarry.slotCapacity)
            {
                reason = "No storage slots available.";
                return false;
            }

            selectedTools.Add(tool);
            slotTools.Add(tool);
            usedSlots++;
            return true;
        }

        reason = "Requires a free hand, may be storable.";
        return false;
    }

    private void Recalculate()
    {
        handTool = null;
        slotTools.Clear();

        handOccupied = false;
        usedSlots = 0;

        var toolsCopy = new List<SOToolItem>(selectedTools);
        selectedTools.Clear();

        foreach (var tool in toolsCopy)
        {
            TryAddTool(tool, out _);
        }
    }

    // =========================================================
    // INFO PROPERTIES
    // =========================================================

    public int RemainingSlots =>
        selectedCarry ? selectedCarry.slotCapacity - usedSlots : 0;

    public bool HandAvailable =>
        selectedCarry && selectedCarry.handsFree && !handOccupied;

    // =========================================================
    // SUMMARY
    // =========================================================

    private void UpdateSummary()
    {
        if (selectedCarry == null)
        {
            SetSummary("No carry item selected.");
            return;
        }

        string toolsText =
            selectedTools.Count == 0
            ? "None"
            : string.Join(", ", selectedTools.ConvertAll(t => t.displayName));

        string summary =
$@"Carry: {selectedCarry.displayName}
Slots: {usedSlots}/{selectedCarry.slotCapacity}
Hands: {(HandAvailable ? "Free" : "Occupied")}
Tools: {toolsText}";

        SetSummary(summary);
    }

    private void SetSummary(string text)
    {
        if (summaryText)
            summaryText.text = text;
        else
            Debug.Log(text);
    }
}