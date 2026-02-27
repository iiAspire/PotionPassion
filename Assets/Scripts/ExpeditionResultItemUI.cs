using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExpeditionResultItemUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text qtyText;

    public void Setup(CardGain gain)
    {
        if (gain == null || gain.card == null) return;

        if (icon) icon.sprite = gain.card.Icon;
        if (nameText) nameText.text = gain.card.cardName;
        if (qtyText) qtyText.text = "x" + gain.amount;
    }
}