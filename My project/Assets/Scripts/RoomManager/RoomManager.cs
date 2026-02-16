using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void EnterDoor(int doorId)
    {
        Debug.Log("Entered door: " + doorId);
        // Later: load next room, teleport player, etc.
    }
}