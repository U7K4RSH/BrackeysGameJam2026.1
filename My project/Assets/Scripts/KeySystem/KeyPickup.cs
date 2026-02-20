
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private bool isHalfA = true;
    private void OnTriggerEnter2D(Collider2D other)
    {
        /*if (!other.CompareTag("Player")) return;
        RoomManager.Instance.CollectKey();
        Destroy(gameObject);
        */
        if (!other.CompareTag("Player")) return;

        if (isHalfA) RoomManager.Instance.CollectHalfA();
        else RoomManager.Instance.CollectHalfB();
        Debug.Log(isHalfA ? "Picked Half A" : "Picked Half B");
        Destroy(gameObject);
    }
}

