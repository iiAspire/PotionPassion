using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ExpeditionResolver
{
    public static List<RegionFind> GetPossibleFinds(
        LoadoutScreen loadout)
    {
        List<RegionFind> results = new();

        if (loadout.selectedRegion == null)
            return results;

        var region = loadout.selectedRegion;

        foreach (var find in region.possibleFinds)
        {
            if (find.resource == null)
                continue;

            // Tool requirement
            if (find.requiredTool != null &&
                !loadout.SelectedTools.Contains(find.requiredTool))
                continue;

            // Capacity requirement
            if (loadout.RemainingSlots <= 0)
                continue;

            //// Season check (if used)
            //if (find.seasonal)
            //{
            //    Season currentSeason =
            //        SeasonUtility.GetCurrentSeason();

            //    if (currentSeason != find.favoredSeason)
            //        continue;
            //}

            results.Add(find);
        }

        return results;
    }
}