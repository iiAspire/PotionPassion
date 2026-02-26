using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientWolf : MonoBehaviour
{
    public AudioClip[] wolfClips;

    AudioSource source;

    bool wasNight = false;
    bool hasHowledTonight = false;

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

        bool fullMoon = IsFullMoon(tm);

        // 🌇 Start of night
        if (isNight && !wasNight)
        {
            hasHowledTonight = false;

            if (fullMoon)
                scheduledTime = PickNightTime(sunset, sunrise);
        }

        wasNight = isNight;

        if (!fullMoon || hasHowledTonight)
            return;

        if (IsTimeReached(t, scheduledTime))
        {
            PlayWolf();
            hasHowledTonight = true;
        }
    }

    float PickNightTime(float sunset, float sunrise)
    {
        float start = sunset + 1.0f;
        float end = sunrise - 1.5f;

        if (start < 24f)
            return Random.Range(start, 24f);

        return Random.Range(0f, end);
    }

    bool IsTimeReached(float now, float target)
    {
        if (target < 0f) return false;

        return now >= target && now < target + 0.1f;
    }

    bool IsFullMoon(TimeManager tm)
    {
        int day = tm.Calendar.lunarDay;
        int cycle = tm.Calendar.lunarCycleLength;

        int full = cycle / 2;

        return Mathf.Abs(day - full) <= 1;
    }

    void PlayWolf()
    {
        if (wolfClips.Length == 0) return;

        var clip = wolfClips[Random.Range(0, wolfClips.Length)];

        source.panStereo = Random.Range(-1f, 1f);
        source.pitch = Random.Range(0.95f, 1.05f);

        source.PlayOneShot(clip);
    }
}