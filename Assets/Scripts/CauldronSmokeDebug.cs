using UnityEngine;
using UnityEngine.UI;

public class CauldronSmokeDebug : MonoBehaviour
{
    public RectTransform container;
    public GameObject smokePrefab;

    void Start()
    {
        Debug.Log("SmokeDebug START — container: " + container + " prefab: " + smokePrefab);

        // Spawn 1 big visible puff at Start()
        SpawnDebugPuff();
    }

    void SpawnDebugPuff()
    {
        GameObject obj = Instantiate(smokePrefab, container);
        RectTransform rt = obj.GetComponent<RectTransform>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();

        Debug.Log("Spawned smoke puff: " + obj);

        // VERY LARGE, VERY OBVIOUS
        rt.sizeDelta = new Vector2(200, 200);
        rt.localScale = Vector3.one;

        // Put it right in the center — GUARANTEED VISIBLE
        rt.anchoredPosition = Vector2.zero;

        // Full alpha
        if (cg != null) cg.alpha = 1f;
        else Debug.Log("No CanvasGroup found!");

        // No animation — just show it.
    }
}