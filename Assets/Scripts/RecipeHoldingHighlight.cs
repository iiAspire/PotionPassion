using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecipeHoldingHighlight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform recipeHoldingParent; // where cards are
    [SerializeField] private Outline outline;

    [Header("Pulse Settings")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.9f;
    public float pulseSpeed = 2f;

    private Coroutine pulseRoutine;

    void Awake()
    {
        if (outline != null)
            outline.enabled = false;
    }

    void Update()
    {
        int cardCount = CountCards();

        if (cardCount >= 2)
        {
            if (pulseRoutine == null)
                pulseRoutine = StartCoroutine(PulseOutline());
        }
        else
        {
            StopPulse();
        }
    }

    int CountCards()
    {
        int count = 0;
        foreach (Transform child in recipeHoldingParent)
        {
            if (child.GetComponent<CardComponent>() != null)
                count++;
        }
        return count;
    }

    IEnumerator PulseOutline()
    {
        outline.enabled = true;
        Color c = outline.effectColor;

        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t) + 1f) * 0.5f);
            c.a = a;
            outline.effectColor = c;
            yield return null;
        }
    }

    void StopPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (outline != null)
            outline.enabled = false;
    }
}