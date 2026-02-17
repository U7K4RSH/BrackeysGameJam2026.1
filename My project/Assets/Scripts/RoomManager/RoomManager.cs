using UnityEngine;

public class RoomManager : MonoBehaviour
{

    [Header("Win / Key")]
    [SerializeField] private GameObject keyPrefab;   
    [SerializeField] private int exitDoorId = 3;     

    private int exitRoomId;      
    private int keyRoomId;      
    private bool hasKey = false;

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
     
    private float doorlockuntil = 0f; // simple cooldown to prevent door spamming
    [SerializeField] private float doorCooldown = 2.5f;

    [SerializeField] private SimpleHUD hud;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        rng = new System.Random(seed);

        int n = roomPrefabs.Length;

        
        exitRoomId = n - 1;

        do { keyRoomId = rng.Next(0, n); }
        while (keyRoomId == 0 || keyRoomId == exitRoomId);



        GenerateMapping();
        LoadRoom(0, entryDoorId: 0);
    }

    public void EnterDoor(int doorId)
    {
        if (Time.time < doorlockuntil) return; 
        if (currentRoom == null) return;

        if (currentRoomId == exitRoomId && doorId == exitDoorId)
        {
            if (!hasKey)
            {
                Debug.Log("Exit is locked. Need the key.");
                return;
            }
            hud.ShowWin();
            Debug.Log("YOU WIN!");
            
            return;
        }

            doorlockuntil = Time.time + doorCooldown;

        int nextRoomId = map[currentRoomId, doorId];
        LoadRoom(nextRoomId, entryDoorId: doorId);

        SetAllDoorsBlocking(true); // block all doors during transition
        CancelInvoke(nameof(UnlockDoors));
        Invoke(nameof(UnlockDoors), doorCooldown); // unlock after cooldown
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

        if (!hasKey && roomId == keyRoomId && keyPrefab != null)
        {
            if (currentRoom.Keyspawn != null)
                Instantiate(keyPrefab, currentRoom.Keyspawn.position, Quaternion.identity);
            else
                Instantiate(keyPrefab, currentRoom.playerSpawnDefault.position, Quaternion.identity);
        }
        Debug.Log("LoadRoom called on: " + gameObject.name);
        if (hud != null)
            hud.SetRoomCounter(currentRoomId);
        Debug.Log("HUD is assigned: " + hud.gameObject.name);
    }

    private void GenerateMapping()
    {
        int n = roomPrefabs.Length;
        map = new int[n, 4];

        for (int r = 0; r < n; r++)
        {


            for (int d = 0; d < 4; d++)
            {
                int next = rng.Next(0, n);

                if (n > 1 && next == r)
                {
                    next = (r + rng.Next(1, n)) % n; 
                }
                map[r, d] = next;
            }
        }
        
        if (n > 1 && map[0, 0] == 0) map[0, 0] = 1;
    }

    private void SetAllDoorsBlocking(bool blocked)
    {
     var doors = FindObjectsByType<DoorTriggers>(FindObjectsSortMode.None);
        foreach (var d in doors)
            d.SetBlocking(blocked);


    }

    private void UnlockDoors()
    { 
    
    SetAllDoorsBlocking(false);
    }

    public void CollectKey()
    {
        hasKey = true;
        Debug.Log("Key collected!");
    }
}
