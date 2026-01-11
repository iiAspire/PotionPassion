using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FailedBrewCardView : MonoBehaviour
{
    public TMP_Text spellNameText;
    public Transform ingredientIconsParent;
    public GameObject ingredientIconPrefab;

    public void Bind(SpellCombo combo)
    {
        spellNameText.text = combo.SpellName;

        foreach (Transform child in ingredientIconsParent)
            Destroy(child.gameObject);

        foreach (string ingredient in combo.Ingredients)
        {
            Sprite typeIcon = IngredientIconResolver.GetItemTypeIcon(ingredient);
            Sprite partIcon = IngredientIconResolver.GetPartIcon(ingredient);

            if (typeIcon != null)
                CreateIcon(typeIcon);

            if (partIcon != null)
                CreateIcon(partIcon);
        }
    }

    private void CreateIcon(Sprite sprite)
    {
        var go = Instantiate(ingredientIconPrefab, ingredientIconsParent);
        go.GetComponent<Image>().sprite = sprite;
    }
}