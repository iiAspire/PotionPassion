using UnityEngine;

public class LoadoutPanelController : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] GameObject panelRoot;

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
        // Later: validate loadout + store in GameState
        Debug.Log("Loadout confirmed");

        // TODO: Load exploration scene or start forage mode
        // SceneLoadManager.Instance.LoadRoom("ForageScene");
    }

    public void OnBackToShop()
    {
        SceneLoadManager.Instance.LoadRoom("ShopScene");
    }
}