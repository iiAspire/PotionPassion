using UnityEngine;

public class MapRegion : MonoBehaviour
{
    public SORegion regionData;
    public LoadoutScreen loadoutScreen;

    public void Select()
    {
        if (loadoutScreen == null || regionData == null)
            return;

        loadoutScreen.SelectRegion(regionData);
        Debug.Log($"Target region: {regionData.displayName}");
    }
}