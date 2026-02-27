using UnityEngine;

public class CardInventoryAdder : MonoBehaviour
{
    [Header("Where cards go (your inventory parent)")]
    [SerializeField] private Transform inventoryParent;

    public void AddCard(CardData template, int amount = 1)
    {
        if (template == null || template.cardPrefab == null || inventoryParent == null)
            return;

        for (int i = 0; i < amount; i++)
            SpawnRuntimeCard(template, inventoryParent);
    }

    private void SpawnRuntimeCard(CardData template, Transform parent)
    {
        // Runtime copy (so we never mutate the asset)
        CardData runtime = ScriptableObject.CreateInstance<CardData>();
        runtime.CopyFrom(template);
        runtime.cardName = template.cardName;
        runtime.baseName = template.baseName;

        runtime.ApplyDefaultColor();
        runtime.processedType = ProcessedType.None;
        runtime.processedIcon = null;
        runtime.ApplyPartIcon();
        runtime.ApplyQuantityIcon();

        GameObject cardObj = Instantiate(template.cardPrefab, parent);

        CardComponent comp = cardObj.GetComponent<CardComponent>();
        if (comp != null)
        {
            if (string.IsNullOrEmpty(comp.runtimeID))
                comp.runtimeID = System.Guid.NewGuid().ToString();

            comp.SetCardData(runtime);
        }
    }
}