
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomManager.Instance.CollectKey();
        Destroy(gameObject);
    }
}

