using UnityEngine;

public class CauldronSmokePuff : MonoBehaviour
{
    RectTransform rt;
    CanvasGroup cg;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
    }

    public void Play(float rise, float duration)
    {
        // Reset
        cg.alpha = 1f;
        rt.localScale = Vector3.one * Random.Range(0.9f, 1.4f);

        // Random drift
        float drift = Random.Range(-50f, 50f);
        Vector2 startPos = rt.anchoredPosition;

        // Animate with built-in Lerp coroutines (NO tweening needed)
        StartCoroutine(AnimateSmoke(startPos, drift, rise, duration));
    }

    System.Collections.IEnumerator AnimateSmoke(Vector2 startPos, float drift, float rise, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            // Move upward & sideways
            rt.anchoredPosition = new Vector2(
                startPos.x + drift * p,
                startPos.y + rise * p
            );

            // Fade out
            cg.alpha = 1f - p;

            yield return null;
        }

        Destroy(gameObject);
    }
}