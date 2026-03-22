using UnityEngine;
using UnityEngine.UI;

public class StoreroomShelfSlot : MonoBehaviour
{
    [SerializeField] private Image shelfImage;
    [SerializeField] private CardData assignedCard;
    [SerializeField] private Sprite shelfSprite;

    public CardData AssignedCard => assignedCard;

    void Start()
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (assignedCard == null)
        {
            shelfImage.enabled = false;
            return;
        }

        shelfImage.sprite = shelfSprite;
        shelfImage.enabled = true;
    }
}