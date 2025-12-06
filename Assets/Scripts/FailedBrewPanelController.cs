using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FailedBrewPanelController : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text failedBrewText;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void ShowFailedBrews()
    {
        if (panel != null)
            panel.SetActive(true);

        RefreshUI();
    }

    public void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void RefreshUI()
    {
        if (failedBrewText == null)
            return;

        failedBrewText.text = "";

        var failedList = GameData.Instance.failedBrews;

        foreach (var combo in failedList)
        {
            string ingredients = string.Join(", ", combo.Ingredients);
            failedBrewText.text += $"{combo.SpellName}: {ingredients}\n";
        }
    }
}