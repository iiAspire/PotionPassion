using UnityEngine;

[CreateAssetMenu(fileName = "CardSpriteLibrary", menuName = "Card System/Card Sprite Library")]
public class CardSpriteLibrary : ScriptableObject
{
    [Header("Numeral Sprites (1–10)")]
    public Sprite[] numeralSprites;

    [Header("Suit Sprites")]
    public Sprite wandsSprite;
    public Sprite cupsSprite;
    public Sprite pentaclesSprite;
    public Sprite swordsSprite;

    [Header("Optional Season Sprites")]
    public Sprite springSprite;
    public Sprite summerSprite;
    public Sprite autumnSprite;
    public Sprite winterSprite;

    public Sprite GetNumeralSprite(int numeral)
    {
        if (numeralSprites != null && numeral > 0 && numeral <= numeralSprites.Length)
            return numeralSprites[numeral - 1];
        return null;
    }

    public Sprite GetSuitSprite(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Wands => wandsSprite,
            CardSuit.Cups => cupsSprite,
            CardSuit.Pentacles => pentaclesSprite,
            CardSuit.Swords => swordsSprite,
            _ => null
        };
    }

    public Sprite GetSeasonSprite(CardSeason season)
    {
        return season switch
        {
            CardSeason.Spring => springSprite,
            CardSeason.Summer => summerSprite,
            CardSeason.Autumn => autumnSprite,
            CardSeason.Winter => winterSprite,
            _ => null
        };
    }

}