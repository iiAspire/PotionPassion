using UnityEngine;

public class FollowUIElement : MonoBehaviour
{
    public RectTransform target;  // CauldronContents
    public RectTransform follower; // FX object inside FXCanvas

    void LateUpdate()
    {
        Vector3 worldPos = target.transform.position;
        follower.position = worldPos;
    }
}
