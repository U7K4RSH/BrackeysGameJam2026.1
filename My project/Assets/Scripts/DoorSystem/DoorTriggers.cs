using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class DoorTriggers : MonoBehaviour
{
    [SerializeField, Range(0, 3)]
    private int doorId;

    [Header("Audio")]
    [SerializeField] private AudioClip doorClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private BoxCollider2D col;
    private AudioSource audioSource;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public void SetBlocking(bool blocked)
    {
        col.isTrigger = !blocked;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
         if (!other.CompareTag("Player")) return;

    if (doorClip != null)
        audioSource.PlayOneShot(doorClip, volume);

    RoomManager.Instance.EnterDoor(doorId);
    }
}