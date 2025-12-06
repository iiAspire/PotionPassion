using UnityEngine;

public class CardCombiner : MonoBehaviour
{
    [Header("Card Halves")]
    public SpriteRenderer topHalfRenderer;
    public SpriteRenderer bottomHalfRenderer;

    [Header("Optional")]
    public SpriteRenderer frameRenderer; // Your border frame, if needed

    /// <summary>
    /// Call this to assign the two card sprites to the prefab.
    /// </summary>
    public void SetCardSprites(Sprite topHalf, Sprite bottomHalf)
    {
        if (topHalfRenderer != null)
            topHalfRenderer.sprite = topHalf;

        if (bottomHalfRenderer != null)
            bottomHalfRenderer.sprite = bottomHalf;
    }
}