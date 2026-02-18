
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [HideInInspector] public int roomId = -1;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomManager.Instance.CollectKey();
        Destroy(gameObject);
    }
}

