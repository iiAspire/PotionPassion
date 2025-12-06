using UnityEngine;
using UnityEngine.UI;

enum LightPhase
{
    Night,
    Dawn,
    Day,
    Dusk
}

public class EnvironmentLightController : MonoBehaviour
{
    [Header("Scene References")]
    public TimeUIController timeUI;    // must match the one controlling sunrise/sunset
    public Image[] windowPanes;        // darkening overlays for window
    public Image sceneTint;            // full-screen tint overlay (Multiply or Additive)

    [Header("Window Pane Darkness")]
    [Range(0f, 1f)] public float maxDarkness = 0.85f;   // how dark windows get at night
    [Range(0f, 1f)] public float duskDarkness = 0.4f;
    [Range(0f, 1f)] public float dawnDarkness = 0.35f;

    [Header("Twilight Timing (Matches Day Bar)")]
    [Tooltip("Hours after sunset until full night")]
    public float duskDurationHours = 2f;

    [Tooltip("Hours before sunrise when dawn begins")]
    public float dawnDurationHours = 4f;

    [Header("Tint Colors")]
    public Color moonTint = new Color(0.05f, 0.09f, 0.18f, 1f);   // deep night blue
    public Color dawnTint = new Color(1.0f, 0.55f, 0.25f, 1f);    // sunrise orange
    public Color sunsetTint = new Color(1.0f, 0.52f, 0.2f, 1f);   // sunset red/orange

    public Color middayTintSummer = new Color(1.0f, 0.95f, 0.65f, 1f); // bright warm summer
    public Color middayTintWinter = new Color(0.85f, 0.9f, 1.0f, 1f);  // pale cold winter

    private bool suspendUpdates = false;
    float visualHour;
    bool visualInitialized;

    private LightPhase lastPhase = (LightPhase)(-1);
    float lastLoggedX = -1f;

    public void Suspend()
    {
        suspendUpdates = true;
    }

    public void Resume()
    {
        suspendUpdates = false;
    }

    void Update()
    {
        if (suspendUpdates || timeUI == null)
            return;

        float hour = timeUI.timeOfDay;
        float sunrise = timeUI.GetSeasonalSunrise();
        float sunset = timeUI.GetSeasonalSunset();

        LightPhase currentPhase;

        if (hour < sunrise)
            currentPhase = LightPhase.Night;
        else if (hour < sunrise + 0.01f)
            currentPhase = LightPhase.Dawn;
        else if (hour < sunset)
            currentPhase = LightPhase.Day;
        else
            currentPhase = LightPhase.Dusk;

        if (currentPhase != lastPhase)
        {
            //    Debug.Log(
            //        $"🌗 LIGHT PHASE CHANGE → {currentPhase} | " +
            //        $"hour={hour:F2}, sunrise={sunrise:F2}, sunset={sunset:F2}, " +
            //        $"season={timeUI.yearProgress:F2}"
            //    );

            lastPhase = currentPhase;
        }

        ApplyLightingInstant();
    }

    // --------------------------------------------------------------
    // 1. WINDOW DARKENING (season-aware)
    // --------------------------------------------------------------
    void UpdateWindowPanes(float hour, float sunrise, float sunset)
    {
        if (timeUI == null)
            return;

        // --- 1) Rebuild the same band positions the bar uses ---
        float sunriseT = sunrise / 24f;
        float sunsetT = sunset / 24f;

        float dawnWidthT = timeUI.dawnBlendWidth; // same values the bar uses
        float duskWidthT = timeUI.duskBlendWidth;

        // These match your TimeUIController.UpdateDayBarGradient
        float p1 = Mathf.Clamp01(sunriseT - dawnWidthT); // Night → Dawn start
        float p2 = Mathf.Clamp01(sunriseT);              // Dawn → Day
        float p3 = Mathf.Clamp01(sunsetT);               // Day → Dusk
        float p4 = Mathf.Clamp01(sunsetT + duskWidthT);  // Dusk → Night

        // Current time as 0–1 across the “day bar”
        float x = Mathf.Clamp01(hour / 24f);

        // --- 2) Decide darkness from those bands ---
        float alpha;

        if (x < p1)
        {
            // Deep night
            alpha = maxDarkness;
        }
        else if (x < p2)
        {
            // Dawn: dark → a bit lighter
            float t = Mathf.InverseLerp(p1, p2, x);
            alpha = Mathf.Lerp(maxDarkness, dawnDarkness, t);
        }
        else if (x < p3)
        {
            // Daytime
            alpha = 0f;
        }
        else if (x < p4)
        {
            // Dusk: light → dark
            float t = Mathf.InverseLerp(p3, p4, x);
            alpha = Mathf.Lerp(duskDarkness, maxDarkness, t);
        }
        else
        {
            // Deep night again
            alpha = maxDarkness;
        }

        // --- 3) Apply to all window panes ---
        foreach (var pane in windowPanes)
        {
            if (!pane) continue;

            var c = pane.color;
            c.a = alpha;
            pane.color = c;
        }
    }

