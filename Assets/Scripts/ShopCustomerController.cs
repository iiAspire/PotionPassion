using UnityEngine;

public class ShopCustomerController : MonoBehaviour
{
    public static ShopCustomerController Instance { get; private set; }

    [Header("Visuals")]
    [SerializeField] GameObject customerSprite;

    public bool CustomerWaiting { get; private set; }

    void Awake()
    {
        Instance = this;
        customerSprite.SetActive(false);
    }

    void Start()
    {
        if (CustomerScheduler.Instance != null &&
            CustomerScheduler.Instance.CustomerWaiting)
        {
            CustomerArrives();
        }
    }

    public void CustomerArrives()
    {
        if (!ShopShelfManager.Instance.IsShopOpen)
            return;   // ignore arrivals if shop closed

        if (CustomerWaiting)
            return;

        CustomerWaiting = true;

        customerSprite.SetActive(true);

        CardClickLog.Instance?.Log("Customer waiting.");
    }

    public void CustomerServed()
    {
        CustomerWaiting = false;

        if (CustomerScheduler.Instance != null)
            CustomerScheduler.Instance.ClearWaitingCustomer();

        customerSprite.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}