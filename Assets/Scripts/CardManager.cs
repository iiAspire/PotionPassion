using UnityEngine;

[CreateAssetMenu(fileName = "CardManager", menuName = "Cards/Card Manager")]
public class CardManager : ScriptableObject
{
    public CardData[] allCards;
    public GameObject cardStackPrefab;

    public CardData GetCardByName(string name)
    {
        foreach (var card in allCards)
        {
            if (card.cardName == name)
                return card;
        }
        return null;
    }
}