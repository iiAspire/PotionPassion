using UnityEngine;
using UnityEngine.UI;

// NOT CURRENTLY IN USE: for fading entire screens, loading etc.

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private bool enableGlobalFade = false;
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.interactable = false;
            fadeGroup.blocksRaycasts = false;

            // Explicit safety: parked unless enabled
            if (!enableGlobalFade)
                fadeGroup.gameObject.SetActive(false);
        }
    }
}