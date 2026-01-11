using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuccessfulBrewPanelController : MonoBehaviour
{
    [Header("Panel Settings")]
    public GameObject panel;

    [Header("Card Display")]
    public Transform cardContainer;
    public GameObject cardPrefab; // Your existing CardComponent prefab

    [Header("Layout Settings")]
    public float cardSpacing = 20f;
    public float entrySpacing = 30f;

    [Header("Optional: Scroll Settings")]
    public ScrollRect scrollRect;

    [Header("Card Manager")]
    public CardManager cardManager; // Optional: assign your CardManager ScriptableObject

    private List<GameObject> instantiatedEntries = new List<GameObject>();

    private void OnEnable()
    {
        RefreshUI();
    }

    public void ShowSuccessfulBrews()
    {
        Debug.Log($"ShowSuccessfulBrews called. Panel: {(panel != null ? "exists" : "NULL")}");

        if (panel != null)
        {
            panel.SetActive(true);

            // Debug visibility
            Canvas canvas = panel.GetComponentInParent<Canvas>();
            Debug.Log($"Panel Canvas: {(canvas != null ? canvas.name : "NULL")}, Render Mode: {(canvas != null ? canvas.renderMode.ToString() : "NULL")}");

            RectTransform rt = panel.GetComponent<RectTransform>();
            Debug.Log($"Panel RectTransform - Active: {panel.activeSelf}, Position: {(rt != null ? rt.anchoredPosition.ToString() : "NULL")}, Size: {(rt != null ? rt.sizeDelta.ToString() : "NULL")}");

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null)
                Debug.Log($"CanvasGroup found - Alpha: {cg.alpha}, Interactable: {cg.interactable}, BlocksRaycasts: {cg.blocksRaycasts}");
        }

        RefreshUI();
    }

    public void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void RefreshUI()
    {
        Debug.Log($"RefreshUI called. cardContainer: {(cardContainer != null ? "exists" : "NULL")}, cardPrefab: {(cardPrefab != null ? "exists" : "NULL")}");

        if (cardContainer == null || cardPrefab == null)
        {
            Debug.LogWarning("SuccessfulBrewPanelController: Missing cardContainer or cardPrefab!");
            return;
        }

        // Clear existing entries
        ClearEntries();

        // Create card displays for each successful brew
        var successfulList = GameData.Instance.successfulBrews;

        Debug.Log($"Successful brews count: {(successfulList != null ? successfulList.Count.ToString() : "NULL")}");

        if (successfulList == null || successfulList.Count == 0)
        {
            Debug.Log("No successful brews to display");
            return;
        }

        foreach (var combo in successfulList)
        {
            if (combo == null || combo.Ingredients == null || combo.Ingredients.Count == 0)
                continue;

            CreateSuccessfulBrewEntry(combo);
        }

        // Reset scroll position to top
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreateSuccessfulBrewEntry(SpellCombo combo)
    {
        // Create a container for this successful brew entry
        GameObject entryContainer = new GameObject($"SuccessfulEntry_{combo.SpellName}");
        entryContainer.transform.SetParent(cardContainer, false);

        RectTransform entryRect = entryContainer.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 180);

        // Add layout component for organizing ingredient cards horizontally
        HorizontalLayoutGroup layout = entryContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = cardSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        // Add background with success color (green tint)
        Image bg = entryContainer.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.3f, 0.15f, 0.7f); // Greenish background
        bg.raycastTarget = false;

        // Add ContentSizeFitter
        ContentSizeFitter fitter = entryContainer.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        instantiatedEntries.Add(entryContainer);

        // Create visual cards for each ingredient
        foreach (string ingredientName in combo.Ingredients)
        {
            CardData ingredientData = FindCardDataByName(ingredientName);

            if (ingredientData != null)
            {
                GameObject cardObj = Instantiate(cardPrefab, entryContainer.transform);

                CardComponent cardComp = cardObj.GetComponent<CardComponent>();
                if (cardComp != null)
                {
                    cardComp.SetCardData(ingredientData, false);
                    DisableCardInteraction(cardComp);
                    AddCardTooltip(cardObj, ingredientData.cardName);
                    cardObj.transform.localScale = Vector3.one;
                }
            }
            else
            {
                CreatePlaceholderCard(entryContainer.transform, ingredientName);
            }
        }

        // Add arrow separator
        AddArrowSeparator(entryContainer);

        // Add result card if available
        if (combo.ResultCard != null)
        {
            GameObject resultCardObj = Instantiate(cardPrefab, entryContainer.transform);

            CardComponent resultComp = resultCardObj.GetComponent<CardComponent>();
            if (resultComp != null)
            {
                resultComp.SetCardData(combo.ResultCard, false);
                DisableCardInteraction(resultComp);
                AddCardTooltip(resultCardObj, combo.ResultCard.cardName);
                resultCardObj.transform.localScale = Vector3.one;

                // Optional: Add glow effect to result card
                AddResultGlow(resultCardObj);
            }
        }
        else
        {
            // Show spell name if no result card
            AddSpellNameLabel(entryContainer, combo.SpellName);
        }

        // Add success indicator
        //AddSuccessIndicator(entryContainer);
    }

    private CardData FindCardDataByName(string cardName)
    {
        // Method 1: Use CardManager if assigned
        if (cardManager != null)
        {
            CardData card = cardManager.GetCardByName(cardName);
            if (card != null)
                return card;
        }

        // Method 2: Search all CardData in Resources/Cards folder
        CardData[] allCards = Resources.LoadAll<CardData>("Cards");
        foreach (var card in allCards)
        {
            if (card != null && card.cardName == cardName)
                return card;
        }

        // Method 3: Try direct load
        CardData loadedCard = Resources.Load<CardData>($"Cards/{cardName}");
        if (loadedCard != null)
            return loadedCard;

        // Method 4: Try different folder structures
        string[] possiblePaths = {
            $"Cards/Ingredients/{cardName}",
            $"Cards/Materials/{cardName}",
            $"Data/Cards/{cardName}"
        };

        foreach (string path in possiblePaths)
        {
            loadedCard = Resources.Load<CardData>(path);
            if (loadedCard != null)
                return loadedCard;
        }

        Debug.LogWarning($"Could not find CardData for: {cardName}");
        return null;
    }

    private void CreatePlaceholderCard(Transform parent, string ingredientName)
    {
        GameObject placeholder = new GameObject($"Placeholder_{ingredientName}");
        placeholder.transform.SetParent(parent, false);

        RectTransform rt = placeholder.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 140);

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
        // Disable dragging
        var draggable = card.GetComponent<CardDragDrop>();
        if (draggable != null)
            draggable.enabled = false;

        // Disable any buttons
        var buttons = card.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
            btn.interactable = false;

        // Disable GraphicRaycaster if present
        var raycaster = card.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
            raycaster.enabled = false;

        // Add/modify CanvasGroup to block dragging but allow hover events
        var canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = card.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true; // Keep true for hover to work
    }

    private void AddCardTooltip(GameObject cardObj, string cardName)
    {
        // Add tooltip component
        CardTooltip tooltip = cardObj.AddComponent<CardTooltip>();
        tooltip.tooltipText = cardName;
    }

    private void AddArrowSeparator(GameObject parent)
    {
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(parent.transform, false);

        RectTransform rt = arrow.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        TMP_Text text = arrow.AddComponent<TextMeshProUGUI>();
        text.text = "→";
        text.fontSize = 32;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.5f, 1f, 0.5f, 1f); // Green arrow
        text.fontStyle = FontStyles.Bold;
    }

    private void AddSpellNameLabel(GameObject parent, string spellName)
    {
        GameObject labelObj = new GameObject("SpellNameLabel");
        labelObj.transform.SetParent(parent.transform, false);

        RectTransform rt = labelObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 140);

        Image bg = labelObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.4f, 0.2f, 0.9f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(labelObj.transform, false);

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = spellName;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -20);
    }

    private void AddResultGlow(GameObject resultCard)
    {
        // Add a subtle glow outline
        Outline outline = resultCard.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 1f, 0.5f, 1f); // Green glow
        outline.effectDistance = new Vector2(2, 2);
    }

    //private void AddSuccessIndicator(GameObject entryContainer)
    //{
    //    GameObject indicator = new GameObject("SuccessIndicator");
    //    indicator.transform.SetParent(entryContainer.transform, false);
    //    indicator.transform.SetAsLastSibling();

    //    RectTransform iconRect = indicator.AddComponent<RectTransform>();
    //    iconRect.anchorMin = new Vector2(1, 1);
    //    iconRect.anchorMax = new Vector2(1, 1);
    //    iconRect.pivot = new Vector2(1, 1);
    //    iconRect.anchoredPosition = new Vector2(-10, -10);
    //    iconRect.sizeDelta = new Vector2(40, 40);

    //    Image iconImage = indicator.AddComponent<Image>();
    //    iconImage.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green background

    //    // Always create the checkmark text
    //    GameObject checkTextObj = new GameObject("CheckText");
    //checkTextObj.transform.SetParent(indicator.transform, false);

    //    TMP_Text checkText = checkTextObj.AddComponent<TextMeshProUGUI>();
    //checkText.text = "✓";
    //    checkText.fontSize = 32;
    //    checkText.alignment = TextAlignmentOptions.Center;
    //    checkText.color = Color.white;
    //    checkText.fontStyle = FontStyles.Bold;

    //    RectTransform checkRect = checkTextObj.GetComponent<RectTransform>();
    //checkRect.anchorMin = Vector2.zero;
    //    checkRect.anchorMax = Vector2.one;
    //    checkRect.sizeDelta = Vector2.zero;
    //}

private void ClearEntries()
    {
        foreach (var entry in instantiatedEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        instantiatedEntries.Clear();
    }

    private void OnDisable()
    {
        ClearEntries();
    }
}