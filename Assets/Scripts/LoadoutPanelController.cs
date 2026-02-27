using UnityEngine;

public class LoadoutPanelController : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] GameObject panelRoot;
    [SerializeField] LoadoutScreen loadoutScreen;

    [Header("Results Root")]
    [SerializeField] private GameObject resultsPanelRoot;
    [SerializeField] private ExpeditionResultsPanel resultsPanel;

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
        if (loadoutScreen == null)
        {
            Debug.LogError("LoadoutPanelController: loadoutScreen not assigned.");
            return;
        }

        if (loadoutScreen.selectedRegion == null)
        {
            Debug.LogWarning("No destination selected.");
            return;
        }

        if (loadoutScreen.selectedCarry == null)
        {
            Debug.LogWarning("No carry selected.");
            return;
        }

        if (!loadoutScreen.CanConfirmExpedition)
        {
            CardClickLog.Instance?.Log(loadoutScreen.ConfirmBlockReason);
            return;
        }

        float t = loadoutScreen.TravelTimeToSelectedRegion;
        float max = loadoutScreen.MaxTravelTime;

        if (float.IsInfinity(t))
        {
            Debug.LogWarning("Destination not reachable from current location (switch map or move first).");
            return;
        }

        if (t > max)
        {
            Debug.LogWarning(
                $"Destination too far. Travel time {t:0} min exceeds your limit {max:0} min.");
            return;
        }

        // ✅ Passed travel validation
        Debug.Log("Expedition confirmed.");

        // ✅ Generate the expedition outcome
        var result = ExpeditionSimulator.Run(loadoutScreen);

        // ✅ Show results panel in THIS scene
        if (resultsPanelRoot) resultsPanelRoot.SetActive(true);
        if (resultsPanel) resultsPanel.Render(result);

        // Optional: hide the loadout panel now
        if (panelRoot) panelRoot.SetActive(false);
    }

    public void OnBackToShop()
    {
        SceneLoadManager.Instance.LoadRoom("ShopScene");
    }
}