using UnityEngine;

public class ShopBarnDoor : MonoBehaviour
{
    [SerializeField] private ShopShelfManager shelfManager;

    [SerializeField] private GameObject closedImage;
    [SerializeField] private GameObject openImage;

    [SerializeField] private ShopOpenPopup popup;

    [SerializeField] private float openHour = 6f;
    [SerializeField] private float closeHour = 18f;

    void Start()
    {
        SyncVisual();
    }

    private void Update()
    {
        SyncVisual();
    }

    public void OnDoorClicked()
    {
        if (shelfManager.IsShopOpen)
        {
            shelfManager.CloseShop();
            SyncVisual();
            return;
        }

        float time = TimeManager.Instance.Calendar.timeOfDay;

        if (CardClickLog.Instance != null)
        {
            if (time < openHour || time >= closeHour)
            {
                CardClickLog.Instance.Log(
                    $"Shop hours are {openHour:00}:00 - {closeHour:00}:00.");
                return;
            }
        }

        ShopOpenPopup.Instance.Show(this, openHour, closeHour);
    }

    public void OpenConfirmed()
    {
        shelfManager.OpenShop();
        CustomerScheduler.Instance.StartShopSession();
        SyncVisual();
    }

    private void SyncVisual()
    {
        bool open = shelfManager.IsShopOpen;

        openImage.SetActive(open);
        closedImage.SetActive(!open);
    }
}