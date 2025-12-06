using UnityEngine;

public class CauldronSmokeUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject smokePrefab;

    public void PlaySmokeBurst()
    {
        // 5–8 puffs look good
        for (int i = 0; i < 6; i++)
            SpawnPuff();
    }

    void SpawnPuff()
    {
        GameObject obj = Instantiate(smokePrefab, container);
        RectTransform rt = obj.GetComponent<RectTransform>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();

        // Start exactly where the debug puff appeared:
        // The center of the container
        rt.anchoredPosition = Vector2.zero;

        // Give it a random horizontal offset (small)
        rt.anchoredPosition += new Vector2(Random.Range(-30f, 30f), 0f);

        // Start fully visible
        cg.alpha = 0.8f;

        // Animate upward
        float rise = Random.Range(100f, 160f);
        float duration = Random.Range(0.7f, 1.3f);

        StartCoroutine(RiseAndFade(rt, cg, rise, duration));
    }

    System.Collections.IEnumerator RiseAndFade(RectTransform rt, CanvasGroup cg, float rise, float duration)
    {
        float t = 0f;
        Vector2 startPos = rt.anchoredPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            rt.anchoredPosition = startPos + new Vector2(0, rise * p);
            cg.alpha = 1f - p;

            yield return null;
        }

        Destroy(rt.gameObject);
    }
}