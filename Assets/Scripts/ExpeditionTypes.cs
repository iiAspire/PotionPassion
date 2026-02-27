using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardGain
{
    public CardData card;
    public int amount = 1;
    public string note;
}

[System.Serializable]
public class ExpeditionResult
{
    public SORegion region;
    public SOCarryItem carry;
    public List<SOToolItem> tools = new();

    public float travelTime;        // one way
    public float returnTime;        // one way back (usually same)
    public float harvestTimeTaken;  // actual time spent harvesting
    public float totalTimeTaken;

    public float maxTravelTime;

    public List<CardGain> gains = new();

    public bool success;
    public string summary;
}

public static class ExpeditionState
{
    public static ExpeditionResult LastResult;
}