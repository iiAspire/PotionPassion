using UnityEngine;

public class ShowSuccessfulBrewsButton : MonoBehaviour
{
    public void OnClick()
    {
        if (PersistentUIManager.Instance != null)
            PersistentUIManager.Instance.ShowSuccessfulBrews();
    }
}