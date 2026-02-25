using UnityEngine;
using UnityEngine.UI;

public class LoadoutCapacityDisplay : MonoBehaviour
{
    [SerializeField] LoadoutScreen loadoutScreen;

    [Header("UI")]
    [SerializeField] Image[] slotImages;   // carry slots
    [SerializeField] Image handImage;      // optional hand slot
    [SerializeField] Sprite handPlaceholderSprite;
    [SerializeField] Sprite emptySlotSprite;

    void OnEnable()
    {
        loadoutScreen.OnLoadoutChanged += Refresh;
    }

    void OnDisable()
    {
        loadoutScreen.OnLoadoutChanged -= Refresh;
    }

    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        var carry = loadoutScreen.selectedCarry;

        if (carry == null)
        {
            SetAllInactive();
            return;
        }

        int capacity = carry.slotCapacity;

        // --- HAND SLOT ---
        if (handImage)
        {
            if (!carry.handsFree)
            {
                handImage.gameObject.SetActive(false);
            }
            else
            {
                handImage.gameObject.SetActive(true);

                var handTool = loadoutScreen.HandTool;

                if (handTool != null)
                {
                    handImage.sprite = handTool.icon;
                    handImage.color = Color.white;
                }
                else
                {
                    handImage.sprite = handPlaceholderSprite; // assign in inspector
                    handImage.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }

        // --- CAPACITY SLOTS ---
        var tools = loadoutScreen.SlotTools;

        for (int i = 0; i < slotImages.Length; i++)
        {
            bool exists = i < capacity;

            slotImages[i].gameObject.SetActive(exists);

            if (!exists) continue;

            if (i < tools.Count)
            {
                // Filled slot → show tool icon
                slotImages[i].sprite = tools[i].icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                // Empty slot → show placeholder
                slotImages[i].sprite = emptySlotSprite; // assign in inspector
                slotImages[i].color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

    void SetAllInactive()
    {
        foreach (var img in slotImages)
            img.gameObject.SetActive(false);

        if (handImage)
            handImage.gameObject.SetActive(false);
    }
}