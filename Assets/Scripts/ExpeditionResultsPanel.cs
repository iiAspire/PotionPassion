using UnityEngine;
using TMPro;

public class ExpeditionResultsPanel : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text regionText;
    [SerializeField] TMP_Text travelText;
    [SerializeField] TMP_Text totalTravelText;
    [SerializeField] TMP_Text summaryText;
    [SerializeField] TMP_Text findsHeaderText;

    [Header("List")]
    [SerializeField] Transform listParent;
    [SerializeField] ExpeditionResultItemUI rowPrefab;

    private ExpeditionResult result;

    private void Start()
    {

    }

    public void Render(ExpeditionResult r)
    {
        result = r;

        if (r == null)
        {
            if (headerText) headerText.text = "RESULTS";
            if (summaryText) summaryText.text = "No expedition data.";
            ClearList();
            return;
        }

        if (headerText) headerText.text = "EXPEDITION RESULTS";

        if (regionText)
            regionText.text = r.region
                ? $"{r.region.locationType} — {r.region.locationName}"
                : "Unknown region";

        if (travelText)
        {
            float travelRoundTrip = r.travelTime * 2f;
            float harvest = r.harvestTimeTaken;
            float total = r.totalTimeTaken; // already computed by simulator

            travelText.text =
                $"To destination: {r.travelTime:0} min\n" +
                $"Harvesting: {r.harvestTimeTaken:0} min\n" +
                $"Return trip: {r.returnTime:0} min\n";
        }

        if (totalTravelText)
        {
            float travelRoundTrip = r.travelTime * 2f;
            float harvest = r.harvestTimeTaken;
            float total = r.totalTimeTaken; // already computed by simulator

            totalTravelText.text =
                $"Total time: {r.totalTimeTaken:0} min";
        }

        if (summaryText)
            summaryText.text = r.summary;

        if (findsHeaderText) findsHeaderText.text = "YOU COLLECTED:";

        PopulateList(r);
    }

    private void PopulateList(ExpeditionResult r)
    {
        ClearList();

        if (r.gains == null || r.gains.Count == 0)
            return;

        foreach (var g in r.gains)
        {
            var row = Instantiate(rowPrefab, listParent);
            row.Setup(g);
        }
    }

    private void ClearList()
    {
        if (!listParent) return;

        for (int i = listParent.childCount - 1; i >= 0; i--)
            Destroy(listParent.GetChild(i).gameObject);
    }

    // Hook to your "Take All / Continue" button
    public void OnCollect()
    {
        if (result == null) return;

        var pm = CardPersistenceManager.Instance;
        if (pm == null)
        {
            Debug.LogError("No CardPersistenceManager.Instance found.");
            return;
        }

        int totalCards = 0;

        foreach (var g in result.gains)
        {
            // Add to actual player inventory (spawns runtime cards + saves)
            pm.GrantToPlayerInventory(g.card, g.amount);
            totalCards += g.amount;
        }

        // Add time taken to game clock
        float hours = result.totalTimeTaken / 60f;
        TimeManager.Instance.AddGameHours(hours);

        // Show message
        if (CardClickLog.Instance != null)
            CardClickLog.Instance.Log($"{totalCards} item{(totalCards == 1 ? "" : "s")} added to your inventory.");


        ExpeditionState.LastResult = null;

        // Close panel or change scene:
        SceneLoadManager.Instance.LoadRoom("ShopScene");
    }
}