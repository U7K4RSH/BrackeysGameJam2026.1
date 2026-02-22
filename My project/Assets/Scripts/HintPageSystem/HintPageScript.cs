using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class HintNotePickup : MonoBehaviour
{
    [TextArea(2, 6)]
    [SerializeField] private string message = "Someone left a note here, - 'Check cabinets with a glint. You need both key halves. the exit is in room 3 bottom'";

    [SerializeField] private float showSeconds = 6f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.9f;

    private AudioSource sfx;

    private bool playerInRange = false;
    private bool pickedUp = false;

    private void Awake()
    {
        sfx = GetComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
        sfx.spatialBlend = 0f; // 2D
        sfx.panStereo = 0f;    // dead center
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (pickedUp) return;
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            pickedUp = true;

            
            if (pickupClip != null && sfx != null)
                sfx.PlayOneShot(pickupClip, pickupVolume);

            var hud = RoomManager.Instance != null ? RoomManager.Instance.GetHUD() : null;
            if (hud != null) hud.ShowDialogue(message, showSeconds);

            float delay = (pickupClip != null) ? pickupClip.length : 0f;
            Destroy(gameObject, delay);
        }
    }
}