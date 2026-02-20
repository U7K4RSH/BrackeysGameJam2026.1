using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Footsteps2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb; // drag your player's RB here (or auto)

    [Header("Footstep Clips (5 sounds)")]
    [SerializeField] private AudioClip[] stepClips;

    [Header("Tuning")]
    [SerializeField] private float stepInterval = 0.35f; // time between steps while moving
    [SerializeField] private float minSpeed = 0.1f;      // below this = not moving
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

    private AudioSource source;
    private float timer = 0f;
    private int lastIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>(); // works if script is on a child
    }

    private void Update()
    {
        if (rb == null) return;
        if (stepClips == null || stepClips.Length == 0) return;

        float speed = rb.linearVelocity.magnitude;

        // Not moving -> reset timer so it doesn't instantly step when you start again
        if (speed < minSpeed)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= stepInterval)
        {
            timer = 0f;
            PlayRandomStep();
        }
    }

    private void PlayRandomStep()
    {
        if (stepClips.Length == 1)
        {
            source.PlayOneShot(stepClips[0], volume);
            return;
        }

        int idx;
        do { idx = Random.Range(0, stepClips.Length); }
        while (idx == lastIndex);

        lastIndex = idx;

        AudioClip clip = stepClips[idx];
        if (clip != null)
            source.PlayOneShot(clip, volume);
    }
}
