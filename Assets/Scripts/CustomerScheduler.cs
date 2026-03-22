using UnityEngine;
using System.Collections.Generic;

public class CustomerScheduler : MonoBehaviour
{
    public static CustomerScheduler Instance { get; private set; }
    public bool CustomerWaiting { get; private set; }
    public bool ShopIsOpen { get; private set; }

    class Visit
    {
        public float minuteOfDay;
        public bool triggered;
    }

    List<Visit> visits = new();

    bool shopActive;

    [Header("Base Settings")]
    [SerializeField] float baseChance = 0.25f;
    [SerializeField] int maxCustomers = 4;

    [SerializeField] bool debugForceCustomerToday = false;

    void Awake()
    {
        Instance = this;
    }

    public void SetShopOpen(bool isOpen)
    {
        ShopIsOpen = isOpen;
    }

    public void StartShopSession()
    {
        visits.Clear();
        shopActive = true;

        GenerateVisits();
    }

    public void EndShopSession()
    {
        shopActive = false;
        visits.Clear();
    }

    void Update()
    { 
        if (!shopActive)
            return;

        if (visits.Count == 0)
            return;

        CheckArrivals();
    }

    void GenerateVisits()
    {
        float notoriety =
            ShopReputation.Instance != null ?
            ShopReputation.Instance.notoriety : 0f;

        //float weatherMod =
        //    WeatherManager.Instance != null ?
        //    WeatherManager.Instance.GetTravelModifier() : 0f;

        //float storyMod =
        //    StoryManager.Instance != null ?
        //    StoryManager.Instance.GetCustomerModifier() : 0f;

        float chance =
            baseChance +
            notoriety; //+
                       //weatherMod +
                       //storyMod;

        float roll = Random.value;
        Debug.Log($"[CustomerScheduler] Daily roll={roll} vs chance={chance}");

        if (!debugForceCustomerToday && roll > chance)
        {
            Debug.Log("[CustomerScheduler] Quiet day — no visitors.");
            return;
        }

        int visitorCount = Random.Range(1, 3);

        Debug.Log($"[CustomerScheduler] Visitors today: {visitorCount}");

        float currentHour = TimeManager.Instance.Calendar.timeOfDay;

        for (int i = 0; i < visitorCount; i++)
        {
            float arrivalHour = Random.Range(currentHour + 0.1f, 17f);

            visits.Add(new Visit
            {
                minuteOfDay = arrivalHour * 60f,
                triggered = false
            });

            int hour = Mathf.FloorToInt(arrivalHour);
            int minute = Mathf.FloorToInt((arrivalHour - hour) * 60f);
            Debug.Log($"[CustomerScheduler] Visit scheduled at {hour:00}:{minute:00}");
        }
    }

    void CheckArrivals()
    {
        float currentMinute =
            TimeManager.Instance.Calendar.timeOfDay * 60f;

        foreach (var visit in visits)
        {

            if (visit.triggered)
                continue;

            if (currentMinute >= visit.minuteOfDay)
            {
                visit.triggered = true;
                CustomerArrived();
            }
        }
    }

    void CustomerArrived()
    {
        CustomerWaiting = true;

        // ring bell globally
        ShopBell.Instance?.Ring();

        if (ShopCustomerController.Instance != null)
            ShopCustomerController.Instance.CustomerArrives();

        CardClickLog.Instance?.Log("Customer waiting.");
    }

    public void ClearWaitingCustomer()
    {
        CustomerWaiting = false;
    }
}