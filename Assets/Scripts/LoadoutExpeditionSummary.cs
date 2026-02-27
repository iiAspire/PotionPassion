using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LoadoutExpeditionSummary : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] LoadoutScreen loadoutScreen;

    [Header("Header")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text findsHeaderText;

    [Header("Destination / Season")]
    [SerializeField] TMP_Text destinationText;
    [SerializeField] TMP_Text seasonText;
    [SerializeField] TMP_Text distanceText;

    [Header("Potential Finds")]
    [SerializeField] Image potentialImage;
    [SerializeField] TMP_Text potentialText;

    [Header("Capacity")]
    [SerializeField] TMP_Text spaceText;

    [Header("Placeholder Sprites")]
    [SerializeField] Sprite defaultPotentialSprite;

    [Header("Controls")]
    [SerializeField] Button confirmButton;

    // ---------------------------------------------------------

    void OnEnable()
    {
        if (loadoutScreen)
            loadoutScreen.OnLoadoutChanged += Refresh;
    }

    void OnDisable()
    {
        if (loadoutScreen)
            loadoutScreen.OnLoadoutChanged -= Refresh;
    }

    void Start()
    {
        Refresh();
    }

    // ---------------------------------------------------------

    void Refresh()
    {
        if (loadoutScreen == null)
            return;

        // --- HEADER ---
        if (headerText)
            headerText.text = "EXPEDITION SUMMARY";

        // --- FINDS HEADER ---
        if (findsHeaderText)
            findsHeaderText.text = "POTENTIAL FINDS";

        // --- DESTINATION ---
        if (loadoutScreen.selectedRegion != null)
            destinationText.text =
                $"Destination: {loadoutScreen.selectedRegion.locationType} — {loadoutScreen.selectedRegion.locationName}";
        else
            destinationText.text = "Destination: Not selected";

        // --- SEASON ---
        if (seasonText)
        {
            if (TimeManager.Instance != null)
                seasonText.text =
                    $"Season: {TimeManager.Instance.Calendar.GetSeasonName()}";
            else
                seasonText.text = "Season: Unknown";
        }

        // --- CONFIRM BUTTON ---
if (confirmButton)
{
    bool can = loadoutScreen.CanConfirmExpedition;

    // keep clickable
    confirmButton.interactable = true;

    // but make it LOOK disabled
    var cg = confirmButton.GetComponent<CanvasGroup>();
    if (cg == null) cg = confirmButton.gameObject.AddComponent<CanvasGroup>();

    cg.alpha = can ? 1f : 0.45f;          // dim when blocked
    cg.blocksRaycasts = true;            // still clickable
    cg.interactable = true;              // still clickable
}

        // --- POTENTIAL FINDS (based on tools) ---
        UpdatePotentialFinds();
        UpdateDistance();

        // --- CARRY CAPACITY ---
        UpdateCapacity();
    }

    // ---------------------------------------------------------
    // POTENTIAL FINDS (simple heuristic for now)
    // ---------------------------------------------------------

    void UpdateDistance()
    {
        if (loadoutScreen.selectedCarry == null)
        {
            distanceText.text = "Travel time: —";
            return;
        }

        float max = loadoutScreen.MaxTravelTime;
        float t = loadoutScreen.TravelTimeToSelectedRegion;

        distanceText.text =
            $"Travel time: {t:0} min / max: {max:0} min";
    }

    void UpdatePotentialFinds()
    {
        var region = loadoutScreen.selectedRegion;

        if (region == null)
        {
            potentialText.text = "Select a destination";
            potentialImage.sprite = defaultPotentialSprite;
            return;
        }

        if (region.possibleFinds == null ||
            region.possibleFinds.Count == 0)
        {
            potentialText.text = "Nothing notable found here";
            potentialImage.sprite = defaultPotentialSprite;
            return;
        }

        System.Text.StringBuilder sb = new();

        foreach (var find in region.possibleFinds)
        {
            if (find.resource == null)
                continue;

            sb.Append("• ");
            sb.Append(find.resource.cardName);

            // Tool requirement display
            if (find.requiredTool != null)
            {
                bool hasTool =
                    loadoutScreen.SelectedTools.Contains(find.requiredTool);

                if (find.toolEssential)
                {
                    sb.Append(hasTool
                        ? $" (tool ready ({find.requiredTool.displayName}))"
                        : $" (requires {find.requiredTool.displayName})");
                }
                else
                {
                    sb.Append(hasTool
                        ? $" (chance/time taken improved ({find.requiredTool.displayName}))"
                        : $" (easier with {find.requiredTool.displayName})");
                }
            }

            if (!string.IsNullOrWhiteSpace(find.notes))
            {
                sb.Append(" — ");
                sb.Append(find.notes);
            }

            sb.AppendLine();
        }

        potentialText.text = sb.ToString().TrimEnd();

        potentialImage.sprite =
            region.possibleFinds[0].resource.Icon
            ? region.possibleFinds[0].resource.Icon
            : defaultPotentialSprite;
    }

    //REPLACED
    //void UpdatePotentialFinds()
    //{
    //    var tools = loadoutScreen.SelectedTools;

    //    if (tools == null || tools.Count == 0)
    //    {
    //        potentialText.text = "None — bring tools to improve yield";
    //        potentialImage.sprite = defaultPotentialSprite;
    //        return;
    //    }

    //    // Simple example logic:
    //    // In a real system this comes from destination + season + tool tags

    //    System.Text.StringBuilder sb = new();

    //    foreach (var tool in tools)
    //    {
    //        string line = "";

    //        // Example: infer from tool name
    //        if (tool.displayName.Contains("Pickaxe"))
    //            line += "Pickaxe: essential for metals and minerals eg. quartz, sandstone. ";

    //        else if (tool.displayName.Contains("Net"))
    //            line += "Net: improved chance/yield of small creatures eg. spider, toad. ";

    //        else if (tool.displayName.Contains("Trap"))
    //            line += "Trap: improved chance/yield of larger creatures eg. mouse, bat.";

    //        else if (tool.displayName.Contains("Shears"))
    //            line += "Shears: improved chance/yield of small botanicals eg. reeds, foxglove.";

    //        else if (tool.displayName.Contains("Saw"))
    //            line += "Saw: essential for larger botanicals eg. tree branches.";

    //        else if (tool.displayName.Contains("Snare"))
    //            line += "Snare: essential for small animals eg. rabbit, crow.";

    //        else if (tool.displayName.Contains("Knife"))
    //            line += "Knife: improved chance/yield of spreading botanicals eg. moss, coral.";

    //        else if (tool.displayName.Contains("Bucket"))
    //            line += "Bucket: essential for waters eg. spring, sea.";

    //        else if (tool.displayName.Contains("Spade"))
    //            line += "Spade: improved chance/yield of elements eg. dirt, clay.";

    //        else if (tool.displayName.Contains("Silk pin"))
    //            line += "Silk pin: improved chance/yield of threads eg. spider silk, copper thread.";

    //        sb.AppendLine("• " + line);
    //    }

    //    potentialText.text = sb.ToString().TrimEnd();

    //    // Use first tool icon as visual hint
    //    potentialImage.sprite = tools[0].icon
    //        ? tools[0].icon
    //        : defaultPotentialSprite;
    //}

    // ---------------------------------------------------------
    // CAPACITY SUMMARY
    // ---------------------------------------------------------

    void UpdateCapacity()
    {
        var carry = loadoutScreen.selectedCarry;

        if (carry == null)
        {
            spaceText.text = "No carry selected";
            return;
        }

        int remaining = loadoutScreen.RemainingSlots;
        int capacity = carry.slotCapacity;

        if (remaining <= 0)
        {
            spaceText.text = "⚠ No space to bring finds back";
        }
        else
        {
            spaceText.text =
                $"Space for finds: {remaining} / {capacity}";
        }
    }
}