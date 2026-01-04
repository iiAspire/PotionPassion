using UnityEngine;

[System.Serializable]
public class CalendarState
{
    // Clock
    public float timeOfDay = 12f;   // 0–24

    // Calendar
    public int dayOfMonth = 1;
    public int lunarDay = 1;
    public int cosmicDay = 1;

    // Year
    public float yearProgress = 0.25f; // 0–1

    // Configuration (can move later if desired)
    public int daysInMonth = 28;
    public int monthsPerSeason = 3;
    public int seasonsInYear = 4;

    public int lunarCycleLength = 32;
    public int daysPerCosmicBody = 2;

    public int MonthsInYear => monthsPerSeason * seasonsInYear;
    public int DaysPerYear => daysInMonth * MonthsInYear;

    public float winterSunrise = 7f;
    public float winterSunset = 16f;
    public float summerSunrise = 4f;
    public float summerSunset = 22f;

    float GetSummerFactor()
    {
        // 0 = winter solstice, 0.5 = summer solstice
        return 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * yearProgress);
    }

    public float GetSeasonalSunrise()
    {
        float summerFactor = GetSummerFactor();
        return Mathf.Lerp(winterSunrise, summerSunrise, summerFactor);
    }

    public float GetSeasonalSunset()
    {
        float summerFactor = GetSummerFactor();
        return Mathf.Lerp(winterSunset, summerSunset, summerFactor);
    }

    public bool IsDaytime()
    {
        float sunrise = GetSeasonalSunrise();
        float sunset = GetSeasonalSunset();
        return timeOfDay >= sunrise && timeOfDay < sunset;
    }

    public float GetNightFactor(float fadeHours = 1f)
    {
        float sunrise = GetSeasonalSunrise();
        float sunset = GetSeasonalSunset();

        // Night → Day (dawn)
        if (timeOfDay >= sunrise - fadeHours && timeOfDay < sunrise + fadeHours)
        {
            return 1f - Mathf.InverseLerp(
                sunrise - fadeHours,
                sunrise + fadeHours,
                timeOfDay
            );
        }

        // Day → Night (dusk)
        if (timeOfDay >= sunset - fadeHours && timeOfDay < sunset + fadeHours)
        {
            return Mathf.InverseLerp(
                sunset - fadeHours,
                sunset + fadeHours,
                timeOfDay
            );
        }

        // Full night
        if (timeOfDay < sunrise - fadeHours || timeOfDay >= sunset + fadeHours)
            return 1f;

        // Full day
        return 0f;
    }
}