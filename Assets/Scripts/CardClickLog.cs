using UnityEngine;
using TMPro;
using System.Collections;

public class CardClickLog : MonoBehaviour
{
    public static CardClickLog Instance;

    [Header("UI")]
    public TextMeshProUGUI text;
    public float visibleTime = 2f;
    public float fadeTime = 0.4f;

    Coroutine routine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        text.gameObject.SetActive(false);
    }

    public void Log(string cardName)
    {
        if (routine != null)
            StopCoroutine(routine);

        text.text = cardName;
        text.alpha = 1f;
        text.gameObject.SetActive(true);

        routine = StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(visibleTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        text.gameObject.SetActive(false);
    }
}