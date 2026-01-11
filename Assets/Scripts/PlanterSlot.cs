using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PlanterSlot : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Unique ID for this planter. Must be stable across scenes.")]
    public string planterID;

    [Header("Database")]
    public PlantGrowthDatabase growthDatabase;

    [Header("UI")]
    public Image planterBaseImage;
    public Image radialTimer;
    public Image growthImage;
    public GameObject seedLabelParent;
    public Image seedLabelSprite;

    [Header("Output Spawn")]
    [Tooltip("Where harvested cards appear on THIS planter")]
    public Transform outputAnchor;

    [Header("Inventory")]
    [Tooltip("Where invalid seeds get returned to")]
    public Transform playerInventoryParent;

    private PlantGrowthEntry currentEntry;
    private CardComponent plantedSeed;
    private bool isGrowing = false;
    private bool isReadyToHarvest = false;
    private float elapsedMinutes = 0f;

    public bool IsActive => isGrowing;
    public string CurrentSeedName => plantedSeedName;
    private string plantedSeedName;
    public float RemainingTime =>
        currentEntry != null ? Mathf.Max(0, currentEntry.growTime - elapsedMinutes) : 0f;

    public void PlantSeed(CardComponent seed)
    {
        if (isGrowing)
        {
            Debug.LogWarning($"[{name}] Already growing - cannot plant!");
            return;
        }

        plantedSeedName = seed.CardData.cardName;
        currentEntry = growthDatabase.GetEntry(seed.CardData.cardName);

        // DropZone already validated this, but double-check just in case
        if (currentEntry == null)
        {
            Debug.LogError($"[{name}] ❌ Unexpected: NO GROWTH ENTRY (should have been caught by DropZone)");
            return;
        }

        // Hide the seed card since it's now "planted"
        seed.gameObject.SetActive(false);

        elapsedMinutes = 0f;
        isGrowing = true;
        isReadyToHarvest = false;

        // Show growth visuals
        growthImage.gameObject.SetActive(true);
        radialTimer.gameObject.SetActive(true);

        // Show seed label
        seedLabelParent.SetActive(true);
        seedLabelSprite.sprite = currentEntry.grownPlant.Icon;
    }

    public void TickByMinutes(float minutes)
    {
        if (!isGrowing || currentEntry == null)
            return;

        elapsedMinutes += minutes;

        float t = Mathf.Clamp01(elapsedMinutes / currentEntry.growTime);
        radialTimer.fillAmount = 1f - t;

        // Growth stages
        if (t < 0.2f)
            growthImage.sprite = currentEntry.stage1;
        else if (t < 0.5f)
            growthImage.sprite = currentEntry.stage2;
        else
            growthImage.sprite = currentEntry.stageFinal;

        if (elapsedMinutes >= currentEntry.growTime)
        {
            isGrowing = false;
            isReadyToHarvest = true;
            Harvest(); // Auto harvest when ready
        }
    }

    public void RestoreFromSave(string seedName, float remainingTime)
    {
        plantedSeedName = seedName;

        currentEntry = growthDatabase.GetEntry(seedName);
        if (currentEntry == null)
            return;

        elapsedMinutes = currentEntry.growTime - remainingTime;
        elapsedMinutes = Mathf.Clamp(elapsedMinutes, 0, currentEntry.growTime);

        isGrowing = true;
        isReadyToHarvest = false;

        growthImage.gameObject.SetActive(true);
        radialTimer.gameObject.SetActive(true);

        seedLabelParent.SetActive(true);
        seedLabelSprite.sprite = currentEntry.grownPlant.Icon;

        float t = Mathf.Clamp01(elapsedMinutes / currentEntry.growTime);
        radialTimer.fillAmount = 1f - t;

        TickByMinutes(0f);
        Canvas.ForceUpdateCanvases();
    }

    public void Harvest()
    {
        if (!isReadyToHarvest || currentEntry == null)
            return;

        // Remove planted seed (if it still exists)
        if (plantedSeed != null)
            Destroy(plantedSeed.gameObject);
        plantedSeed = null;

        // Hide growth visuals
        radialTimer.gameObject.SetActive(false);
        seedLabelParent.SetActive(false);
        if (growthImage != null)
        {
            growthImage.sprite = null;
            growthImage.gameObject.SetActive(false);
        }

        // 👇 Spawn harvested cards ON THE PLANTER (outputAnchor)
        for (int i = 0; i < currentEntry.outputQuantity; i++)
        {
            GameObject cardGO = Instantiate(currentEntry.grownPlant.cardPrefab, outputAnchor);
            cardGO.transform.localPosition = Vector3.zero;

            CardComponent card = cardGO.GetComponent<CardComponent>();
            CardData runtimeCopy = ScriptableObject.Instantiate(currentEntry.grownPlant);
            card.SetCardData(runtimeCopy, true);
        }

        // Reset planter state
        currentEntry = null;
        isReadyToHarvest = false;
    }
}