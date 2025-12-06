using UnityEngine;
using UnityEngine.UI;

public class HeatGlowFlicker : MonoBehaviour
{
    public Image glow;
    public float speed = 5f;
    public float intensity = 0.1f;

    Color baseColor;

    void Start()
    {
        baseColor = glow.color;
    }

    void Update()
    {
        float f = (Mathf.Sin(Time.time * speed) * intensity) + 1f;
        glow.color = new Color(baseColor.r * f, baseColor.g * f, baseColor.b * f, baseColor.a);
    }
}