using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class GlyphAnim : MonoBehaviour
{
    public bool isHero;

    [Header("Timing")]
    public float ritualDuration = 4f;
    public float pauseDuration = 0.1f;

    [Header("Orbit")]
    public float startRadius = 120f;
    public float orbitRadius = 100f;
    public float orbitSpeedStart = 90f;
    public float orbitSpeedEnd = 360f;

    [Header("Explode")]
    public float collapsePoint = 0.7f;
    public float explodeDistance = 1200f;

    private LearningRitualUI ritual;
    private RectTransform rt;
    private CanvasGroup cg;

    private float angle;
    private float angleOffset;

    private Vector2 collapseStartPos;
    private bool collapseCaptured;

    private Vector2 explodeDir;
    private bool explodeDirSet;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        ritual = GetComponentInParent<LearningRitualUI>();
        angle = Random.Range(0f, 360f);
        angleOffset = Random.Range(0f, 360f);

        collapseCaptured = false;
        explodeDirSet = false;
        rt.localScale = Vector3.one;
        cg.alpha = 0f;
    }

    void Update()
    {
        float t = ritual.RitualTime;
        if (t <= 0f) return;

        Vector2 pos = Vector2.zero;

        // -----------------------
        // ORBIT PHASE
        // -----------------------
        if (t < collapsePoint && !isHero)
        {
            float p = Mathf.SmoothStep(0f, 1f, t / collapsePoint);
            float radius = Mathf.Lerp(startRadius, orbitRadius, p);

            float speed = Mathf.Lerp(orbitSpeedStart, orbitSpeedEnd, p);
            angle += speed * Time.unscaledDeltaTime;

            float rad = (angle + angleOffset) * Mathf.Deg2Rad;
            pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            cg.alpha = Mathf.Clamp01(t * 2f);
        }
        else
        {
            // -----------------------
            // COLLAPSE TO CENTER
            // -----------------------
            if (!collapseCaptured)
            {
                collapseStartPos = rt.anchoredPosition;
                collapseCaptured = true;
            }

            float p = Mathf.SmoothStep(0f, 1f, (t - collapsePoint) / 0.15f);
            pos = Vector2.Lerp(collapseStartPos, Vector2.zero, p);
        }

        if (isHero && t < collapsePoint)
        {
            rt.anchoredPosition = Vector2.zero;
            cg.alpha = Mathf.Clamp01(t * 2f);
        }

        // -----------------------
        // PAUSE AT CENTER
        // -----------------------
        float explodeStart = collapsePoint + 0.15f;
        float pauseT = pauseDuration / ritualDuration;

        if (t >= explodeStart && t < explodeStart + pauseT)
        {
            rt.anchoredPosition = Vector2.zero;
            cg.alpha = 1f;
            return;
        }

        // -----------------------
        // EXPLOSION (scale only)
        // -----------------------
        if (t >= explodeStart + pauseT)
        {
            float e = Mathf.InverseLerp(explodeStart + pauseT, 1f, t);
            e = Mathf.SmoothStep(0f, 1f, e);

            // slight offset so they don’t perfectly overlap
            if (!explodeDirSet)
            {
                explodeDir = Random.insideUnitCircle * 40f;
                explodeDirSet = true;
            }

            rt.anchoredPosition = explodeDir; // stay near center

            float scale = Mathf.Lerp(1f, 5f, e);
            rt.localScale = Vector3.one * scale;

            if (!isHero)
            {
                Destroy(gameObject);
                return;
            }
        }

        rt.anchoredPosition = pos;
    }
}