using System.Collections.Generic;
using UnityEngine;
using TMPro; // Remove if not using TextMeshPro
using System;
using System.Linq;

public class LoadoutScreen : MonoBehaviour
{
    [Header("UI (Optional)")]
    public TMP_Text summaryText; // Can be null if you just log

    [Header("Current Selection")]
    public SOCarryItem selectedCarry;

    [Header("Region")]
    public SORegion selectedRegion;

    public event Action OnLoadoutChanged;

    private readonly List<SOToolItem> selectedTools = new();
    public IReadOnlyList<SOToolItem> SelectedTools => selectedTools;

    private bool handOccupied = false;
    private int usedSlots = 0;

    private SOToolItem handTool = null;
    private readonly List<SOToolItem> slotTools = new();
    public SOToolItem HandTool => handTool;
    public IReadOnlyList<SOToolItem> SlotTools => slotTools;


    private float GetFindSeasonMultiplier(RegionFind f)
    {
        if (TimeManager.Instance == null) return 1f;

        Season season = TimeManager.Instance.Calendar.CurrentSeason;

        return season switch
        {
            Season.Winter => f.winter,
            Season.Spring => f.spring,
            Season.Summer => f.summer,
            Season.Autumn => f.autumn,
            _ => 1f
        };
    }

    public float MaxTravelTime
    {
        get
        {
            if (!selectedCarry) return 0f;

            // Base time allowance from the carry item (minutes)
            float time = selectedCarry.travelTimeBudget;

            // Only apply penalties if something is actually in the hand
            if (HandTool != null)
            {
                // Apply multiplier (e.g. 0.75 for -25%)
                time *= HandTool.travelMultiplier;

                // Apply cap (e.g. 30 for bucket). 0 means "no cap".
                if (HandTool.travelCapMinutes > 0f)
                    time = Mathf.Min(time, HandTool.travelCapMinutes);
            }

            return time;
        }
    }

    public float ExpectedHarvestTime
    {
        get
        {
            if (selectedRegion == null) return 0f;

            float expected = 0f;

            foreach (var f in selectedRegion.possibleFinds)
            {
                if (f == null || f.resource == null) continue;

                bool hasTool = f.requiredTool == null || SelectedTools.Contains(f.requiredTool);
                if (f.toolEssential && !hasTool) continue;

                float seasonMult = GetFindSeasonMultiplier(f);
                float p = Mathf.Clamp01(f.chance * seasonMult);

                expected += p * Mathf.Max(0f, f.harvestMinutes);
            }

            return expected;
        }
    }

    public float MaxHarvestTime
    {
        get
        {
            if (selectedRegion == null) return 0f;

            float max = 0f;

            foreach (var f in selectedRegion.possibleFinds)
            {
                if (f == null || f.resource == null) continue;

                bool hasTool = f.requiredTool == null || SelectedTools.Contains(f.requiredTool);
                if (f.toolEssential && !hasTool) continue;

                max += Mathf.Max(0f, f.harvestMinutes);
            }

            return max;
        }
    }

    public float RoundTripTravelTime
    {
        get
        {
            float oneWay = TravelTimeToSelectedRegion;
            if (float.IsInfinity(oneWay)) return float.PositiveInfinity;
            return oneWay * 2f;
        }
    }


    // =========================================================
    // SELECT REGION
    // =========================================================

    public void SelectRegion(SORegion region)
    {
        selectedRegion = region;

        Recalculate();     // Re-apply constraints
        UpdateSummary();
        OnLoadoutChanged?.Invoke();
    }

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
                    reason = $"{tool.displayName} is too bulky to store in this pack.";
                    return false;
                }
            }

            int capacity = selectedCarry.slotCapacity;

            if (usedSlots >= capacity)
            {
                reason = "No storage slots available.";
                return false;
            }

            selectedTools.Add(tool);
            slotTools.Add(tool);
            usedSlots++;
            return true;
        }

        reason = $"{tool.displayName} requires a free hand, may also be stored but in a different pack.";
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

    public float TravelTimeToSelectedRegion
    {
        get
        {
            if (selectedRegion == null)
                return float.PositiveInfinity;

            var current = RegionLookup.Get(WorldState.CurrentNodeID);
            if (current == null)
                return float.PositiveInfinity;

            foreach (var edge in current.edges)
            {
                if (edge != null && edge.destination == selectedRegion)
                    return edge.travelTime;
            }

            // No direct connection from current node
            return float.PositiveInfinity;
        }
    }

    public bool CanReachSelectedRegion
    {
        get
        {
            if (!selectedCarry || selectedRegion == null)
                return false;

            float t = TravelTimeToSelectedRegion;
            return !float.IsInfinity(t) && t <= MaxTravelTime;
        }
    }

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

        string regionText =
            selectedRegion ? selectedRegion.locationName : "None";

        string toolsText =
            selectedTools.Count == 0
            ? "None"
            : string.Join(", ", selectedTools.ConvertAll(t => t.displayName));

        string summary =
            $@"Region: {regionText}
            Carry: {selectedCarry.displayName}
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

    public bool HasDestination => selectedRegion != null;
    public bool HasCarry => selectedCarry != null;

    // travel time to selected region should already be computed from edges / graph
    public bool IsSelectedRegionReachable =>
        selectedRegion != null && !float.IsInfinity(TravelTimeToSelectedRegion);

    public float ExpectedTotalTime => RoundTripTravelTime + ExpectedHarvestTime;

    public bool CanConfirmExpedition
    {
        get
        {
            if (!HasDestination || !HasCarry) return false;
            if (!IsSelectedRegionReachable) return false;

            float oneWay = TravelTimeToSelectedRegion;

            return oneWay <= MaxTravelTime;
        }
    }

    public string ConfirmBlockReason
    {
        get
        {
            if (selectedRegion == null)
                return "Select a destination.";

            if (selectedCarry == null)
                return "Select a carry item.";

            if (!IsSelectedRegionReachable)
                return "Destination not reachable from here.";

            float oneWay = TravelTimeToSelectedRegion;
            float max = MaxTravelTime;

            if (oneWay > max)
                return $"Too far with current choices. Requires {oneWay:0} min, limit {max:0} min.";

            if (RemainingSlots <= 0)
                return "No space to bring finds back.";

            return null;
        }
    }
}