using UnityEngine;
using UnityEngine.InputSystem;

public class HintNotePickup : MonoBehaviour
{
    [TextArea(2, 6)]
    [SerializeField] private string message = "Someone left a note here, - 'Check cabinets with a glint. You need both key halves.'";

    [SerializeField] private float showSeconds = 6f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.9f;

    private bool playerInRange = false;
    private bool pickedUp = false;

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

            // play pickup sound (won't get cut off after Destroy)
            if (pickupClip != null && Camera.main != null)
                AudioSource.PlayClipAtPoint(pickupClip, Camera.main.transform.position, pickupVolume);

            var hud = RoomManager.Instance != null ? RoomManager.Instance.GetHUD() : null;
            if (hud != null) hud.ShowDialogue(message, showSeconds);

            Destroy(gameObject);
        }
    }
}