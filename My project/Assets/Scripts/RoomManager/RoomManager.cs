using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Room prefabs (index = roomId)")]
    public GameObject[] roomPrefabs;

    [Header("Player Transform")]
    public Transform player;

    [Header("Run seed (change to reshuffle)")]
    public int seed = 12345;

    private int[,] map;                 // [roomId, doorId] -> nextRoomId
    private int currentRoomId;
    private RoomDefinition currentRoom;
    private System.Random rng;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        rng = new System.Random(seed);
        GenerateMapping();

        LoadRoom(0, entryDoorId: 0);
    }

    public void EnterDoor(int doorId)
    {
        if (currentRoom == null) return;

        int nextRoomId = map[currentRoomId, doorId];
        LoadRoom(nextRoomId, entryDoorId: doorId);
    }

    private void LoadRoom(int roomId, int entryDoorId)
    {
        if (currentRoom != null)
            Destroy(currentRoom.gameObject);

        GameObject roomGO = Instantiate(roomPrefabs[roomId]);
        currentRoom = roomGO.GetComponent<RoomDefinition>();
        currentRoomId = roomId;

        Transform spawn = currentRoom.GetEntrySpawn(entryDoorId);
        player.position = spawn.position;
    }

    private void GenerateMapping()
    {
        int n = roomPrefabs.Length;
        map = new int[n, 4];

        for (int r = 0; r < n; r++)
            for (int d = 0; d < 4; d++)
                map[r, d] = rng.Next(0, n);

        // Optional: avoid start Door_0 sending you back to start immediately
        if (n > 1 && map[0, 0] == 0) map[0, 0] = 1;
    }
}
