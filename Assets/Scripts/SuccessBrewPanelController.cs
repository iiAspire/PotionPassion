using UnityEngine;
using TMPro;

public class SuccessfulBrewPanelController : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text successfulBrewText;

    void OnEnable()
    {
        RefreshUI();
    }

    public void ShowSuccessfulBrews()
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
        if (successfulBrewText == null)
            return;

        successfulBrewText.text = "";

        var successfulList = GameData.Instance.successfulBrews;

        foreach (var combo in successfulList)
        {
            string ingredients = string.Join(", ", combo.Ingredients);
            successfulBrewText.text += $"{combo.SpellName}: {ingredients}\n";
        }
    }
}