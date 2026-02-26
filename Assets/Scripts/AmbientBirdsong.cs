using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AmbientBirdsong : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip[] birdClips;

    [Header("Check Timing")]
    public float checkInterval = 2.5f;

    [Header("Burst Behavior")]
    public int minBurst = 1;
    public int maxBurst = 5;
    public Vector2 gapBetweenChirps = new Vector2(0.08f, 0.35f);

    [Header("Distance Simulation")]
    [Range(0f, 1f)] public float distance;

    [Header("Variation")]
    public Vector2 pitchRange = new Vector2(0.97f, 1.03f);

    AudioSource source;
    AudioLowPassFilter lowPass;
    AudioReverbFilter reverb;

    int lastDay = -1;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.spatialBlend = 0f;   // 2D
        source.playOnAwake = false;

        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        reverb = gameObject.AddComponent<AudioReverbFilter>();
    }

    void Start()
    {
        RandomizePosition();
        StartCoroutine(MainLoop());
    }

    void Update()
    {
        var tm = TimeManager.Instance;
        if (tm == null) return;

        int day = tm.Calendar.dayOfMonth;

        // Move once per new day
        if (day != lastDay)
        {
            lastDay = day;
            RandomizePosition();
        }
    }

    IEnumerator MainLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (ShouldMakeSound())
                yield return StartCoroutine(PlayBurst());
        }
    }

    // =====================================================
    // SEASONAL DAYLIGHT LOGIC
    // =====================================================

    bool ShouldMakeSound()
    {
        var tm = TimeManager.Instance;
        var timeUI = TimeUIController.Instance;

        if (tm == null || timeUI == null) return false;

        float t = tm.Calendar.timeOfDay;

        float sunrise = timeUI.GetSeasonalSunrise();
        float sunset = timeUI.GetSeasonalSunset();

        // 🌅 Dawn chorus (before → after sunrise)
        if (t >= sunrise - 0.5f && t <= sunrise + 1.5f)
            return Random.value < 0.35f;

        // 🌞 Daytime birds
        if (t > sunrise && t < sunset)
            return Random.value < 0.12f;

        // 🌇 Evening taper
        if (t >= sunset && t <= sunset + 1f)
            return Random.value < 0.05f;

        // 🌙 Night — silent
        return false;
    }

    // =====================================================
    // BURST PLAYBACK
    // =====================================================

    IEnumerator PlayBurst()
    {
        if (birdClips == null || birdClips.Length == 0)
            yield break;

        int burstCount = Random.Range(minBurst, maxBurst + 1);

        for (int i = 0; i < burstCount; i++)
        {
            var clip = birdClips[Random.Range(0, birdClips.Length)];

            source.pitch = Random.Range(pitchRange.x, pitchRange.y);

            ApplyDistanceSettings();

            source.PlayOneShot(clip);

            float gap = Random.Range(gapBetweenChirps.x, gapBetweenChirps.y);
            yield return new WaitForSeconds(clip.length + gap);
        }
    }

    // =====================================================
    // FAKE DISTANCE / POSITION
    // =====================================================

    void RandomizePosition()
    {
        distance = Random.value;
        ApplyDistanceSettings();
    }

    void ApplyDistanceSettings()
    {
        source.volume = Mathf.Lerp(1f, 0.2f, distance);
        source.panStereo = Random.Range(-1f, 1f);

        lowPass.cutoffFrequency = Mathf.Lerp(22000f, 2500f, distance);
        reverb.reverbLevel = Mathf.Lerp(-1000f, 0f, distance);
    }
}