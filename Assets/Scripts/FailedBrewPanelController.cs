using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FailedBrewPanelController : MonoBehaviour
{
    public static FailedBrewPanelController Instance;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This keeps it alive across scene loads!
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void ShowFailedBrews()
    {
        Debug.Log($"ShowFailedBrews called. Panel: {(panel != null ? "exists" : "NULL")}");

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
            Debug.LogWarning("FailedBrewPanelController: Missing cardContainer or cardPrefab!");
            return;
        }

        // Clear existing entries
        ClearEntries();

        // Create card displays for each failed brew
        var failedList = GameData.Instance.failedBrews;

        Debug.Log($"Failed brews count: {(failedList != null ? failedList.Count.ToString() : "NULL")}");

        if (failedList == null || failedList.Count == 0)
        {
            Debug.Log("No failed brews to display");
            return;
        }

        foreach (var combo in failedList)
        {
            if (combo == null || combo.Ingredients == null || combo.Ingredients.Count == 0)
                continue;

            CreateFailedBrewEntry(combo);
        }

        // Reset scroll position to top
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreateFailedBrewEntry(SpellCombo combo)
    {
        // Create a container for this failed brew entry
        GameObject entryContainer = new GameObject($"FailedEntry_{combo.SpellName}");
        entryContainer.transform.SetParent(cardContainer, false);

        RectTransform entryRect = entryContainer.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 180); // Adjust height as needed

        // Add layout component for organizing ingredient cards horizontally
        HorizontalLayoutGroup layout = entryContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = cardSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        // Optional: Add background
        Image bg = entryContainer.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        bg.raycastTarget = false;

        // Add ContentSizeFitter
        ContentSizeFitter fitter = entryContainer.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        instantiatedEntries.Add(entryContainer);

        // Create visual cards for each ingredient
        foreach (string ingredientName in combo.Ingredients)
        {
            // Find the ingredient CardData
            CardData ingredientData = FindCardDataByName(ingredientName);

            if (ingredientData != null)
            {
                GameObject cardObj = Instantiate(cardPrefab, entryContainer.transform);

                CardComponent cardComp = cardObj.GetComponent<CardComponent>();
                if (cardComp != null)
                {
                    // Set the card data (visual only, don't create runtime instance)
                    cardComp.SetCardData(ingredientData, false);

                    // Disable all interaction
                    DisableCardInteraction(cardComp);
                    AddCardTooltip(cardObj, ingredientData.cardName);
                    // Scale down slightly for display
                    cardObj.transform.localScale = Vector3.one;
                }
            }
            else
            {
                // Create a placeholder if card data not found
                CreatePlaceholderCard(entryContainer.transform, ingredientName);
            }
        }

        // Add a "failed" indicator overlay
        //AddFailedIndicator(entryContainer);
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

        // Method 3: Try direct load (assumes Cards folder structure)
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

        // Add text showing the ingredient name
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

        // Add/modify CanvasGroup to block all interaction
        var canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = card.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
    }

    //private void AddFailedIndicator(GameObject entryContainer)
    //{
    //    // Create a "Failed" or "X" overlay in the top-right corner
    //    GameObject indicator = new GameObject("FailedIndicator");
    //    indicator.transform.SetParent(entryContainer.transform, false);
    //    indicator.transform.SetAsLastSibling(); // Bring to front

    //    RectTransform iconRect = indicator.AddComponent<RectTransform>();
    //    iconRect.anchorMin = new Vector2(1, 1);
    //    iconRect.anchorMax = new Vector2(1, 1);
    //    iconRect.pivot = new Vector2(1, 1);
    //    iconRect.anchoredPosition = new Vector2(-10, -10);
    //    iconRect.sizeDelta = new Vector2(40, 40);

    //    Image iconImage = indicator.AddComponent<Image>();
    //    iconImage.color = new Color(0.8f, 0.1f, 0.1f, 0.9f); // Red background

    //    // Always create the X text (don't rely on sprite)
    //    GameObject xTextObj = new GameObject("XText");
    //    xTextObj.transform.SetParent(indicator.transform, false);

    //    TMP_Text xText = xTextObj.AddComponent<TextMeshProUGUI>();
    //    xText.text = "✗";
    //    xText.fontSize = 28;
    //    xText.alignment = TextAlignmentOptions.Center;
    //    xText.color = Color.white;
    //    xText.fontStyle = FontStyles.Bold;

    //    RectTransform xRect = xTextObj.GetComponent<RectTransform>();
    //    xRect.anchorMin = Vector2.zero;
    //    xRect.anchorMax = Vector2.one;
    //    xRect.sizeDelta = Vector2.zero;
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

    private void AddCardTooltip(GameObject cardObj, string cardName)
    {
        // Add tooltip component
        CardTooltip tooltip = cardObj.AddComponent<CardTooltip>();
        tooltip.tooltipText = cardName;
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
}