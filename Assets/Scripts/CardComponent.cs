using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardComponent : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;              // Main card icon
    public Image backgroundImage;        // Frame / background
    public Image typeIconImage;          // Bottom-left type icon
    public Text nameText;                // Card name
    public Image processedIconImage;     // Bottom-right processed icon

    [Header("Processing Visual Outputs UI")]
    public Transform outputIconsParent;          // parent container for extra icons
    public GameObject outputIconPrefab;          // prefab with icon + text
    public Image outputPartIcon;                // top-left
    public TextMeshProUGUI stackCountText;             // top-right

    [SerializeField] private CardData cardData;
    public CardData CardData => cardData;

    public SpellCombo AssignedCombo;     // Cached combo assigned to this card
    [Header("Spell Tier UI")]
    public TextMeshProUGUI spellTierText;

    // Cached CardIconManager instance
    private static CardIconManager iconManager;

    public List<ProcessingVisualOutput> visualOutputs;

    [Header("Borders")]
    public Image typeIconBorder;
    public Image processedIconBorder;    // Slightly larger white border
    public Image partIconBorder;
    //public Image quantityIconBorder;

    [Header("Border Settings")]
    public bool showBorders = true;      // Toggle borders on/off in inspector
    public float borderScale = 1f;       // Scale multiplier for the border

    public string runtimeID;
    public string RuntimeID => runtimeID;

    private void Awake()
    {
        if (iconManager == null)
        {
            iconManager = Resources.Load<CardIconManager>("CardIconManager");
            if (iconManager == null)
                Debug.LogError("CardIconManager asset not found in Resources!");
        }

        if (string.IsNullOrEmpty(runtimeID))
        {
            runtimeID = System.Guid.NewGuid().ToString();
        }
    }

    //void OnEnable()
    //{
    //    Debug.Log("CARD SPAWNED UNDER → " + transform.parent.name, transform.parent);
    //}

    private bool isProcessedVisually = false;

    public void SetCardData(CardData newData, bool forceShowProcessed = false)
    {
        isProcessedVisually = false;
        
        if (newData == null) return;

        cardData = newData;

        // Main icon
        if (iconImage != null)
            iconImage.sprite = cardData.Icon;

        // Background color
        if (backgroundImage != null)
            backgroundImage.color = cardData.cardColor;

        // Name
        if (nameText != null)
            nameText.text = cardData.cardName;

        // Type icon + border (fetch from CardIconManager at runtime)
        if (typeIconImage != null && iconManager != null)
        {
            Sprite typeSprite = iconManager.GetIconForType(cardData.itemType);
            typeIconImage.sprite = typeSprite;
            typeIconImage.gameObject.SetActive(typeSprite != null);

            if (typeIconBorder != null)
            {
                typeIconBorder.gameObject.SetActive(typeSprite != null && showBorders);
                typeIconBorder.rectTransform.localScale = Vector3.one * borderScale;
            }
        }

        // Processed icon + border
        if (processedIconImage != null && iconManager != null)
        {
            bool showProcessed =
                cardData.processedType != ProcessedType.None &&
                (forceShowProcessed || isProcessedVisually);
            Sprite processedSprite = showProcessed ? iconManager.GetIconForProcessed(cardData.processedType) : null;

            processedIconImage.sprite = processedSprite;
            processedIconImage.gameObject.SetActive(processedSprite != null);

            if (processedIconBorder != null)
            {
                bool showBorder = processedSprite != null && showBorders;
                processedIconBorder.gameObject.SetActive(showBorder);
                processedIconBorder.rectTransform.localScale = Vector3.one * borderScale;
            }
        }

        // Top-left: part icon for STARTING cards
        if (outputPartIcon != null && iconManager != null)
        {
            Sprite partSprite = iconManager.GetIconForPart(cardData.partType);
            outputPartIcon.sprite = partSprite;
            outputPartIcon.gameObject.SetActive(partSprite != null);

            if (partIconBorder != null)
            {
                bool showBorder = partSprite != null && showBorders;
                partIconBorder.gameObject.SetActive(showBorder);
                partIconBorder.rectTransform.localScale = Vector3.one * borderScale;
            }
        }

        if (spellTierText != null)
        {
            if (AssignedCombo != null)
            {
                spellTierText.text = AssignedCombo.SpellLevel.ToString();
                spellTierText.gameObject.SetActive(true);
            }
            else
            {
                spellTierText.gameObject.SetActive(false);
            }
        }
    }
        public void SetQuantityNumber(int count)
        {
        if (stackCountText == null) return;

        if (count <= 1)
        {
            stackCountText.text = "";
            stackCountText.enabled = false;
        }
        else
        {
            stackCountText.text = count.ToString();
            stackCountText.enabled = true;
        }
    

        // Show visual outputs if any are assigned
        if (visualOutputs != null && visualOutputs.Count > 0)
        {
            SetVisualOutputCard(visualOutputs[0]);
        }
        else if (outputIconsParent != null)
        {
            // Clear previous icons if none
            foreach (Transform child in outputIconsParent)
                Destroy(child.gameObject);
            outputIconsParent.gameObject.SetActive(false);
        }
    }

    public void MarkAsProcessed()
    {
        // Existing: apply processed icon
        if (CardData != null)
        {
            CardData.ApplyProcessedIcon();

            // 🔥 Append processed type to card name (if not None)
            if (CardData.processedType != ProcessedType.None)
            {
                string suffix = CardData.processedType.ToString();   // e.g. "Chopped", "Powder", etc.

                if (!CardData.cardName.EndsWith(" " + suffix))
                    CardData.cardName = CardData.cardName + " " + suffix;
            }
        }

        // Refresh visuals
        SetCardData(CardData, forceShowProcessed: true);
    }

    public void SetVisualOutputCard(ProcessingVisualOutput visualOutput)
    {
        if (visualOutput == null)
        {
            Debug.LogWarning("VisualOutput is null!");
            return;
        }

        // Main icon remains the original card icon
        if (iconImage != null)
            iconImage.sprite = cardData.Icon;

        // Background color
        if (backgroundImage != null)
            backgroundImage.color = cardData.cardColor;

        // Name
        if (nameText != null)
            nameText.text = $"{cardData.cardName} ({visualOutput.name})";

        // Type icon remains
        if (typeIconImage != null && iconManager != null)
        {
            typeIconImage.sprite = iconManager.GetIconForType(cardData.itemType);
            typeIconImage.gameObject.SetActive(typeIconImage.sprite != null);
        }

        // Hide processed icon (bottom-right)
        if (processedIconImage != null)
            processedIconImage.gameObject.SetActive(false);
        if (processedIconBorder != null)
            processedIconBorder.gameObject.SetActive(false);

        // Top-left: part icon
        if (outputPartIcon != null)
        {
            outputPartIcon.sprite = visualOutput.icon;

            bool showPart = (visualOutput.icon != null);
            outputPartIcon.gameObject.SetActive(showPart);

            if (partIconBorder != null)
                partIconBorder.gameObject.SetActive(showPart);
        }

    }
}
