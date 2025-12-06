using UnityEngine;
using UnityEngine.UI;

public class LeaveRoomButtonLock : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField, Range(0f, 1f)] float dimAlpha = 0.4f;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        ManualToolState.OnBusyChanged += Refresh;
        ManualToolState.OnPausedChanged += Refresh;
        Refresh(ManualToolState.IsBusy);
    }

    void OnDisable()
    {
        ManualToolState.OnBusyChanged -= Refresh;
        ManualToolState.OnPausedChanged -= Refresh;
    }

    void Refresh(bool _)
    {
        bool locked = ManualToolState.IsBusy && !ManualToolState.IsPaused;

        button.interactable = !locked;

        if (canvasGroup != null)
            canvasGroup.alpha = locked ? dimAlpha : 1f;
    }

    void OnBusyChanged(bool busy)
    {
        button.interactable = !busy;

        if (canvasGroup != null)
            canvasGroup.alpha = busy ? dimAlpha : 1f;
    }
}