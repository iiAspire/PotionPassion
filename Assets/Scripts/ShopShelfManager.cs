using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopShelfManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform saleInventoryRoot;
    [SerializeField] private Transform shelvesRoot;
    private List<Transform> shelfGridRoots = new();

    [SerializeField] private GameObject shelfItemPrefab;

    private void Start()
    {
        StartCoroutine(WaitAndPopulate());
    }

    private void CacheShelfGrids()
    {
        shelfGridRoots.Clear();

        foreach (Transform shelf in shelvesRoot)
        {
            // each shelf has ONE child: the ItemsGrid
            if (shelf.childCount == 0)
            {
                Debug.LogError($"Shelf {shelf.name} has no ItemsGrid child");
                continue;
            }

            Transform itemsGrid = shelf.GetChild(0);
            shelfGridRoots.Add(itemsGrid);
        }

        Debug.Log($"[SHOP] Cached {shelfGridRoots.Count} shelf item grids");
    }

    private IEnumerator WaitAndPopulate()
    {
        while (!CardPersistenceManager.Instance.CardsRestored)
            yield return null;

        CacheShelfGrids();     // ← THIS IS THE FIX
        PopulateShelves();
    }

    public void PopulateShelves()
    {
        Debug.Log(
            $"[SHOP] SaleInventory children = {saleInventoryRoot.childCount}",
            saleInventoryRoot
        );

        ClearShelves();

        if (shelfGridRoots == null || shelfGridRoots.Count == 0)
        {
            Debug.LogWarning("No shelf grids assigned.");
            return;
        }

        int shelfIndex = 0;

        foreach (Transform child in saleInventoryRoot)
        {
            CardComponent card = child.GetComponent<CardComponent>();
            if (card == null)
                continue;

            if (card.CardData == null)
                continue;

            Debug.Log($"[SHOP] Attempting to display card: {card.CardData.cardName}");

            Transform targetShelf = shelfGridRoots[shelfIndex];

            GameObject viewGO = Instantiate(shelfItemPrefab, targetShelf, false);

            RectTransform rt = viewGO.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            ShelfItemView view = viewGO.GetComponent<ShelfItemView>();
            view.Bind(card);

            // Move to next shelf (round-robin)
            shelfIndex = (shelfIndex + 1) % shelfGridRoots.Count;
        }
    }

    public void ClearShelves()
    {
        foreach (Transform shelf in shelfGridRoots)
        {
            for (int i = shelf.childCount - 1; i >= 0; i--)
            {
                Destroy(shelf.GetChild(i).gameObject);
            }
        }
    }
}