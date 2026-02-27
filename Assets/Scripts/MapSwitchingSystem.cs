using UnityEngine;

public static class MapSwitchSystem
{
    // Hub nodes for each map (you can load this from data later)
    public static int GetHubNodeForMap(int mapIndex)
    {
        return mapIndex switch
        {
            1 => 1001,
            2 => 2001,
            3 => 3001,
            4 => 4001,
            _ => 1001
        };
    }

    public static void SwitchToMap(int mapIndex)
    {
        // Pay a day here (your time system)
        // TimeManager.Instance.AdvanceDay();   // or whatever your call is

        WorldState.CurrentNodeID = GetHubNodeForMap(mapIndex);
        Debug.Log($"Switched to map {mapIndex}. Current node = {WorldState.CurrentNodeID}");
    }
}