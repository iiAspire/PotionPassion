using UnityEngine;

public enum EquipmentType
{
    Carry,
    Tool
}

public abstract class SOEquipmentItem : ScriptableObject
{
    public string displayName;
    public EquipmentType type;
}