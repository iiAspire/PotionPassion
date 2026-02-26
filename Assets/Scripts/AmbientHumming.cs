using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AmbientHumming : MonoBehaviour
{
    public AudioClip[] humNotes;

    [Header("Silence Between Episodes")]
    public float minSilence = 20f;
    public float maxSilence = 60f;

    [Header("Riff Settings")]
    public int minNotes = 3;
    public int maxNotes = 4;

    [Header("Repeats per Episode")]
    public int minRepeats = 2;
    public int maxRepeats = 4;

    [Header("Motif Memory")]
    [Range(0f, 1f)]
    public float chanceReuseOldMotif = 0.6f;

    [Range(0f, 1f)]
    public float chanceReplaceMotif = 0.25f;

    [Header("Timing")]
    public Vector2 gapBetweenNotes = new Vector2(0.2f, 0.5f);
    public Vector2 gapBetweenRepeats = new Vector2(0.6f, 1.6f);

    [Header("Variation")]
    public Vector2 pitchRange = new Vector2(0.99f, 1.01f);
    public Vector2 volumeRange = new Vector2(0.85f, 1f);

    private AudioSource source;
    private List<AudioClip> storedMotif = null;

    void Awake()
    {
        source = GetComponent<AudioSource>();

        source.spatialBlend = 0f; // 2D
        source.loop = false;
        source.playOnAwake = false;
    }

    void Start()
    {
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSilence, maxSilence));

            var motif = ChooseMotif();

            yield return StartCoroutine(PlayMotif(motif));
        }
    }

    List<AudioClip> ChooseMotif()
    {
        bool reuse =
            storedMotif != null &&
            Random.value < chanceReuseOldMotif;

        if (reuse)
        {
            // Maybe forget it afterward
            if (Random.value < chanceReplaceMotif)
                storedMotif = GenerateMotif();

            return storedMotif;
        }

        // Create new motif
        storedMotif = GenerateMotif();
        return storedMotif;
    }

    List<AudioClip> GenerateMotif()
    {
        var motif = new List<AudioClip>();

        if (humNotes == null || humNotes.Length == 0)
            return motif;

        int count = Random.Range(minNotes, maxNotes + 1);

        for (int i = 0; i < count; i++)
        {
            motif.Add(humNotes[Random.Range(0, humNotes.Length)]);
        }

        return motif;
    }

    IEnumerator PlayMotif(List<AudioClip> motif)
    {
        int repeats = Random.Range(minRepeats, maxRepeats + 1);

        for (int r = 0; r < repeats; r++)
        {
            foreach (var clip in motif)
            {
                PlayNote(clip);

                float gap = Random.Range(gapBetweenNotes.x, gapBetweenNotes.y);
                yield return new WaitForSeconds(clip.length / source.pitch + gap);
            }

            yield return new WaitForSeconds(
                Random.Range(gapBetweenRepeats.x, gapBetweenRepeats.y)
            );
        }
    }

    void PlayNote(AudioClip clip)
    {
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.volume = Random.Range(volumeRange.x, volumeRange.y);

        source.clip = clip;
        source.Play();
    }
}