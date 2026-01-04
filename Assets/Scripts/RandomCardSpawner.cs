using UnityEngine;
using System.Collections;

public class RandomCardSpawner : MonoBehaviour
{
    [Header("References")]
    public CardManager cardManager;
    public CardPersistenceManager persistence;
    public Transform leftInventory;  // this should be the same as playerInventoryParent

    [Header("Random spawn")]
    public int numberOfCardsToSpawn = 5;
    public bool spawnTestCards = true;

    IEnumerator Start()
    {
        // Wait one frame so GameData + Persistence are alive
        yield return null;

        if (GameData.Instance == null)
            yield break;

        // If we already have saved cards, DO NOTHING.
        // CardPersistenceManager handles loading.
        if (GameData.Instance.savedCards.Count > 0)
        {
            //Debug.Log("RandomCardSpawner: Saved cards exist → skipping all spawns.");
            yield break;
        }

        if (spawnTestCards && !GameData.Instance.testCardsSpawned)
        {
            GameData.Instance.testCardsSpawned = true;
            yield return StartCoroutine(SpawnTestCards());
        }

        if (!GameData.Instance.initialRandomCardsSpawned)
        {
            GameData.Instance.initialRandomCardsSpawned = true;
            yield return StartCoroutine(SpawnInitialRandomCards());
        }

    }

    private IEnumerator SpawnTestCards()
    {
        yield return null;

        string[] testCards = { "Sandstone", "Chia", "Corn", "Chia Seed" };

        foreach (string cardName in testCards)
        {
            CardData template = cardManager.GetCardByName(cardName);
            if (template == null || template.cardPrefab == null)
                continue;

            SpawnRuntimeCard(template, leftInventory);
            yield return null;
        }
    }

    private IEnumerator SpawnInitialRandomCards()
    {
        for (int i = 0; i < numberOfCardsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, cardManager.allCards.Length);
            CardData template = cardManager.allCards[randomIndex];

            if (template != null && template.cardPrefab != null)
            {
                SpawnRuntimeCard(template, leftInventory);
            }

            yield return null;
        }
    }

    private void SpawnRuntimeCard(CardData template, Transform parent)
    {
        CardData runtime = ScriptableObject.CreateInstance<CardData>();
        runtime.CopyFrom(template);
        runtime.cardName = template.cardName;
        runtime.baseName = template.baseName;

        runtime.ApplyDefaultColor();
        runtime.processedType = ProcessedType.None;
        runtime.processedIcon = null;
        runtime.ApplyPartIcon();
        runtime.ApplyQuantityIcon();

        GameObject cardObj = Instantiate(template.cardPrefab, parent);
        CardComponent comp = cardObj.GetComponent<CardComponent>();
        if (comp != null)
        {
            if (string.IsNullOrEmpty(comp.runtimeID))
                comp.runtimeID = System.Guid.NewGuid().ToString();

            comp.SetCardData(runtime);
        }
    }
}