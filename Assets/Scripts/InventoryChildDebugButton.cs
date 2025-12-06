using UnityEngine;

public class InventoryChildDebugButton : MonoBehaviour
{
    [Header("Target to inspect at click time")]
    [SerializeField] private Transform target;

    public void DumpChildren()
    {
        Debug.Log($"📍 DebugButton instanceID = {GetInstanceID()} | scene={gameObject.scene.name}");

        if (target == null)
        {
            Debug.LogWarning("[InventoryDebugButton] No target assigned.");
            return;
        }

        Debug.Log(
            $"🔍 INVENTORY CHILD DUMP — {target.name} | " +
            $"active={target.gameObject.activeInHierarchy} | " +
            $"sceneLoaded={target.gameObject.scene.isLoaded} | " +
            $"childCount={target.childCount}"
        );

        for (int i = 0; i < target.childCount; i++)
        {
            var child = target.GetChild(i);
            var card = child.GetComponent<CardComponent>();

            Debug.Log(
                $"  [{i}] {child.name} | " +
                $"active={child.gameObject.activeInHierarchy} | " +
                $"hasCard={(card != null)} | " +
                $"instanceID={child.GetInstanceID()}"
            );
        }

        // Extra: global safety scan
        var allCards = FindObjectsOfType<CardComponent>(true);
        Debug.Log($"🌍 GLOBAL CardComponent count = {allCards.Length}");
    }
}