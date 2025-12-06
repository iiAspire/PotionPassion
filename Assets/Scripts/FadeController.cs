using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            fadeGroup.alpha = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeToScene(string sceneName)
    {
        Debug.Log("✅ FadeToScene CALLED with duration = " + fadeDuration);
        StartCoroutine(FadeAndRequest(sceneName));
    }

    private IEnumerator FadeAndRequest(string sceneName)
    {
        float t = 0f;

        // -------- FADE OUT --------
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / fadeDuration);
            fadeGroup.alpha = Mathf.SmoothStep(0f, 1f, n);

            Debug.Log($"[FadeController] OUT t={t:F2}, n={n:F2}, alpha={fadeGroup.alpha:F2}");

            yield return null;
        }

        fadeGroup.alpha = 1f;

        // BEFORE LOAD
        var oldEnv = FindObjectOfType<EnvironmentLightController>();
        if (oldEnv)
            oldEnv.gameObject.SetActive(false);

        // LOAD SCENE
        yield return StartCoroutine(SceneLoadManager.Instance.LoadRoomRoutine(sceneName));

        // wait for layout + canvas scaler
        yield return null;
        yield return null;

        // AFTER LOAD
        var newEnv = FindObjectOfType<EnvironmentLightController>();
        if (newEnv)
        {
            newEnv.gameObject.SetActive(false);   // keep hidden
            newEnv.ApplyLightingInstant();
        }

        // FADE IN
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / fadeDuration;
            fadeGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, n);
            yield return null;
        }

        // NOW safely reveal environment lighting
        if (newEnv)
            newEnv.gameObject.SetActive(true);

        fadeGroup.alpha = 0f;
    }
}