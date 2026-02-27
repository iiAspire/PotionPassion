using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Loadout/Tool")]
public class SOToolItem : ScriptableObject
{
    public string displayName;
    public Sprite icon;

    [Header("Handling Requirements")]
    [Tooltip("Requires a hand if not stored")]
    public bool requiresHand = false;

    [Tooltip("Can be placed in carry slots")]
    public bool canBeStored = true;

    [Tooltip("If true, can only be stored in specific carry types")]
    public bool restrictedStorage = false;

    [Tooltip("Carry items that allow storage when restricted")]
    public List<SOCarryItem> allowedCarries;

    public float travelMultiplier = 1f;
    public float travelCapMinutes = 0f; // 0 = no cap
    public float harvestMinutes = 0f;
}