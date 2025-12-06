using UnityEngine;

public class ExitButtonsController : MonoBehaviour
{
    public static ExitButtonsController Instance;

    public CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
    }

    public static void SetEnabled(bool enabled)
    {
        if (Instance == null || Instance.canvasGroup == null)
            return;

        Instance.canvasGroup.alpha = enabled ? 1f : 0.35f;
        Instance.canvasGroup.interactable = enabled;
        Instance.canvasGroup.blocksRaycasts = enabled;
    }
}