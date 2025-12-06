using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DryingRackSlot
{
    public CardComponent card;       // Assigned card
    public Slider timerSlider;
    public Image iconImage;          // Icon representing the card
    public float totalTime;          // Total processing time
    public GameObject timerFrame;
    public ProcessingRecipe recipe;
    [HideInInspector] public float elapsedTime;
    [HideInInspector] public bool active;
}