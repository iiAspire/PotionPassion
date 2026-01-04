using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShelfItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image shelfContainerImage;
    //[SerializeField] private Image shelfContentsImage;

    private CardComponent sourceCard;

    // ALTERNATE WHILE TESTING
    //public void Bind(CardComponent card)
    //{
    //    shelfContainerImage.sprite = card.CardData.shelfVisuals.containerSprite;
    //    shelfContainerImage.enabled = true;
    //}

    public void Bind(CardComponent card)
    {
        var visuals = card.CardData.shelfVisuals;

        if (visuals == null)
        {
            Debug.LogWarning($"[ShelfItemView] No shelf visuals for {card.CardData.cardName}");
            return;
        }

        shelfContainerImage.sprite = visuals.containerSprite;
        shelfContainerImage.enabled = visuals.containerSprite != null;

        //shelfContentsImage.sprite = visuals.contentsSprite;
        //shelfContentsImage.color = visuals.contentsColor;
        //shelfContentsImage.enabled = visuals.contentsSprite != null;
    }


    public CardComponent GetSourceCard()
    {
        return sourceCard;
    }
}