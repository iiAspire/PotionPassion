using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    private static TimeManager instance;
    public static TimeManager Instance => instance;

    [Header("Reference to UI")]
    public static double TotalGameMinutes;
    TimeUIController UI => TimeUIController.Instance;

    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float timeOfDay = 12f; // 0 = midnight, 12 = noon
    [Range(0f, 1f)]
    public float yearProgress = 0f; // 0 = start spring, 0.25 = start summer

    private float lastTimeOfDay;

    [Header("Game Time Scale")]
    [Tooltip("How many in-game minutes pass per real second")]
    public float minutesPerRealSecond = 0.25f;
    // 0.25 = 15 in-game minutes per real minute

    //For speeding up during testing
    public static float MinutesPerRealSecond =>
        Instance != null ? Instance.minutesPerRealSecond : 0.25f;

    float logAccumulator = 0f;

    void Awake()
    { 
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (UI != null)
        {
            UI.SetTime(timeOfDay);
            UI.SetYearProgress(yearProgress);
        }
    }

    void Start()
    {
        yearProgress = 0.25f; // start of spring (March-ish)

        if (UI != null)
        {
            UI.SetYearProgress(yearProgress);
            UI.SetTime(timeOfDay);
        }
    }

    // Call this from WorkbenchStation coroutines
    public void AddGameHours(float hours)
    {
        timeOfDay += hours;

        bool rolledOver = timeOfDay >= 24f;
        if (rolledOver)
            timeOfDay -= 24f;

        UI?.SetTime(timeOfDay);

        if (rolledOver)
            UI?.AdvanceDay();
    }

    public void SetTime(float newTime)
    {
        timeOfDay = Mathf.Clamp(newTime, 0f, 24f);
        UI?.SetTime(timeOfDay);
    }

    // Optional: add seasons
    public void AddYearProgress(float fraction)
    {
        yearProgress += fraction;
        if (yearProgress > 1f) yearProgress -= 1f;

        UI?.SetTime(timeOfDay);
    }

    void Update()
    {
        logAccumulator += Time.deltaTime;

        float minutesThisFrame = Time.deltaTime * minutesPerRealSecond;
        AddGameHours(minutesThisFrame / 60f);

        if (logAccumulator >= 1f)
        {
            logAccumulator = 0f;
            //Debug.Log($"⏱ TimeOfDay = {timeOfDay:F2} @ speed={minutesPerRealSecond}x");
        }

        foreach (var rack in FindObjectsOfType<DryingRackTimer>())
            rack.TickByMinutes(minutesThisFrame);

        foreach (var planter in FindObjectsOfType<PlanterSlot>())
            planter.TickByMinutes(minutesThisFrame);
    }
}