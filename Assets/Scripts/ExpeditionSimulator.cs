using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ExpeditionSimulator
{
    public static ExpeditionResult Run(LoadoutScreen loadout)
    {
        var result = new ExpeditionResult
        {
            region = loadout.selectedRegion,
            carry = loadout.selectedCarry,
            tools = new List<SOToolItem>(loadout.SelectedTools),
            travelTime = loadout.TravelTimeToSelectedRegion,
            returnTime = loadout.TravelTimeToSelectedRegion,
            maxTravelTime = loadout.MaxTravelTime
        };

        if (result.region == null || result.carry == null)
        {
            result.success = false;
            result.summary = "No expedition configured.";
            return result;
        }

        if (float.IsInfinity(result.travelTime))
        {
            result.success = false;
            result.summary = "Destination unreachable.";
            return result;
        }

        GenerateGains(result);

        result.totalTimeTaken =
            result.travelTime +
            result.harvestTimeTaken +
            result.returnTime;

        result.success = result.gains.Count > 0;
        result.summary = result.success
            ? ""
            : "You found nothing of value.";

        return result;
    }

    static void GenerateGains(ExpeditionResult result)
    {
        if (result.region.possibleFinds == null) return;

        foreach (var f in result.region.possibleFinds)
        {
            if (f == null || f.resource == null)
                continue;

            bool hasTool = f.requiredTool == null || result.tools.Contains(f.requiredTool);

            if (f.toolEssential && !hasTool)
                continue;

            float seasonMult = GetFindSeasonMultiplier(f);
            float chance = Mathf.Clamp01(f.chance * seasonMult);

            bool boostsYield = false;

            if (f.requiredTool != null && !f.toolEssential)
            {
                if (!hasTool)
                    chance *= 0.5f;
                else
                    boostsYield = true;
            }

            if (Random.value <= chance)
            {
                if (result.gains.Any(g => g.card == f.resource))
                    continue;

                AddOrStack(result.gains, f.resource, 1, BuildNote(f, hasTool, boostsYield));

                // ✅ Time cost is per successful action
                result.harvestTimeTaken += Mathf.Max(0f, f.harvestMinutes);
            }
        }

        float roundTrip = result.travelTime * 2f;
        result.totalTimeTaken = roundTrip + result.harvestTimeTaken;
    }

    static float GetFindSeasonMultiplier(RegionFind f)
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

    static string BuildNote(RegionFind f, bool hasTool, bool boostsYield)
    {
        if (!string.IsNullOrWhiteSpace(f.notes))
            return f.notes;

        if (f.requiredTool == null) return null;

        if (f.toolEssential)
            return hasTool ? $"Used {f.requiredTool.displayName}." : null;

        return boostsYield ? $"Easier with {f.requiredTool.displayName}." : null;
    }

    static void AddOrStack(List<CardGain> gains, CardData card, int amount, string note)
    {
        for (int i = 0; i < gains.Count; i++)
        {
            if (gains[i].card == card)
            {
                gains[i].amount += amount;
                if (string.IsNullOrEmpty(gains[i].note) && !string.IsNullOrEmpty(note))
                    gains[i].note = note;
                return;
            }
        }

        gains.Add(new CardGain { card = card, amount = amount, note = note });
    }

    static void ClampToSlots(ExpeditionResult result)
    {
        int capacity = result.carry != null ? result.carry.slotCapacity : 0;
        if (capacity <= 0) return;

        if (result.gains.Count <= capacity) return;

        result.gains.RemoveRange(capacity, result.gains.Count - capacity);
        result.summary = "Limited by carry capacity. Some finds were left behind.";
    }
}