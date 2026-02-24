using UnityEngine;

[CreateAssetMenu(menuName = "Loadout/Tool")]
public class SOToolItem : ScriptableObject
{
    public string displayName;
    public Sprite icon;

    [Header("Handling Requirements")]
    public bool requiresHand = true;
    public bool canBeStored = true;
}