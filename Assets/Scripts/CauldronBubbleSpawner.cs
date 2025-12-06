using UnityEngine;
using UnityEngine.UI;

public class CauldronBubbleUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject bubblePrefab;

    public float spawnRate = 0.5f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnBubble();
        }
    }

    void SpawnBubble()
    {
        GameObject obj = Instantiate(bubblePrefab, container);
        RectTransform rt = obj.GetComponent<RectTransform>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();

        // Start near the bottom of the cauldron
        float x = Random.Range(-container.rect.width * 0.4f, container.rect.width * 0.4f);
        float startY = -container.rect.height * 0.35f;
        rt.anchoredPosition = new Vector2(x, startY);

        // Random size variation
        float size = Random.Range(0.8f, 1.3f);
        rt.localScale = new Vector3(size, size, 1f);

        // Rise + fade values
        float rise = Random.Range(container.rect.height * 0.3f, container.rect.height * 0.55f);
        float duration = Random.Range(0.7f, 1.2f);

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

            // Rise upward
            rt.anchoredPosition = startPos + new Vector2(0, rise * p);

            // Fade out
            cg.alpha = 1f - p;

            yield return null;
        }

        Destroy(rt.gameObject);
    }
}