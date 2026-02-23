using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SuccessfulBrewBookController : MonoBehaviour
{
    [Header("Panel Settings")]
    public GameObject panel;

    [Header("Book Pages")]
    public RectTransform leftContent;
    public RectTransform rightContent;
    public CanvasGroup leftPageGroup;
    public CanvasGroup rightPageGroup;

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;
    public TMP_Text pageNumberText;

    [Header("Card Display")]
    public GameObject cardPrefab;
    public CardManager cardManager;

    [Header("Layout")]
    public float cardSpacing = 16f;
    public float pagePadding = 24f;

    [Header("Page Turn Animation")]
    public float turnDuration = 0.18f;
    public float slidePixels = 40f;

    [Header("Text Colors")]
    public Color titleColor = new Color(0.25f, 0.18f, 0.05f); //parchment brown
    public Color headerColor = new Color(0.30f, 0.22f, 0.07f); //darker brown
    public Color bodyColor = new Color(0.22f, 0.16f, 0.05f); //ink colour
    public Color arrowColor = new Color(0.30f, 0.22f, 0.07f); //darker brown

    [Header("Tabs")]
    public Button basicTabButton;
    public Button intermediateTabButton;
    public Button advancedTabButton;

    private SpellTier selectedTier = SpellTier.Basic;

    private readonly List<SpellCombo> pages = new();
    private int spreadIndex = 0;
    private bool isTurning = false;
    private Vector2 leftHomePos;
    private Vector2 rightHomePos;
    private bool homeCached = false;

    [Header("Tab New Indicators")]
    public GameObject basicNewIndicator;
    public GameObject intermediateNewIndicator;
    public GameObject advancedNewIndicator;

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(PrevSpread);
        if (nextButton != null) nextButton.onClick.AddListener(NextSpread);

        if (basicTabButton != null) basicTabButton.onClick.AddListener(() => SetTier(SpellTier.Basic));
        if (intermediateTabButton != null) intermediateTabButton.onClick.AddListener(() => SetTier(SpellTier.Intermediate));
        if (advancedTabButton != null) advancedTabButton.onClick.AddListener(() => SetTier(SpellTier.Advanced));
    }

    private void SetTier(SpellTier tier)
    {
        selectedTier = tier;
        Refresh();
        UpdateTabVisuals();
        UpdateNewIndicators();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);

        CacheHomePositions();

        // snap to home every time you open
        if (leftPageGroup != null) leftPageGroup.GetComponent<RectTransform>().anchoredPosition = leftHomePos;
        if (rightPageGroup != null) rightPageGroup.GetComponent<RectTransform>().anchoredPosition = rightHomePos;

        Refresh();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        ClearPage(leftContent);
        ClearPage(rightContent);
    }

    public void Refresh()
    {
        BuildPagesFromGameData();
        UpdateTabAvailability();
        UpdateTabVisuals();
        UpdateNewIndicators();
        spreadIndex = 0;
        RenderSpreadImmediate();
        UpdateNav();
    }

    private void CacheHomePositions()
    {
        if (homeCached) return;

        if (leftPageGroup != null)
            leftHomePos = leftPageGroup.GetComponent<RectTransform>().anchoredPosition;

        if (rightPageGroup != null)
            rightHomePos = rightPageGroup.GetComponent<RectTransform>().anchoredPosition;

        homeCached = true;
    }

    void UpdateNewIndicators()
    {
        if (GameData.Instance == null || GameInitialization.Recipes == null)
            return;

        bool HasNewInTier(SpellTier tier)
        {
            if (tier == selectedTier)
                return false;

            foreach (var combo in GameInitialization.Recipes.AllCombos)
            {
                if (combo == null) continue;

                if (combo.SpellLevel != tier) continue;

                if (!GameData.Instance.knownRecipes.Contains(combo.SpellName))
                    continue;

                if (GameData.Instance.GetRecipeStatus(combo.SpellName) == RecipeStatus.New)
                    return true;
            }

            return false;
        }

        if (basicNewIndicator != null)
            basicNewIndicator.SetActive(HasNewInTier(SpellTier.Basic));

        if (intermediateNewIndicator != null)
            intermediateNewIndicator.SetActive(HasNewInTier(SpellTier.Intermediate));

        if (advancedNewIndicator != null)
            advancedNewIndicator.SetActive(HasNewInTier(SpellTier.Advanced));
    }

    private void BuildPagesFromGameData()
    {
        pages.Clear();

        if (GameData.Instance == null)
            return;

        if (GameInitialization.Recipes == null)
            return;

        // Get known recipe names
        var known = GameData.Instance.knownRecipes;

        if (known == null || known.Count == 0)
            return;

        // Convert names → SpellCombo objects
        foreach (var combo in GameInitialization.Recipes.AllCombos)
        {
            if (combo == null) continue;

            if (!known.Contains(combo.SpellName))
                continue;

            if (combo.SpellLevel != selectedTier)
                continue;

            pages.Add(combo);
        }

        // Optional: sort by tier then name
        pages.Sort((a, b) =>
        {
            int tierCompare = a.SpellLevel.CompareTo(b.SpellLevel);
            if (tierCompare != 0) return tierCompare;
            return string.Compare(a.SpellName, b.SpellName);
        });
    }

    private void UpdateTabAvailability()
    {
        if (GameData.Instance == null || GameInitialization.Recipes == null) return;

        bool HasTier(SpellTier tier)
        {
            foreach (var c in GameInitialization.Recipes.AllCombos)
                if (c != null && c.SpellLevel == tier && GameData.Instance.knownRecipes.Contains(c.SpellName))
                    return true;
            return false;
        }

        if (basicTabButton != null) basicTabButton.interactable = HasTier(SpellTier.Basic);
        if (intermediateTabButton != null) intermediateTabButton.interactable = HasTier(SpellTier.Intermediate);
        if (advancedTabButton != null) advancedTabButton.interactable = HasTier(SpellTier.Advanced);
    }

    private void PrevSpread()
    {
        if (isTurning) return;
        if (spreadIndex <= 0) return;
        StartCoroutine(TurnToSpread(spreadIndex - 1, direction: -1));
    }

    private void NextSpread()
    {
        if (isTurning) return;
        int maxSpread = Mathf.Max(0, (pages.Count - 1) / 2);
        if (spreadIndex >= maxSpread) return;
        StartCoroutine(TurnToSpread(spreadIndex + 1, direction: +1));
    }

    private IEnumerator TurnToSpread(int newSpreadIndex, int direction)
    {
        isTurning = true;
        SetNavInteractable(false);

        // Fade + slide out
        yield return AnimatePages(outAlpha: 0f, direction: direction);

        spreadIndex = newSpreadIndex;
        RenderSpreadImmediate();

        // Fade + slide in
        yield return AnimatePages(outAlpha: 1f, direction: direction);

        if (leftPageGroup != null) leftPageGroup.GetComponent<RectTransform>().anchoredPosition = leftHomePos;
        if (rightPageGroup != null) rightPageGroup.GetComponent<RectTransform>().anchoredPosition = rightHomePos;

        UpdateNav();
        SetNavInteractable(true);
        isTurning = false;
    }

    private IEnumerator AnimatePages(float outAlpha, int direction)
    {
        float t = 0f;

        // Always use home, never current
        Vector2 leftStart = leftHomePos;
        Vector2 rightStart = rightHomePos;

        // For fade-out, slide toward target. For fade-in, slide back to home.
        Vector2 leftOffset = new Vector2(slidePixels * direction, 0);
        Vector2 rightOffset = new Vector2(slidePixels * direction, 0);

        Vector2 leftFrom, leftTo;
        Vector2 rightFrom, rightTo;

        if (outAlpha <= 0.01f)
        {
            // fading OUT: home -> home + offset
            leftFrom = leftStart;
            leftTo = leftStart + leftOffset;

            rightFrom = rightStart;
            rightTo = rightStart + rightOffset;
        }
        else
        {
            // fading IN: home - offset -> home
            leftFrom = leftStart - leftOffset;
            leftTo = leftStart;

            rightFrom = rightStart - rightOffset;
            rightTo = rightStart;
        }

        float leftAlphaStart = (leftPageGroup != null) ? leftPageGroup.alpha : 1f;
        float rightAlphaStart = (rightPageGroup != null) ? rightPageGroup.alpha : 1f;

        float leftAlphaTarget = outAlpha;
        float rightAlphaTarget = outAlpha;

        // Ensure starting positions are correct at the beginning of each phase
        if (leftPageGroup != null) leftPageGroup.GetComponent<RectTransform>().anchoredPosition = leftFrom;
        if (rightPageGroup != null) rightPageGroup.GetComponent<RectTransform>().anchoredPosition = rightFrom;

        while (t < turnDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / turnDuration);
            u = u * u * (3f - 2f * u);

            if (leftPageGroup != null)
            {
                leftPageGroup.alpha = Mathf.Lerp(leftAlphaStart, leftAlphaTarget, u);
                leftPageGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(leftFrom, leftTo, u);
            }

            if (rightPageGroup != null)
            {
                rightPageGroup.alpha = Mathf.Lerp(rightAlphaStart, rightAlphaTarget, u);
                rightPageGroup.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(rightFrom, rightTo, u);
            }

            yield return null;
        }

        // Snap exactly to eliminate drift
        if (leftPageGroup != null)
        {
            leftPageGroup.alpha = leftAlphaTarget;
            leftPageGroup.GetComponent<RectTransform>().anchoredPosition = leftTo;
        }

        if (rightPageGroup != null)
        {
            rightPageGroup.alpha = rightAlphaTarget;
            rightPageGroup.GetComponent<RectTransform>().anchoredPosition = rightTo;
        }
    }

    private void RenderSpreadImmediate()
    {
        ClearPage(leftContent);
        ClearPage(rightContent);

        int leftIdx = spreadIndex * 2;
        int rightIdx = leftIdx + 1;

        RenderSinglePage(leftContent, leftIdx);
        RenderSinglePage(rightContent, rightIdx);
    }

    private void RenderSinglePage(RectTransform pageRoot, int pageIdx)
    {
        if (pageRoot == null) return;

        if (pageIdx < 0 || pageIdx >= pages.Count)
        {
            // Blank page
            return;
        }

        SpellCombo combo = pages[pageIdx];

        // Page container (vertical)
        GameObject page = new GameObject($"RecipePage_{pageIdx}_{combo.SpellName}");
        page.transform.SetParent(pageRoot, false);

        RectTransform pageRect = page.AddComponent<RectTransform>();
        // Force the page container to be top-anchored inside pageRoot
        pageRect.anchorMin = new Vector2(0f, 1f);
        pageRect.anchorMax = new Vector2(1f, 1f);
        pageRect.pivot = new Vector2(0.5f, 1f);

        // Give it a fixed top band area (tune height)
        // Start with something like 520–650 depending on your page size.
        pageRect.sizeDelta = new Vector2(0f, 600f);

        // Position it down from the top by padding
        pageRect.anchoredPosition = new Vector2(0f, -pagePadding);

        VerticalLayoutGroup vlg = page.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = page.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Title
        AddHeader(page.transform, combo.SpellName);

        // Ingredients label
        AddSubHeader(page.transform, "Ingredients");

        // Ingredients row
        Transform ingRow = CreateRow(page.transform, "IngredientsRow");
        foreach (string ingredientName in combo.Ingredients)
        {
            CardData ingredientData = FindCardDataByName(ingredientName);
            if (ingredientData != null) SpawnStaticCard(ingRow, ingredientData, scale: 1.0f);
            else CreatePlaceholderCard(ingRow, ingredientName);
        }

        // Result label
        AddSubHeader(page.transform, "Result");

        // Result row (arrow + result)
        Transform resRow = CreateRow(page.transform, "ResultRow");
        AddArrow(resRow);

        if (combo.ResultCard != null)
            SpawnStaticCard(resRow, combo.ResultCard, scale: 0.90f, glow: true);
        else
            AddNote(resRow, "Unknown Result");

        // Optional notes section
        AddSubHeader(page.transform, "Notes");
        var status = GameData.Instance != null ? GameData.Instance.GetRecipeStatus(combo.SpellName) : RecipeStatus.New;

        string note = status == RecipeStatus.New
            ? "New recipe"
            : "";

        AddBodyText(page.transform, note);
    }

    private void UpdateNav()
    {
        int maxSpread = Mathf.Max(0, (pages.Count - 1) / 2);

        if (prevButton != null) prevButton.interactable = spreadIndex > 0;
        if (nextButton != null) nextButton.interactable = spreadIndex < maxSpread;

        if (pageNumberText != null)
        {
            int total = pages.Count;
            int left = spreadIndex * 2 + 1;
            int right = left + 1;

            if (right <= total) pageNumberText.text = $"Pages {left}-{right} of {total}";
            else pageNumberText.text = $"Page {left} of {total}";
        }
    }

    private void SetNavInteractable(bool state)
    {
        if (prevButton != null) prevButton.interactable = state && prevButton.interactable;
        if (nextButton != null) nextButton.interactable = state && nextButton.interactable;
    }

    private void ClearPage(RectTransform root)
    {
        if (root == null) return;
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    // --- UI building helpers ---

    private void AddHeader(Transform parent, string text)
    {
        var go = new GameObject("Header");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 48;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold | FontStyles.Italic;
        t.color = titleColor;
    }

    private void AddSubHeader(Transform parent, string text)
    {
        var go = new GameObject("SubHeader");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 24;
        t.alignment = TextAlignmentOptions.Left;
        t.fontStyle = FontStyles.SmallCaps;
        t.alpha = 0.85f;
        t.color = headerColor;
    }

    private void AddBodyText(Transform parent, string text)
    {
        var go = new GameObject("BodyText");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 24;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.fontStyle = FontStyles.Italic;
        t.enableWordWrapping = true;
        t.alpha = 0.9f;
        t.color = bodyColor;
    }

    private Transform CreateRow(Transform parent, string name)
    {
        var row = new GameObject(name);
        row.transform.SetParent(parent, false);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = cardSpacing;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(6, 6, 6, 6);

        var fitter = row.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return row.transform;
    }

    private void AddArrow(Transform parent)
    {
        var go = new GameObject("Arrow");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = "→";
        t.fontSize = 72;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.alpha = 0.9f;
        t.color = arrowColor;
    }

    private void AddNote(Transform parent, string msg)
    {
        var go = new GameObject("Note");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = msg;
        t.fontSize = 14;
        t.alignment = TextAlignmentOptions.Center;
        t.alpha = 0.85f;
    }

    private void SpawnStaticCard(Transform parent, CardData data, float scale = 1f, bool glow = false)
    {
        GameObject cardObj = Instantiate(cardPrefab, parent);
        CardComponent cardComp = cardObj.GetComponent<CardComponent>();
        if (cardComp != null)
        {
            cardComp.SetCardData(data, false);
            DisableCardInteraction(cardComp);
            AddCardTooltip(cardObj, data.cardName);
        }

        cardObj.transform.localScale = Vector3.one * scale;

        if (glow) AddResultGlow(cardObj);
    }

    // --- Reused logic from your existing controllers ---

    private CardData FindCardDataByName(string cardName)
    {
        if (cardManager != null)
        {
            CardData card = cardManager.GetCardByName(cardName);
            if (card != null) return card;
        }

        CardData[] allCards = Resources.LoadAll<CardData>("Cards");
        foreach (var card in allCards)
        {
            if (card != null && card.cardName == cardName)
                return card;
        }

        CardData loadedCard = Resources.Load<CardData>($"Cards/{cardName}");
        if (loadedCard != null) return loadedCard;

        string[] possiblePaths =
        {
            $"Cards/Ingredients/{cardName}",
            $"Cards/Materials/{cardName}",
            $"Data/Cards/{cardName}"
        };

        foreach (string path in possiblePaths)
        {
            loadedCard = Resources.Load<CardData>(path);
            if (loadedCard != null) return loadedCard;
        }

        Debug.LogWarning($"Could not find CardData for: {cardName}");
        return null;
    }

    private void CreatePlaceholderCard(Transform parent, string ingredientName)
    {
        GameObject placeholder = new GameObject($"Placeholder_{ingredientName}");
        placeholder.transform.SetParent(parent, false);

        RectTransform rt = placeholder.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(90, 125);

        Image img = placeholder.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(placeholder.transform, false);

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = ingredientName;
        text.fontSize = 12;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void DisableCardInteraction(CardComponent card)
    {
        var draggable = card.GetComponent<CardDragDrop>();
        if (draggable != null) draggable.enabled = false;

        var buttons = card.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons) btn.interactable = false;

        var raycaster = card.GetComponent<GraphicRaycaster>();
        if (raycaster != null) raycaster.enabled = false;

        var canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = card.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
    }

    private void AddCardTooltip(GameObject cardObj, string cardName)
    {
        CardTooltip tooltip = cardObj.AddComponent<CardTooltip>();
        tooltip.tooltipText = cardName;
    }

    private void AddResultGlow(GameObject resultCard)
    {
        Outline outline = resultCard.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 1f, 0.5f, 1f);
        outline.effectDistance = new Vector2(2, 2);
    }

    void UpdateTabVisuals()
    {
        SetTabState(basicTabButton, selectedTier == SpellTier.Basic);
        SetTabState(intermediateTabButton, selectedTier == SpellTier.Intermediate);
        SetTabState(advancedTabButton, selectedTier == SpellTier.Advanced);
    }

    void SetTabState(Button btn, bool selected)
    {
        if (btn == null) return;

        // Make selected tab non-interactable so it uses Disabled sprite
        btn.interactable = !selected;
    }
}