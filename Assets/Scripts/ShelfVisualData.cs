using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shelf Visual Data")]
public class ShelfVisualData : ScriptableObject
{
    public Sprite containerSprite;
    public Sprite contentsSprite;
    public Color contentsColor = Color.white;
}