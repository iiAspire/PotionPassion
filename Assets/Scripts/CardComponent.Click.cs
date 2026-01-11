using UnityEngine;
using UnityEngine.EventSystems;

public partial class CardComponent :
    MonoBehaviour,
    IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (CardData == null)
            return;

        CardClickLog.Instance?.Log(CardData.cardName);
    }
}