using System;

public static class ManualToolState
{
    public static bool IsBusy { get; private set; }
    public static bool IsPaused { get; private set; }

    public static event Action<bool> OnBusyChanged;
    public static event Action<bool> OnPausedChanged;

    public static void SetBusy(bool busy)
    {
        if (IsBusy == busy)
            return;

        IsBusy = busy;

        if (!busy)
            IsPaused = false;

        OnBusyChanged?.Invoke(IsBusy);
    }

    public static void SetPaused(bool paused)
    {
        if (!IsBusy)
            return;

        if (IsPaused == paused)
            return;

        IsPaused = paused;
        OnPausedChanged?.Invoke(IsPaused);
    }
}