using UnityEngine;
using System.Collections;

public class TestFadeOnly : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 5.5f;

    public void TestFadeOnlyButton()
    {
        StartCoroutine(TestFadeRoutine());
    }

    IEnumerator TestFadeRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.SmoothStep(1f, 0f, t / fadeDuration);
            yield return null;
        }
    }
}