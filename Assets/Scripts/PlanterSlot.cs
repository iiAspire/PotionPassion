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

    [Header("Inventory")]
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

    [Header("Output Spawn")]
    public Transform outputAnchor;  // An empty object above the planter
    public Transform ingredientInventoryParent;  // where to send the item if a new seed is planted

    public void PlantSeed(CardComponent seed)
    {
        Debug.Log($"{name} planted seed {plantedSeedName}");

        if (isGrowing)
            return;

        plantedSeedName = seed.CardData.cardName;
        seed.gameObject.SetActive(false);

        currentEntry = growthDatabase.GetEntry(seed.CardData.cardName);
        if (currentEntry == null)
            return;

        elapsedMinutes = 0f;
        isGrowing = true;
        isReadyToHarvest = false;

        // visuals on
        growthImage.gameObject.SetActive(true);
        radialTimer.gameObject.SetActive(true);

        // label
        seedLabelParent.SetActive(true);
        seedLabelSprite.sprite = currentEntry.grownPlant.Icon;
    }

    public void TickByMinutes(float minutes)
    {
        //Debug.Log($"{name} TickByMinutes: {minutes}");

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

            Harvest(); // ✅ AUTO harvest, global-time based
        }
    }

    public void RestoreFromSave(string seedName, float remainingTime)
    {
        Debug.Log($"{name} restored: growing={isGrowing}");

        // 1️⃣ Remember the planted seed (NO CardComponent exists)
        plantedSeedName = seedName;

        // 2️⃣ Rebuild growth entry
        currentEntry = growthDatabase.GetEntry(seedName);
        if (currentEntry == null)
            return;

        // 3️⃣ Restore elapsed time
        elapsedMinutes = currentEntry.growTime - remainingTime;
        elapsedMinutes = Mathf.Clamp(elapsedMinutes, 0, currentEntry.growTime);

        // 4️⃣ Mark growing state
        isGrowing = true;
        isReadyToHarvest = false;

        // 5️⃣ Restore visuals
        growthImage.gameObject.SetActive(true);
        radialTimer.gameObject.SetActive(true);

        seedLabelParent.SetActive(true);
        seedLabelSprite.sprite = currentEntry.grownPlant.Icon;

        float t = Mathf.Clamp01(elapsedMinutes / currentEntry.growTime);
        radialTimer.fillAmount = 1f - t;

        // 6️⃣ Force UI refresh
        TickByMinutes(0f);
    }

    public void Harvest()
    {
        if (!isReadyToHarvest || currentEntry == null)
            return;

        // remove seed
        if (plantedSeed != null)
            Destroy(plantedSeed.gameObject);

        plantedSeed = null;

        // hide visuals
        radialTimer.gameObject.SetActive(false);
        seedLabelParent.SetActive(false);

        if (growthImage != null)
        {
            growthImage.sprite = null;
            growthImage.gameObject.SetActive(false);
        }

        // spawn output
        for (int i = 0; i < currentEntry.outputQuantity; i++)
        {
            GameObject cardGO =
                Instantiate(currentEntry.grownPlant.cardPrefab, outputAnchor);

            cardGO.transform.localPosition = Vector3.zero;

            CardComponent card = cardGO.GetComponent<CardComponent>();
            CardData runtimeCopy =
                ScriptableObject.Instantiate(currentEntry.grownPlant);

            card.SetCardData(runtimeCopy, true);
        }

        // reset
        currentEntry = null;
        isReadyToHarvest = false;
    }
}