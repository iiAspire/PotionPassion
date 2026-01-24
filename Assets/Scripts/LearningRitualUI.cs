using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LearningRitualUI : MonoBehaviour
{
    public CanvasGroup clouds;
    public Transform glyphContainer;
    public GlyphLibrary glyphLibrary;
    public GameObject glyphPrefab;
    public float RitualTime;

    public IEnumerator Play(SpellCombo combo)
    {
        foreach (Transform c in glyphContainer)
            Destroy(c.gameObject);

        gameObject.SetActive(true);

        Time.timeScale = 0.2f;

        yield return Fade(0, 1);

        RitualTime = 0f;

        int glyphCount = Mathf.Max(1, combo.Ingredients?.Count ?? 0);

        // background glyphs
        for (int i = 0; i < glyphCount - 1; i++)
        {
            SpawnGlyph(glyphLibrary.GetRandomTierGlyph(combo.SpellLevel), false);
        }

        // small delay for readability
        yield return new WaitForSecondsRealtime(0.15f);

        // hero glyph
        Sprite hero = glyphLibrary.GetSpellGlyph(combo.SpellName);
        SpawnGlyph(hero, true);

        // 2️⃣ Run ONE shared ritual clock
        float ritualDuration = 4.0f;

        while (RitualTime < 1f)
        {
            RitualTime += Time.unscaledDeltaTime / ritualDuration;
            yield return null;
        }

        // 3️⃣ Small hold for readability
        yield return new WaitForSecondsRealtime(0.3f);

        yield return Fade(1, 0);

        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    void SpawnGlyph(Sprite sprite, bool isHero)
    {
        if (sprite != null)
            Debug.Log($"SPAWN {(isHero ? "HERO" : "BG")} → {sprite.name}");

        GameObject g = Instantiate(glyphPrefab, glyphContainer);
        var anim = g.GetComponent<GlyphAnim>();
        anim.isHero = isHero;

        foreach (var img in g.GetComponentsInChildren<Image>())
        {
            img.sprite = null;
            img.color = Color.white;
        }

        if (sprite != null)
        {
            foreach (var img in g.GetComponentsInChildren<Image>())
                img.sprite = sprite;
        }
    }


    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime;
            clouds.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }
}