using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spellcraft/Glyph Library")]
public class GlyphLibrary : ScriptableObject
{
    [Serializable]
    public class SpellGlyph
    {
        public string spellName;
        public Sprite glyph;
    }

    [Serializable]
    public class TierGlyphs
    {
        public SpellTier tier;
        public List<Sprite> glyphs;
    }

    [Header("Hero Glyphs (per spell)")]
    public List<SpellGlyph> spellGlyphs = new();

    [Header("Background Glyphs (per tier)")]
    public List<TierGlyphs> tierGlyphs = new();

    // ---------- runtime-only (never saved) ----------
    private Dictionary<string, Sprite> spellLookup;
    private Dictionary<SpellTier, List<Sprite>> runtimeTierGlyphs;

    private void OnEnable()
    {
        BuildRuntimeData();
    }

    private void BuildRuntimeData()
    {
        // build hero lookup
        spellLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in spellGlyphs)
        {
            if (!string.IsNullOrEmpty(s.spellName) && s.glyph != null)
                spellLookup[s.spellName] = s.glyph;
        }

        // clone tier glyph lists (IMPORTANT: prevents asset mutation)
        runtimeTierGlyphs = new Dictionary<SpellTier, List<Sprite>>();
        foreach (var tier in tierGlyphs)
        {
            runtimeTierGlyphs[tier.tier] = new List<Sprite>(tier.glyphs);
        }
    }

    // ---------- public API ----------

    // HERO glyph (exact spell match)
    public Sprite GetSpellGlyph(string spellName)
    {
        if (spellLookup == null)
            BuildRuntimeData();

        return spellLookup.TryGetValue(spellName, out var s) ? s : null;
    }

    // BACKGROUND glyph (random by tier)
    public Sprite GetRandomTierGlyph(SpellTier tier)
    {
        if (runtimeTierGlyphs == null)
            BuildRuntimeData();

        if (!runtimeTierGlyphs.TryGetValue(tier, out var list) || list.Count == 0)
            return null;

        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    // DEBUG / SAFETY
    public bool IsHeroGlyph(Sprite s)
    {
        if (spellLookup == null)
            BuildRuntimeData();

        return spellLookup.ContainsValue(s);
    }
}