using UnityEngine;

public class ShowFailedBrewsButton : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("ShowFailedBrewsButton.OnClick called!");

        if (PersistentUIManager.Instance == null)
        {
            Debug.LogError("PersistentUIManager.Instance is NULL!");
            return;
        }

        Debug.Log("PersistentUIManager found, calling ShowFailedBrews...");
        PersistentUIManager.Instance.ShowFailedBrews();
    }
}