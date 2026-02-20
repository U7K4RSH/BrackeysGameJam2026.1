using UnityEngine;
using UnityEngine.InputSystem;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private bool isHalfA = true;

    private bool playerInRange = false;
    private bool pickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
    }

    private void Update()
    {
        if (pickedUp) return;
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            pickedUp = true;

            if (isHalfA) RoomManager.Instance.CollectHalfA();
            else RoomManager.Instance.CollectHalfB();

            Debug.Log(isHalfA ? "Picked Half A" : "Picked Half B");
            Destroy(gameObject);
        }
    }
}
