using UnityEngine;

[System.Serializable]
public class ManualProcessState
{
    public string cardRuntimeID;
    public ProcessingTool tool;
    public float remainingTime;
    public ProcessingRecipe recipe;
}