    // --------------------------------------------------------------
    // 2. TINT OVERLAY (season-aware, stronger visuals)
    // --------------------------------------------------------------
    void UpdateSceneTint(float hour, float sunrise, float sunset, float summerFactor)
    {
        if (sceneTint == null || timeUI == null)
            return;

        // Season-based midday colour (unchanged)
        Color middayTint = Color.Lerp(middayTintWinter, middayTintSummer, summerFactor);

        // Same normalized positions as the day bar gradient
        float sunriseT = sunrise / 24f;
        float sunsetT = sunset / 24f;

        float dawnWidthT = timeUI.dawnBlendWidth;
        float duskWidthT = timeUI.duskBlendWidth;

        float p1 = Mathf.Clamp01(sunriseT - dawnWidthT); // Night → Dawn start
        float p2 = Mathf.Clamp01(sunriseT);              // Dawn → Day
        float p3 = Mathf.Clamp01(sunsetT);               // Day → Dusk
        float p4 = Mathf.Clamp01(sunsetT + duskWidthT);  // Dusk → Night

        // Current time mapped onto 0..1 like the bar
        float x = Mathf.Clamp01(hour / 24f);

        if (Mathf.Abs(x - lastLoggedX) > 0.01f)
        {
            //Debug.Log(
            //    $"🌈 FADE STATE phase | hour={hour:F2} (x={x:F3}) " +
            //    $"p1={p1:F3} p2={p2:F3} p3={p3:F3} p4={p4:F3}"
            //);
            lastLoggedX = x;
        }

        Color tint;

        if (x < p1)
        {
            // Deep night
            tint = moonTint;
        }
        else if (x < p2)
        {
            // Dawn band: moon → dawn
            float t = Mathf.InverseLerp(p1, p2, x);
            tint = Color.Lerp(moonTint, dawnTint, t);
        }
        else if (x < p3)
        {
            // Full day
            tint = middayTint;
        }
        else if (x < p4)
        {
            // Dusk band: day → sunset
            float t = Mathf.InverseLerp(p3, p4, x);
            tint = Color.Lerp(middayTint, sunsetTint, t);
        }
        else
        {
            // Late evening → night: sunset → moon
            float t = Mathf.InverseLerp(p4, 1f, x);
            tint = Color.Lerp(sunsetTint, moonTint, t);
        }

        sceneTint.color = tint;
    }

    public void ApplyLightingInstant()
    {
        if (!visualInitialized)
        {
            visualHour = timeUI.timeOfDay;
            visualInitialized = true;
        }

        if (timeUI == null) return;

        float hour = timeUI.timeOfDay;
        float sunrise = timeUI.GetSeasonalSunrise();
        float sunset = timeUI.GetSeasonalSunset();
        float season = timeUI.yearProgress;

        float summerFactor = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * season);

        UpdateWindowPanes(hour, sunrise, sunset);
        UpdateSceneTint(hour, sunrise, sunset, summerFactor);
    }

    public void DisableVisuals()
    {
        if (sceneTint) sceneTint.canvasRenderer.SetAlpha(0f);
        foreach (var pane in windowPanes)
            if (pane) pane.canvasRenderer.SetAlpha(0f);

        gameObject.SetActive(false);
    }

    public void EnableVisualsInstant()
    {
        gameObject.SetActive(true);
        ApplyLightingInstant();
    }
}