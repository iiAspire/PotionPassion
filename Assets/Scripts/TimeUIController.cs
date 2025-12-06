using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeUIController : MonoBehaviour
{
    public static TimeUIController Instance { get; private set; }

    [Header("Day / Night Bar")]
    public RectTransform dayBar;
    public RectTransform dayArrow;
    [Tooltip("0–24, where 0 = midnight, 12 = noon")]
    [Range(0f, 24f)] public float timeOfDay = 12f;

    [Header("DayBar Gradient (UI/Horizontal4ColorGradient)")]
    public Image dayBarImage;  // same Image that uses the Horizontal4ColorGradient material
    [Tooltip("Width of the dawn blend band (0–1, as fraction of bar)")]
    [Range(0f, 0.5f)] public float dawnBlendWidth = 0.05f;
    [Tooltip("Width of the dusk blend band (0–1, as fraction of bar)")]
    [Range(0f, 0.5f)] public float duskBlendWidth = 0.05f;

    [Header("Season Bar")]
    public RectTransform seasonBar;
    public RectTransform seasonArrow;
    [Tooltip("0 = start of winter, 0.25 = start of spring, 0.5 = start of summer, 0.75 = start of autumn")]
    [Range(0f, 1f)] public float yearProgress = 0.25f; // start of spring by default

    [Header("Calendar")]
    [Tooltip("Day number within the current month (1..daysInMonth)")]
    public int dayOfMonth = 1;

    [Header("Calendar Structure")]
    [Tooltip("Days in each month")]
    public int daysInMonth = 28;
    [Tooltip("Months in each season")]
    public int monthsPerSeason = 3;   // Winter, Spring, Summer, Autumn
    [Tooltip("Number of seasons in a year")]
    public int seasonsInYear = 4;     // 4 seasons = 12 months total

    int MonthsInYear => monthsPerSeason * seasonsInYear;       // = 12
    int DaysPerYear => daysInMonth * MonthsInYear;             // = 336

    [Header("Day / Night Icon")]
    public Image dayIcon;       // icon riding on the dayArrow
    public Sprite sunSprite;
    public Sprite[] moonPhaseSprites; // 0..N-1 phases
    public Color dayIconColor = Color.white;

    [Header("Lunar Cycle")]
    [Tooltip("1..lunarCycleLength")]
    public int lunarDay = 1;
    public int lunarCycleLength = 32;   // or 28 etc.

    [Header("Cosmic Cycle")]
    public string[] cosmicBodies = new string[12]
    {
        "Sun", "Moon", "Mercury", "Venus", "Mars",
        "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto", "Constellation", "Comet"
    };

    [Tooltip("Advances once per in-game day")]
    public int cosmicDay = 1;
    [Tooltip("How many days each cosmic body lasts")]
    public int daysPerCosmicBody = 2;

    public Image cosmicIcon;
    public Sprite[] cosmicSprites;
    // public TMP_Text cosmicLabel;  // if you want text labels too

    [Header("Seasonal Daylight Settings")]
    [Tooltip("Sunrise in deepest winter (24h clock)")]
    public float winterSunrise = 7f;
    [Tooltip("Sunset in deepest winter (24h clock)")]
    public float winterSunset = 16f;
    [Tooltip("Sunrise in peak summer (24h clock)")]
    public float summerSunrise = 4f;
    [Tooltip("Sunset in peak summer (24h clock)")]
    public float summerSunset = 22f;

    void Awake()
    {
        //Debug.Log("✅ TimeUIController bound: " + gameObject.scene.name);
        Instance = this;
    }

    void OnEnable()
    {
        // Ensure UI matches current values
        UpdateSeasonArrow();
        UpdateDayArrow();
        UpdateCosmicUI();
        UpdateDayIcon();
        UpdateDayBarGradient();
    }

    // =========================================================
    // PUBLIC API – called by TimeManager / gameplay
    // =========================================================

    /// <summary>
    /// Set absolute time of day (0–24) and update UI.
    /// </summary>
    public void SetTime(float newTime)
    {
        timeOfDay = Mathf.Clamp(newTime, 0f, 24f);
        UpdateDayArrow();
    }

    /// <summary>
    /// Set absolute year progress (0–1) and move season arrow.
    /// </summary>
    public void SetYearProgress(float progress)
    {
        yearProgress = Mathf.Repeat(progress, 1f);
        UpdateSeasonArrow();
        UpdateDayIcon();
        UpdateDayBarGradient();
    }

    /// <summary>
    /// Add a fraction of the full year and update season bar.
    /// </summary>
    public void AddYearFraction(float fractionOfYear)
    {
        yearProgress = Mathf.Repeat(yearProgress + fractionOfYear, 1f);
        UpdateSeasonArrow();
        UpdateDayIcon();
        UpdateDayBarGradient();
    }

    /// <summary>
    /// Should be called once every time the in-game day rolls over
    /// (e.g. when timeOfDay goes from X to a smaller value).
    /// Handles: calendar day, yearProgress, lunarDay, cosmicDay, icons.
    /// </summary>
    public void AdvanceDay()
    {
        // Calendar
        dayOfMonth++;
        if (dayOfMonth > daysInMonth)
            dayOfMonth = 1;

        // Year progression (28 days * 12 months = 336 days/year)
        float yearStep = 1f / Mathf.Max(1, DaysPerYear);
        yearProgress = Mathf.Repeat(yearProgress + yearStep, 1f);
        UpdateSeasonArrow();

        // Lunar cycle
        lunarDay++;
        if (lunarDay > lunarCycleLength)
            lunarDay = 1;

        // Cosmic cycle
        int totalCosmicDays = Mathf.Max(1, cosmicBodies.Length * daysPerCosmicBody);
        cosmicDay++;
        if (cosmicDay > totalCosmicDays)
            cosmicDay = 1;

        // Update visuals that depend on day count / season
        UpdateDayIcon();        // uses moon phase + seasonal sunrise/sunset
        UpdateCosmicUI();       // uses cosmicDay
        UpdateDayBarGradient(); // in case sunrise/sunset moved
    }

    // =========================================================
    // INTERNAL: DAY ARROW, ICON, GRADIENT
    // =========================================================

    void UpdateDayArrow()
    {
        if (dayBar == null || dayArrow == null)
            return;

        // Arrow always moves linearly from left (0h) to right (24h)
        float t = Mathf.Clamp01(timeOfDay / 24f);
        float posX = dayBar.rect.width * t;

        dayArrow.anchoredPosition = new Vector2(posX, dayArrow.anchoredPosition.y);

        UpdateDayIcon();
        UpdateDayBarGradient();
    }

    void UpdateDayIcon()
    {
        if (dayIcon == null)
            return;

        bool isDaytime = IsDaytime();

        if (isDaytime)
        {
            dayIcon.sprite = sunSprite;
            dayIcon.color = dayIconColor;
        }
        else
        {
            dayIcon.sprite = GetMoonPhaseSprite();
            dayIcon.color = dayIconColor;
        }
    }

    /// <summary>
    /// Updates the 4 gradient stop positions based on seasonal sunrise/sunset,
    /// for the shader "UI/Horizontal4ColorGradient" with _Stop1..4.
    /// </summary>
    void UpdateDayBarGradient()
    {
        if (dayBarImage == null)
            return;

Material mat = dayBarImage.materialForRendering;
        if (mat == null)
            return;

        // Compute normalized sunrise/sunset positions (0–1 across the bar)
        float sunriseT = GetSeasonalSunrise() / 24f;
        float sunsetT = GetSeasonalSunset() / 24f;

        // Blend ranges (dawn/dusk widths)
        float p1 = Mathf.Clamp01(sunriseT - dawnBlendWidth); // Night → Dawn
        float p2 = Mathf.Clamp01(sunriseT);                  // Dawn → Day
        float p3 = Mathf.Clamp01(sunsetT);                   // Day → Dusk
        float p4 = Mathf.Clamp01(sunsetT + duskBlendWidth);  // Dusk → Night

        mat.SetFloat("_Stop1", p1);
        mat.SetFloat("_Stop2", p2);
        mat.SetFloat("_Stop3", p3);
        mat.SetFloat("_Stop4", p4);
    }

    // =========================================================
    // INTERNAL: SEASONS & SUNRISE / SUNSET
    // =========================================================

    void UpdateSeasonArrow()
    {
        if (seasonBar == null || seasonArrow == null)
            return;

        float t = Mathf.Clamp01(yearProgress);
        float posX = seasonBar.rect.width * t;
        seasonArrow.anchoredPosition = new Vector2(posX, seasonArrow.anchoredPosition.y);
    }

    /// <summary>
    /// Returns 0 in deepest winter, 0.5 at equinoxes, 1 at peak summer.
    /// We want: shortest days at yearProgress = 0 (start of winter),
    /// longest days at yearProgress = 0.5 (start of summer).
    /// </summary>
    float GetSummerFactor()
    {
        // yearProgress: 0   = winter solstice
        //               0.25= spring equinox
        //               0.5 = summer solstice
        //               0.75= autumn equinox
        //               1.0 = winter again
        // Cosine-based daylight curve
        return 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * yearProgress);
    }

    public float GetSeasonalSunrise()
    {
        float summerFactor = GetSummerFactor(); // 0=deep winter, 1=peak summer
        return Mathf.Lerp(winterSunrise, summerSunrise, summerFactor);
    }

    public float GetSeasonalSunset()
    {
        float summerFactor = GetSummerFactor();
        return Mathf.Lerp(winterSunset, summerSunset, summerFactor);
    }

    bool IsDaytime()
    {
        float sunrise = GetSeasonalSunrise();
        float sunset = GetSeasonalSunset();
        return (timeOfDay >= sunrise && timeOfDay < sunset);
    }

    // =========================================================
    // INTERNAL: MOON PHASES
    // =========================================================

    Sprite GetMoonPhaseSprite()
    {
        if (moonPhaseSprites == null || moonPhaseSprites.Length == 0)
            return null;

        // 1..lunarCycleLength -> 0..1
        float cyclePos = (float)(lunarDay - 1) / Mathf.Max(1, lunarCycleLength);

        // Map that 0..1 range to our set of sprites
        int index = Mathf.FloorToInt(cyclePos * moonPhaseSprites.Length);
        index = Mathf.Clamp(index, 0, moonPhaseSprites.Length - 1);

        return moonPhaseSprites[index];
    }

    // =========================================================
    // INTERNAL: COSMIC CYCLE
    // =========================================================

    void UpdateCosmicUI()
    {
        if (cosmicIcon == null || cosmicSprites == null || cosmicSprites.Length == 0)
            return;

        int index = GetCurrentCosmicIndex();
        if (index < 0 || index >= cosmicSprites.Length)
            return;

        cosmicIcon.sprite = cosmicSprites[index];

        // if (cosmicLabel != null && cosmicBodies != null && index < cosmicBodies.Length)
        //     cosmicLabel.text = cosmicBodies[index];
    }

    int GetCurrentCosmicIndex()
    {
        if (cosmicBodies == null || cosmicBodies.Length == 0 || daysPerCosmicBody <= 0)
            return 0;

        int index = (cosmicDay - 1) / daysPerCosmicBody;
        return Mathf.Clamp(index, 0, cosmicBodies.Length - 1);
    }
}
