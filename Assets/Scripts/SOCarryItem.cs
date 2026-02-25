using UnityEngine;

[CreateAssetMenu(menuName = "Loadout/Carry Item")]
public class SOCarryItem : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public int slotCapacity;
    public bool handsFree;
    public float travelDistance = 1f;
}