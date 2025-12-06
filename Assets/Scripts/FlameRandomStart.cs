using UnityEngine;

public class FlameRandomStart : MonoBehaviour
{
    void Start()
    {
        Animator anim = GetComponent<Animator>();
        float offset = Random.Range(0f, 1f);  // random time between 0 and 1 second
        anim.Play(0, 0, offset);

        anim.speed = Random.Range(0.8f, 1.2f);
    }
}