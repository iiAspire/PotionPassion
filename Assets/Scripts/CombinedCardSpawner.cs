using UnityEngine;

public class CombinedCardSpawner : MonoBehaviour
{
    public CardManager cardManager;
    //public GameObject Mix2;

    ///// <summary>
    ///// Creates a combined card from two card names.
    ///// </summary>
    //public GameObject SpawnCombinedCard(CardData topCard, CardData bottomCard, Vector3 position)
    //{

    //    if (topCard == null || bottomCard == null)
    //    {
    //        Debug.LogError("Missing card data for combination!");
    //        return null;
    //    }

    //    GameObject combinedCardObj = Instantiate(Mix2, position, Quaternion.identity);
    //    CardCombiner combiner = combinedCardObj.GetComponent<CardCombiner>();

    //    if (combiner != null)
    //    {
    //        combiner.SetCardSprites(topCard.topHalfSprite, bottomCard.bottomHalfSprite);
    //    }
    //    else
    //    {
    //        Debug.LogError("CombinedCard prefab is missing a CardCombiner script.");
    //    }

    //    return combinedCardObj;
    //}
}