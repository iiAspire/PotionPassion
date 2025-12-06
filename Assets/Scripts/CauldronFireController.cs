using UnityEngine;

public class CauldronFireController : MonoBehaviour
{
    public GameObject[] fireVisuals;   // Supports multiple flames
    public GameObject heatGlow;        // Heat glow image object

    public bool IsFireOn { get; private set; }

    public void ToggleFire()
    {
        SetFire(!IsFireOn);
    }

    public void SetFire(bool state)
    {
        IsFireOn = state;

        foreach (GameObject flame in fireVisuals)
        {
            if (flame != null)
            {
                flame.SetActive(state);

                if (state)
                {
                    // Restart animation for variation script
                    var animator = flame.GetComponent<Animator>();
                    if (animator)
                        animator.Update(0);
                }
            }
        }

        // Turn heat glow ON/OFF
        if (heatGlow != null)
            heatGlow.SetActive(state);
    }
}