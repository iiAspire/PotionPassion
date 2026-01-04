using UnityEngine;

public class WindowNightFadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] panes;
    [SerializeField] private float fadeSpeed = 3f;

    CalendarState Cal => TimeManager.Instance?.Calendar;

    void Update()
    {
        if (Cal == null || panes == null || panes.Length == 0)
            return;

        float nightAlpha = Cal.GetNightFactor(1.0f); // 1 in-game hour fade

        foreach (var pane in panes)
        {
            if (pane == null) continue;
            pane.alpha = nightAlpha;
        }
    }

    //void Update()
    //{

    //    Debug.Log($"[WindowFade] panes.Length = {panes?.Length ?? -1}");

    //    if (Cal == null)
    //    {
    //        Debug.Log("❌ Cal is null");
    //        return;
    //    }

    //    Debug.Log($"🕒 time={Cal.timeOfDay:F2}, isDay={Cal.IsDaytime()}");

    //    float targetAlpha = Cal.IsDaytime() ? 0f : 1f;

    //    foreach (var pane in panes)
    //    {
    //        pane.alpha = Mathf.MoveTowards(
    //            pane.alpha,
    //            targetAlpha,
    //            Time.deltaTime * fadeSpeed
    //        );
    //    }

    //    for (int i = 0; i < panes.Length; i++)
    //    {
    //        Debug.Log(
    //            $"[WindowFade] pane[{i}] name={panes[i].name} alpha={panes[i].alpha}",
    //            panes[i]
    //        );
    //    }
    //}
}