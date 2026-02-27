using UnityEngine;
using System.Collections.Generic;

public class RegionDatabase : MonoBehaviour
{
    public static Dictionary<int, SORegion> ByID { get; private set; }

    void Awake()
    {
        var regions = Resources.LoadAll<SORegion>("Regions");

        ByID = new Dictionary<int, SORegion>();

        foreach (var r in regions)
        {
            if (r != null)
                ByID[r.nodeID] = r;
        }

        Debug.Log($"Loaded {ByID.Count} regions");
    }
}