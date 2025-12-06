using UnityEngine;

public enum CardSuit { Wands, Cups, Pentacles, Swords }
public enum CardColor { Black, Copper, Silver, Gold }
public enum CardSeason { Spring, Summer, Autumn, Winter }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data")]
public class CardDataSO : ScriptableObject
{
    [Header("Card Identity")]
    public CardSuit suit;
    public int numeral;           // 1–10
    public CardColor color;

    [Header("Card Appearance")]
    public string cardName;
    public Sprite numeralSprite;
    public Sprite suitSprite;
    public Sprite seasonSprite;
    public Color colorTint;

    [Header("Card Behavior")]
    public CardSeason season;     // assigned manually to affect delays or gameplay
    public float timeDelay;       // optional, if needed for season effects

    [Header("Auto-Fill Settings")]
    public CardSpriteLibrary spriteLibrary;

    private void OnValidate()
    {
        // Automatically set tint based on selected color
        switch (color)
        {
            case CardColor.Black:
                colorTint = new Color(0.1f, 0.1f, 0.1f);
                break;
            case CardColor.Copper:
                colorTint = new Color(0.72f, 0.45f, 0.2f);
                break;
            case CardColor.Silver:
                colorTint = new Color(0.75f, 0.75f, 0.75f);
                break;
            case CardColor.Gold:
                colorTint = new Color(0.93f, 0.79f, 0.25f);
                break;
        }

        // Auto-assign sprites from library
        if (spriteLibrary != null)
        {
            numeralSprite = spriteLibrary.GetNumeralSprite(numeral);
            suitSprite = spriteLibrary.GetSuitSprite(suit);
            seasonSprite = spriteLibrary.GetSeasonSprite(season);
        }
    }
}