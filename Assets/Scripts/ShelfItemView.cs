using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShelfItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image shelfContainerImage;
    //[SerializeField] private Image shelfContentsImage;

    private CardComponent sourceCard;

    public void Bind(CardComponent card)
    {
        Debug.Log($"[ShelfItemView] Binding card: {card.CardData.cardName}");

        if (card.CardData.shelfVisuals == null)
        {
            Debug.LogError($"[ShelfItemView] shelfVisuals is NULL for {card.CardData.cardName}");
            return;
        }

        if (card.CardData.shelfVisuals.containerSprite == null)
        {
            Debug.LogError($"[ShelfItemView] containerSprite is NULL for {card.CardData.cardName}");
            return;
        }

        Debug.Log($"[ShelfItemView] Assigning sprite to Image: {shelfContainerImage.name}", shelfContainerImage);
        Debug.Log($"[ShelfItemView] Applying sprite: {card.CardData.shelfVisuals.containerSprite.name}");

        shelfContainerImage.sprite = card.CardData.shelfVisuals.containerSprite;
        shelfContainerImage.enabled = true;
    }

    //public void Bind(CardComponent card)
    //{
    //    var visuals = card.CardData.shelfVisuals;

    //    if (visuals == null)
    //    {
    //        Debug.LogWarning($"[ShelfItemView] No shelf visuals for {card.CardData.cardName}");
    //        return;
    //    }

    //    shelfContainerImage.sprite = visuals.containerSprite;
    //    shelfContainerImage.enabled = visuals.containerSprite != null;

    //    //shelfContentsImage.sprite = visuals.contentsSprite;
    //    //shelfContentsImage.color = visuals.contentsColor;
    //    //shelfContentsImage.enabled = visuals.contentsSprite != null;
    //}


    public CardComponent GetSourceCard()
    {
        return sourceCard;
    }
}