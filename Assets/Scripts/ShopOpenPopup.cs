using UnityEngine;
using UnityEngine.UI;

public class ShopOpenPopup : MonoBehaviour
{
    public static ShopOpenPopup Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Text messageText;

    private ShopBarnDoor door;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        popupPanel.SetActive(false);
    }

    public void Show(ShopBarnDoor sourceDoor, float openHour, float closeHour)
    {
        door = sourceDoor;

        messageText.text =
            $"Open shop?\n\nHours: {openHour:00}:00 - {closeHour:00}:00";

        popupPanel.SetActive(true);
    }

    public void ConfirmOpen()
    {
        door.OpenConfirmed();
        popupPanel.SetActive(false);
    }

    public void Cancel()
    {
        popupPanel.SetActive(false);
    }

    public void ShowShopClosed()
    {
        messageText.text = "The shop is now closed.";
        door = null;
        popupPanel.SetActive(true);
    }
}