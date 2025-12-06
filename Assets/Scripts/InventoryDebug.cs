using UnityEngine;
using System.Text;

public static class InventoryDebug
{
    public static void Dump(string label, params Transform[] parents)
    {
        //StringBuilder sb = new StringBuilder();
        //sb.AppendLine($"🔎 INVENTORY DUMP — {label}");

        //for (int i = 0; i < parents.Length; i++)
        //{
        //    Transform p = parents[i];

        //    if (p == null)
        //    {
        //        sb.AppendLine($"  [{i}] NULL parent");
        //        continue;
        //    }

        //    sb.AppendLine(
        //        $"  [{i}] {p.name} | " +
        //        $"sceneLoaded={p.gameObject.scene.isLoaded} | " +
        //        $"active={p.gameObject.activeInHierarchy} | " +
        //        $"children={p.childCount}"
        //    );

        //    foreach (var card in p.GetComponentsInChildren<CardComponent>(true))
        //    {
        //        sb.AppendLine(
        //            $"     - {card.name} | runtimeID={card.runtimeID}"
        //        );
        //    }
        //}

        //Debug.Log(sb.ToString());
    }
}