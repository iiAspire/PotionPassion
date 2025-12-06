using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolTimer : MonoBehaviour
{
    public GameObject timerUIRoot;   // assign the whole timer object here (Image + Slider + anything else)
    public Slider timerSlider;
    public Transform spawnPoint;
    public float processingTime;

    private GameObject currentCard;

    private Action onCompleteCallback;

    private void Start()
    {
        if (timerUIRoot != null)
            timerUIRoot.SetActive(false);  // hide the whole timer at start
    }

    public void StartProcessing(GameObject cardObj, float time)
    {
        currentCard = cardObj;
        processingTime = time;

        // Snap card to spawn point
        if (spawnPoint != null)
        {
            currentCard.transform.SetParent(spawnPoint, false);
            currentCard.transform.localPosition = Vector3.zero;
        }

        // Show the whole timer UI
        if (timerUIRoot != null)
            timerUIRoot.SetActive(true);

        // Reset slider if assigned
        if (timerSlider != null)
        {
            timerSlider.maxValue = processingTime;
            timerSlider.value = processingTime;
        }

        StartCoroutine(ProcessRoutine());
    }

    public void StartTimer(Action onComplete, float time = 5f)
    {
        processingTime = time;
        onCompleteCallback = onComplete;

        if (timerUIRoot != null)
            timerUIRoot.SetActive(true);

        if (timerSlider != null)
        {
            timerSlider.maxValue = processingTime;
            timerSlider.value = processingTime;
        }

        StartCoroutine(ProcessRoutine());
    }

    private IEnumerator ProcessRoutine()
    {
        float elapsed = 0f;

        while (elapsed < processingTime)
        {
            elapsed += Time.deltaTime;

            if (timerSlider != null)
                timerSlider.value = processingTime - elapsed;

            yield return null;
        }

        CompleteProcessing();
    }

    private void CompleteProcessing()
    {
        if (timerUIRoot != null)
            timerUIRoot.SetActive(false);

        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }
}