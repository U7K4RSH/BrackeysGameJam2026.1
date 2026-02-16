using UnityEngine;

public class DoorTriggers : MonoBehaviour
{
    [SerializeField, Range(0, 3)]
    private int doorId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomManager.Instance.EnterDoor(doorId);
    }
}