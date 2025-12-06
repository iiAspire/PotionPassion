using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [Header("Setup")]
    public CardFabricator cardPrefab;        // Your ItemCard prefab or scene object
    public CardDataSO[] cardsToSpawn;        // Assign a few CardDataSO assets here
    public Transform spawnParent;            // Where to place them (e.g. a Canvas or panel)
    public Vector2 spacing = new Vector2(200, 0); // How far apart to space them

    private void Start()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardSpawner: No cardPrefab assigned!");
            return;
        }

        if (cardsToSpawn == null || cardsToSpawn.Length == 0)
        {
            Debug.LogWarning("CardSpawner: No cardsToSpawn assigned — nothing to show.");
            return;
        }

        SpawnCards();
    }

    public void SpawnCards()
    {
        for (int i = 0; i < cardsToSpawn.Length; i++)
        {
            var data = cardsToSpawn[i];
            if (data == null) continue;

            // Instantiate
            var card = Instantiate(cardPrefab, spawnParent);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchoredPosition = new Vector2(i * spacing.x, 0); // horizontal layout
            rt.localRotation = Quaternion.identity;

            // Update visuals
            card.UpdateCard(data);
        }
    }
}