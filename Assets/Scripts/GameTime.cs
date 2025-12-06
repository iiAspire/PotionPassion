using UnityEngine;

public static class GameTime
{
    public static float currentMinutes;

    public static void AddMinutes(float minutes)
    {
        currentMinutes += minutes;
    }
}