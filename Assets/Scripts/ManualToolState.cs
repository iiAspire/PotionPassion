using System;
using UnityEngine;

public static class ManualToolState
{
    public static bool IsBusy { get; private set; }
    public static bool IsPaused { get; private set; }

    public static event Action<bool> OnBusyChanged;
    public static event Action<bool> OnPausedChanged;

    static bool IsManualTool(ProcessingTool tool)
    {
        return tool == ProcessingTool.MortarAndPestle
            || tool == ProcessingTool.ChoppingBoard;
    }

    public static void Recompute()
    {
        var stations = UnityEngine.Object.FindObjectsOfType<WorkbenchStation>();

        bool anyManualRunning = false;
        bool anyManualBusy = false;

        foreach (var s in stations)
        {
            // Ignore passive tools entirely (Drying Rack, etc.)
            if (!IsManualTool(s.tool))
                continue;

            if (!s.IsBusy)
                continue;

            anyManualBusy = true;

            if (!s.IsPaused)
            {
                anyManualRunning = true;
                break;
            }
        }

        bool newBusy = anyManualRunning;
        bool newPaused = anyManualBusy && !anyManualRunning;

        if (IsBusy != newBusy)
        {
            IsBusy = newBusy;
            OnBusyChanged?.Invoke(IsBusy);
        }

        if (IsPaused != newPaused)
        {
            IsPaused = newPaused;
            OnPausedChanged?.Invoke(IsPaused);
        }
    }
}