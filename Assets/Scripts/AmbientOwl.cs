using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientOwl : MonoBehaviour
{
    public AudioClip[] owlClips;

    [Range(0f, 1f)]
    public float chancePerNight = 0.3f;

    public float minAfterSunset = 1.0f;
    public float maxBeforeSunrise = 1.5f;

    AudioSource source;

    bool wasNight = false;
    bool willCallTonight = false;
    bool hasCalledTonight = false;

    float scheduledTime = -1f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.spatialBlend = 0f;
    }

    void Update()
    {
        var tm = TimeManager.Instance;
        var ui = TimeUIController.Instance;
        if (tm == null || ui == null) return;

        float t = tm.Calendar.timeOfDay;

        float sunrise = ui.GetSeasonalSunrise();
        float sunset = ui.GetSeasonalSunset();

        bool isNight = t >= sunset || t <= sunrise;

        // 🌇 Start of night
        if (isNight && !wasNight)
        {
            willCallTonight = Random.value < chancePerNight;
            hasCalledTonight = false;

            if (willCallTonight)
                scheduledTime = PickNightTime(sunset, sunrise);
        }

        wasNight = isNight;

        if (!willCallTonight || hasCalledTonight)
            return;

        if (IsTimeReached(t, scheduledTime))
        {
            PlayOwl();
            hasCalledTonight = true;
        }
    }

    float PickNightTime(float sunset, float sunrise)
    {
        float start = sunset + minAfterSunset;
        float end = sunrise - maxBeforeSunrise;

        if (start < 24f)
            return Random.Range(start, 24f);

        // wrap-around case
        return Random.Range(0f, end);
    }

    bool IsTimeReached(float now, float target)
    {
        if (target < 0f) return false;

        if (target >= 0f && target < 24f)
            return now >= target && now < target + 0.1f;

        return false;
    }

    void PlayOwl()
    {
        if (owlClips.Length == 0) return;

        var clip = owlClips[Random.Range(0, owlClips.Length)];

        source.panStereo = Random.Range(-0.8f, 0.8f);
        source.pitch = Random.Range(0.95f, 1.05f);

        source.PlayOneShot(clip);
    }
}