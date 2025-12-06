using UnityEngine;
using UnityEngine.UI;

public class CauldronBubble : MonoBehaviour
{
    public float speed = 2f;
    public float intensity = 0.05f;

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * intensity;
        transform.localScale = baseScale + new Vector3(offset, offset, 0f);
    }
}