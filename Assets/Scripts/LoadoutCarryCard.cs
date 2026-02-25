using UnityEngine;
using UnityEngine.UI;

public class LoadoutCarryCard : MonoBehaviour
{
    [SerializeField] LoadoutCarryCard slotCard;
    [SerializeField] Image icon;

    public SOCarryItem CarryData { get; private set; }

    public void Initialize(SOCarryItem data)
    {
        CarryData = data;
        icon.sprite = data.icon;
    }
}