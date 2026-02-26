using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Camera uiCamera;

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 worldPoint =
            uiCamera.ScreenToWorldPoint(eventData.position);

        RaycastHit2D hit =
            Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider == null) return;

        MapRegion region =
            hit.collider.GetComponent<MapRegion>();

        if (region != null)
            region.Select();
    }
}