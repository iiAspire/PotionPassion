using UnityEngine;
using UnityEngine.UI;

public class CardFabricator : MonoBehaviour
{
    [Header("UI References")]
    public Image numeralImageMain;
    public Image numeralImageSecondary;
    public Image suitImageMain;
    public Image suitImageSecondary;
    public Image seasonImageMain;
    public Image seasonImageSecondary;
    public Text namedText;
    public Image colorImage;

    public void UpdateCard(CardDataSO card)
    {
        if (!card) return;

        // Numerals
        if (numeralImageMain) numeralImageMain.sprite = card.numeralSprite;
        if (numeralImageSecondary) numeralImageSecondary.sprite = card.numeralSprite;

        // Suits
        if (suitImageMain) suitImageMain.sprite = card.suitSprite;
        if (suitImageSecondary) suitImageSecondary.sprite = card.suitSprite;

        // Season icons
        if (seasonImageMain) seasonImageMain.sprite = card.seasonSprite;
        if (seasonImageSecondary) seasonImageSecondary.sprite = card.seasonSprite;

        // Text and tint
        if (namedText) namedText.text = card.cardName;
        if (colorImage) colorImage.color = card.colorTint;
    }


    private Sprite GetSeasonSprite(CardSeason season)
    {
        // You could store season sprites in a static dictionary or ScriptableObject
        return null; // placeholder
    }
}