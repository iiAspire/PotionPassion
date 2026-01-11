using UnityEngine;

public class PersistentUIManager : MonoBehaviour
{
    public static PersistentUIManager Instance;

    public Canvas persistentCanvas;
    public FailedBrewPanelController failedBrewPanel;
    public SuccessfulBrewPanelController successfulBrewPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Wrapper method for easy access
    public void ShowFailedBrews()
    {
        if (failedBrewPanel != null)
            failedBrewPanel.ShowFailedBrews();
    }

    public void ShowSuccessfulBrews()
    {
        if (successfulBrewPanel != null)
            successfulBrewPanel.ShowSuccessfulBrews();
    }
}