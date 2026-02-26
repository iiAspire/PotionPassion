using UnityEngine;

public class LoadoutPanelController : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] GameObject panelRoot;
    [SerializeField] LoadoutScreen loadoutScreen;

    // =========================================================
    // PANEL VISIBILITY
    // =========================================================

    public void Show()
    {
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    // =========================================================
    // BUTTON ACTIONS
    // =========================================================

    public void OnConfirm()
    {
        if (loadoutScreen.selectedRegion == null)
        {
            Debug.LogWarning("No destination selected.");
            return;
        }

        if (loadoutScreen.selectedCarry == null)
        {
            Debug.LogWarning("No carry item selected.");
            return;
        }

        if (!loadoutScreen.CanReachSelectedRegion)
        {
            Debug.Log("Destination out of range.");
            return;
        }

        Debug.Log("Expedition confirmed.");

        GameState.TargetRegion = loadoutScreen.selectedRegion;

        // SceneLoadManager.Instance.LoadRoom("ForageScene");
    }

    public void OnBackToShop()
    {
        SceneLoadManager.Instance.LoadRoom("ShopScene");
    }
}