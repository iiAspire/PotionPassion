using System.Collections.Generic;
using UnityEngine;

public class StoreroomDisplayController : MonoBehaviour
{
    [Header("Shelves")]
    [SerializeField] private List<Transform> shelves;

    [Header("Jar Prefab")]
    [SerializeField] private ShelfItemView shelfItemPrefab;

    private int shelfIndex = 0;

    void Start()
    {
        Debug.Log("Storeroom controller running");
        PopulateShelves();
    }

    void PopulateShelves()
    {
        foreach (var shelf in shelves)
        {
            foreach (Transform child in shelf)
                Destroy(child.gameObject);
        }

        List<CardComponent> cards = GetDisplayCards();

        // Stored cards first, in saved positions
        var storedCards = cards.FindAll(c => c.storedInStoreroom);
        storedCards.Sort((a, b) =>
        {
            int shelfCompare = a.storeroomShelfIndex.CompareTo(b.storeroomShelfIndex);
            if (shelfCompare != 0) return shelfCompare;
            return a.storeroomOrderInShelf.CompareTo(b.storeroomOrderInShelf);
        });

        foreach (var card in storedCards)
        {
            int shelfIndex = Mathf.Clamp(card.storeroomShelfIndex, 0, shelves.Count - 1);
            Transform shelf = shelves[shelfIndex];

            ShelfItemView item = Instantiate(shelfItemPrefab, shelf);
            item.Bind(card, this);

            int sibling = Mathf.Clamp(card.storeroomOrderInShelf, 0, shelf.childCount - 1);
            item.transform.SetSiblingIndex(sibling);
        }

        // Then unstored cards fill remaining shelves in order
        var unstoredCards = cards.FindAll(c => !c.storedInStoreroom);

        int currentShelf = 0;
        while (currentShelf < shelves.Count && shelves[currentShelf].childCount >= 4)
            currentShelf++;

        foreach (var card in unstoredCards)
        {
            while (currentShelf < shelves.Count && shelves[currentShelf].childCount >= 4)
                currentShelf++;

            if (currentShelf >= shelves.Count)
                break;

            Transform shelf = shelves[currentShelf];

            ShelfItemView item = Instantiate(shelfItemPrefab, shelf);
            item.Bind(card, this);
        }
    }

    List<CardComponent> GetDisplayCards()
    {
        List<CardComponent> cards = new List<CardComponent>();

        var persistence = CardPersistenceManager.Instance;
        if (persistence == null)
        {
            Debug.LogError("CardPersistenceManager not found");
            return cards;
        }

        Transform playerParent = persistence.playerInventoryParent;
        Transform ingredientParent = persistence.ingredientsInventoryParent;
        Transform storeroomParent = persistence.storeroomParent;

        if (playerParent)
            AddShelfItems(playerParent.GetComponentsInChildren<CardComponent>(true), cards);

        if (ingredientParent)
            AddShelfItems(ingredientParent.GetComponentsInChildren<CardComponent>(true), cards);

        if (storeroomParent)
            AddShelfItems(storeroomParent.GetComponentsInChildren<CardComponent>(true), cards);

        Debug.Log("Cards to display: " + cards.Count);

        return cards;
    }

    public List<Transform> GetShelves()
    {
        return shelves;
    }

    public int GetShelfIndex(Transform shelf)
    {
        return shelves.IndexOf(shelf);
    }

    void AddShelfItems(CardComponent[] source, List<CardComponent> result)
    {
        if (source == null)
            return;

        foreach (var card in source)
        {
            if (card == null)
                continue;

            if (card.CardData == null)
                continue;

            if (card.CardData.shelfVisuals == null)
                continue;

            result.Add(card);
        }
    }
}