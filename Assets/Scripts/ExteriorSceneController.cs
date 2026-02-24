using UnityEngine;

public class ExteriorSceneController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject preparationPanel;
    [SerializeField] GameObject selectionPanel;
    [SerializeField] GameObject resultsPanel;

    bool outingCommitted = false;

    // =========================================================
    // INITIAL STATE
    // =========================================================

    void Start()
    {
        ShowPreparation();
    }

    // =========================================================
    // PANEL SWITCHING
    // =========================================================

    void ShowPreparation()
    {
        preparationPanel.SetActive(true);
        selectionPanel.SetActive(false);
        resultsPanel.SetActive(false);
    }

    void ShowSelection()
    {
        preparationPanel.SetActive(false);
        selectionPanel.SetActive(true);
        resultsPanel.SetActive(false);
    }

    void ShowResults()
    {
        preparationPanel.SetActive(false);
        selectionPanel.SetActive(false);
        resultsPanel.SetActive(true);
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    // Tier 1 confirm → generate opportunities
    public void OnConfirmPreparation()
    {
        GenerateAvailableResources();
        ShowSelection();
    }

    // Tier 2 confirm → resolve outcome
    public void OnConfirmSelection()
    {
        outingCommitted = true;

        ResolveHarvest();
        ShowResults();
    }

    public void OnBackToShop()
    {
        if (outingCommitted)
        {
            AdvanceTime();
            ApplyWorldChanges();
        }

        SceneLoadManager.Instance.LoadRoom("ShopScene");
    }

    // =========================================================
    // CORE LOGIC (STUBS)
    // =========================================================

    void GenerateAvailableResources()
    {
        Debug.Log("Generating harvest options");
        // TODO:
        // - Read loadout
        // - Read location
        // - Build list of resource cards
    }

    void ResolveHarvest()
    {
        Debug.Log("Resolving selected harvest");
        // TODO:
        // - Apply chosen items
        // - Add to inventory
        // - Deplete nodes
    }

    void AdvanceTime()
    {
        Debug.Log("Time passes");
    }

    void ApplyWorldChanges()
    {
        Debug.Log("Plants grow, shop updates, etc.");
    }
}