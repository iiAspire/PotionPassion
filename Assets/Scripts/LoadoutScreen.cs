using System.Collections.Generic;
using UnityEngine;
using TMPro; // Remove if not using TextMeshPro

public class LoadoutScreen : MonoBehaviour
{
    [Header("UI (Optional)")]
    public TMP_Text summaryText; // Can be null if you just log

    [Header("Current Selection")]
    public SOCarryItem selectedCarry;

    private readonly List<SOToolItem> selectedTools = new();

    private bool handOccupied = false;
    private int usedSlots = 0;

    // =========================================================
    // CARRY SELECTION
    // =========================================================

    public void SelectCarry(SOCarryItem carry)
    {
        selectedCarry = carry;

        // Re-evaluate tools against new carry
        Recalculate();

        UpdateSummary();
    }

    // =========================================================
    // TOOL TOGGLE (hook to tool buttons)
    // =========================================================

    public void ToggleTool(SOToolItem tool)
    {
        if (selectedCarry == null)
            return;

        // Remove if already selected
        if (selectedTools.Contains(tool))
        {
            selectedTools.Remove(tool);
            Recalculate();
            UpdateSummary();
            return;
        }

        // Try to add
        if (TryAddTool(tool))
        {
            UpdateSummary();
        }
    }

    // =========================================================
    // CORE RULES
    // =========================================================

    private bool TryAddTool(SOToolItem tool)
    {
        // --- Try to hold in hands ---
        if (tool.requiresHand && selectedCarry.handsFree && !handOccupied)
        {
            selectedTools.Add(tool);
            handOccupied = true;
            return true;
        }

        // --- Otherwise try storing in slots ---
        if (tool.canBeStored && usedSlots < selectedCarry.slotCapacity)
        {
            selectedTools.Add(tool);
            usedSlots++;
            return true;
        }

        // Not allowed
        return false;
    }

    private void Recalculate()
    {
        handOccupied = false;
        usedSlots = 0;

        var toolsCopy = new List<SOToolItem>(selectedTools);
        selectedTools.Clear();

        // Re-add tools using current rules
        foreach (var tool in toolsCopy)
        {
            TryAddTool(tool);
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