using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientCicadas : MonoBehaviour
{
    public AudioClip cicadaLoop;

    [Header("Fade Speed")]
    public float fadeSpeed = 1.2f;

    [Header("Distance Simulation")]
    [Range(0f, 1f)]
    public float distance = 0.5f;   // set once, not randomized daily

    AudioSource source;
    AudioLowPassFilter lowPass;
    AudioReverbFilter reverb;

    void Awake()
    {
        source = GetComponent<AudioSource>();

        source.clip = cicadaLoop;
        source.loop = true;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.volume = 0f;

        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        reverb = gameObject.AddComponent<AudioReverbFilter>();
    }

    void Start()
    {
        ApplyDistanceSettings();
        source.Play();
    }

    void Update()
    {
        var tm = TimeManager.Instance;
        var timeUI = TimeUIController.Instance;

        if (tm == null || timeUI == null) return;

        float t = tm.Calendar.timeOfDay;

        float sunrise = timeUI.GetSeasonalSunrise();
        float sunset = timeUI.GetSeasonalSunset();

        float startAfterSunset = 0.5f;   // hours after sunset
        float stopBeforeSunrise = 1.0f;  // hours before sunrise

        bool active =
            t >= sunset + startAfterSunset ||
            t <= sunrise - stopBeforeSunrise;

        float targetVolume = 0f;

        if (active)
        {
            float nightFactor = GetAfterMidnightFactor(t);
            float seasonal = GetSeasonalFactor();
            targetVolume = GetDistanceVolume() * nightFactor * seasonal;
        }

        source.volume = Mathf.MoveTowards(
            source.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );
    }

    // =====================================================
    // LOUDNESS DROP AFTER MIDNIGHT
    // =====================================================

    float GetAfterMidnightFactor(float t)
    {
        var timeUI = TimeUIController.Instance;
        if (timeUI == null) return 1f;

        float sunrise = timeUI.GetSeasonalSunrise();

        // Before midnight → full
        if (t >= 18f && t < 24f)
            return 1f;

        // After midnight → decline toward sunrise
        if (t < sunrise)
        {
            float nightSpan = sunrise;   // midnight (0) → sunrise
            float progress = t / nightSpan;
            return Mathf.Lerp(1f, 0.6f, progress);
        }

        return 1f;
    }

    // =====================================================
    // DISTANCE ILLUSION
    // =====================================================

    void ApplyDistanceSettings()
    {
        source.panStereo = Random.Range(-1f, 1f);
        source.pitch = Random.Range(0.98f, 1.02f);

        lowPass.cutoffFrequency = Mathf.Lerp(22000f, 2500f, distance);
        reverb.reverbLevel = Mathf.Lerp(-1000f, 0f, distance);
    }

    float GetDistanceVolume()
    {
        return Mathf.Lerp(1f, 0.25f, distance);
    }

    float GetSeasonalFactor()
    {
        var tm = TimeManager.Instance;
        if (tm == null) return 0f;

        float y = tm.Calendar.yearProgress;

        // Smooth peak at summer (0.5), zero at winter (0 or 1)
        float factor = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * y);

        // Optional threshold: no cicadas when too cold
        if (factor < 0.35f) return 0f;  // was 0.2f

        return factor;
    }
}