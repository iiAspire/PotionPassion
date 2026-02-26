using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BirdTest : MonoBehaviour
{
    public AudioClip[] clips;
    AudioSource src;

    void Start()
    {
        src = GetComponent<AudioSource>();
        src.spatialBlend = 0f;
        InvokeRepeating(nameof(Play), 1f, 2f);
    }

    void Play()
    {
        if (clips.Length == 0) return;
        src.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}