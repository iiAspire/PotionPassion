using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<CardComponent> cards = new List<CardComponent>();
    public string stackName;

    [Header("Fan Settings")]
    public float fanOffsetAngle = 12f;   // degrees between cards
    public float fanRadius = 10f;        // how far behind the top card lower ones sit
    public int maxFannedCards = 3;       // how many lower cards to show fanned

    public CardComponent TopCard => cards.Count > 0 ? cards[cards.Count - 1] : null;

    public void Initialize(CardComponent card)
    {
        cards.Clear();
        cards.Add(card);
        stackName = card.CardData.cardName;

        card.transform.SetParent(transform, false);
        var rt = card.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        UpdateStackVisuals();
    }

    public void AddCard(CardComponent card)
    {
        if (card == null) return;

        cards.Add(card);

        card.transform.SetParent(transform, false);
        var rt = card.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        UpdateStackVisuals();
    }

    public CardComponent PopCard()
    {
        if (cards.Count == 0) return null;

        CardComponent popped = TopCard;
        cards.RemoveAt(cards.Count - 1);

        if (cards.Count > 0)
        {
            UpdateStackVisuals();
        }
        else
        {
            // destroy the stack container if it's empty
            Destroy(gameObject);
        }

        // Do NOT unparent to null; let drag/drop manage parenting.
        if (popped != null)
        {
            var rt = popped.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
        }

        return popped;
    }

    public void RemoveCard(CardComponent card)
    {
        if (card == null || !cards.Contains(card)) return;

        cards.Remove(card);

        if (cards.Count > 0)
        {
            UpdateStackVisuals();
        }
        else
        {
            Destroy(gameObject);
        }

        // Do not change parent here – CardDragDrop will handle that.
        card.SetQuantityNumber(1);

        var rt = card.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
    }

    public void RefreshStack()
    {
        UpdateStackVisuals();
        UpdateQuantityNumbers();
    }

    private void UpdateStackVisuals()
    {
        if (cards.Count == 0) return;

        // Ensure deterministic sibling order: older at bottom, newer on top
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
                cards[i].transform.SetSiblingIndex(i);
        }

        // --- TOP CARD: always upright, centered ---
        CardComponent top = TopCard;
        RectTransform topRT = top.GetComponent<RectTransform>();
        topRT.localPosition = Vector3.zero;
        topRT.localRotation = Quaternion.identity;
        topRT.localScale = Vector3.one;
        topRT.SetAsLastSibling();

        // If only one card, no fan effect at all
        if (cards.Count == 1)
        {
            UpdateQuantityNumbers();
            return;
        }

        // --- LOWER CARDS: fanned under the top ---
        int lowerCount = cards.Count - 1;
        int visibleLower = Mathf.Min(lowerCount, maxFannedCards);

        float totalFan = (visibleLower - 1) * fanOffsetAngle;
        float startAngle = -totalFan * 0.5f;

        for (int i = 0; i < lowerCount; i++)
        {
            CardComponent c = cards[i];
            if (c == null) continue;

            RectTransform rt = c.GetComponent<RectTransform>();

            if (i < visibleLower)
            {
                float angle = startAngle + fanOffsetAngle * i;

                rt.localRotation = Quaternion.Euler(0f, 0f, angle);

                // slight offset behind the top card
                Vector3 offset = Quaternion.Euler(0f, 0f, angle) * new Vector3(0f, -fanRadius, 0f);
                rt.localPosition = offset;
            }
            else
            {
                // cards beyond visible limit sit under the stack, reset their transform
                rt.localRotation = Quaternion.identity;
                rt.localPosition = Vector3.zero;
            }

            rt.localScale = Vector3.one;
            rt.SetSiblingIndex(i);
        }

        UpdateQuantityNumbers();
    }

    private void UpdateQuantityNumbers()
    {
        int count = cards.Count;
        foreach (var c in cards)
        {
            if (c != null)
                c.SetQuantityNumber(count);
        }
    }
}