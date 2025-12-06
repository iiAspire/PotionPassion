using UnityEngine;

public class SmokeTestScript : MonoBehaviour
{
    RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        Debug.Log("SmokeTestScript Started");
    }

    void Update()
    {
        // Slowly rise up
        rt.anchoredPosition += new Vector2(0, 40f) * Time.deltaTime;

        // Rotate
        rt.Rotate(0f, 0f, 20f * Time.deltaTime);

        // Pulse scale
        float pulse = Mathf.Sin(Time.time * 2f) * 0.1f;
        rt.localScale = Vector3.one * (1f + pulse);
    }
}