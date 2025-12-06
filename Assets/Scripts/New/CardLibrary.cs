using UnityEngine;

[CreateAssetMenu(fileName = "CardLibrary", menuName = "Card System/Card Library")]
public class CardLibrary : ScriptableObject
{
    public CardDataSO[] blackCards;
    public CardDataSO[] copperCards;
    public CardDataSO[] silverCards;
    public CardDataSO[] goldCards;
}