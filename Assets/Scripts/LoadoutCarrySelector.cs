using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutCarrySelector : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] List<SOCarryItem> availableCarry;
    [SerializeField] LoadoutScreen loadoutScreen;

    [Header("UI")]
    [SerializeField] LoadoutCarryCard slotCard;

    [SerializeField] Button leftArrow;
    [SerializeField] Button rightArrow;

    int selectedIndex = 0;

    void Start()
    {
        RefreshSlot();
    }

    // ---------------------------------------------------------
    // BUTTONS
    // ---------------------------------------------------------

    public void ScrollLeft()
    {
        if (selectedIndex <= 0) return;

        selectedIndex--;
        RefreshSlot();
    }

    public void ScrollRight()
    {
        if (selectedIndex >= availableCarry.Count - 1) return;

        selectedIndex++;
        RefreshSlot();
    }

    // ---------------------------------------------------------
    // UPDATE DISPLAY
    // ---------------------------------------------------------

    void RefreshSlot()
    {
        slotCard.Initialize(availableCarry[selectedIndex]);

        if (loadoutScreen)
            loadoutScreen.SelectCarry(availableCarry[selectedIndex]);

        leftArrow.interactable = selectedIndex > 0;
        rightArrow.interactable = selectedIndex < availableCarry.Count - 1;
    }

    // ---------------------------------------------------------
    // ACCESS
    // ---------------------------------------------------------

    public SOCarryItem GetSelectedCarry()
    {
        return availableCarry[selectedIndex];
    }
}