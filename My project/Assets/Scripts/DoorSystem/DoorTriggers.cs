using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DoorTriggers : MonoBehaviour
{
    [SerializeField, Range(0, 3)]
    private int doorId;




    private BoxCollider2D Col;
    private void Awake()
    {
        Col = GetComponent<BoxCollider2D>();
        Col.isTrigger = true;

    }

    public void SetBlocking(bool blocked)
    { Col.isTrigger = !blocked; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomManager.Instance.EnterDoor(doorId);
    }
}