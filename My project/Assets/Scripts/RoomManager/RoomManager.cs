using System.Collections;
//using System.Diagnostics;
using UnityEngine;

public class RoomManager : MonoBehaviour
{

    //[Header("Win / Key")]
    //[SerializeField] private GameObject keyPrefab;

    [Header("Key Halves")]
    [SerializeField] private GameObject keyHalfAPrefab;
    [SerializeField] private GameObject keyHalfBPrefab;

    [Header("Hint / Paper")]
    [SerializeField] private GameObject hintPrefab;
    // When true, the next room loaded will receive the hint paper
    private bool spawnHintNextRoom = false;
    private GameObject spawnedHint;
    private bool hintAlreadySpawned = false;
   
    //private int keyRoomId;
    private int halfARoomId;
    private int halfBRoomId;

    //private bool hasKey = false;
    private bool hasHalfA = false;
    private bool hasHalfB = false;

    //private GameObject spawnedKey;
    private GameObject spawnedHalfA;
    private GameObject spawnedHalfB;

    //private bool keyAlreadySpawned = false;
    private bool halfAAlreadySpawned = false;
    private bool halfBAlreadySpawned = false;


    [SerializeField] private int exitDoorId = 3;
   

    private int exitRoomId;

    //private bool blackoutActive = false;
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

    private readonly System.Collections.Generic.HashSet<string> usedInteractables
    = new System.Collections.Generic.HashSet<string>();

    public int GetCurrentRoomId() => currentRoomId;

    //private bool keyAlreadySpawned = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        rng = new System.Random();

        int n = roomPrefabs.Length;

        
        exitRoomId = n - 1;

        halfARoomId = rng.Next(0, n);

        // forcing half b to be different if possible, to ensure they are not in the same room
        if (n <= 1) halfBRoomId = halfARoomId;
        else
        {
            do { halfBRoomId = rng.Next(0, n); }
            while (halfBRoomId == halfARoomId);
        }

        GenerateMapping();
        LoadRoom(0, entryDoorId: 0, useDefaultSpawn: true);

        // Schedule a hint to appear in the next room entered after 30s
        Invoke(nameof(EnableHintNextRoomFlag), 30f);
    }

    private void EnableHintNextRoomFlag()
    {
        spawnHintNextRoom = true;
        Debug.Log("Hint scheduled: will spawn in the next room the player enters.");
    }

    // Public manual trigger (if needed elsewhere)
    public void TriggerHintNextRoom()
    {
        spawnHintNextRoom = true;
    }

    public void EnterDoor(int doorId)
    {
        if (Time.time < doorlockuntil) return; 
        if (currentRoom == null) return;

        if (currentRoomId == exitRoomId && doorId == exitDoorId)
        {
            if (!(hasHalfA && hasHalfB))
            {
                Debug.Log("Exit is locked. Need both key halves.");
                return;
            }
            hud.ShowWin();
            Debug.Log("YOU WIN!");
            
            return;
        }

        doorlockuntil = Time.time + doorCooldown;

        int nextRoomId = map[currentRoomId, doorId];
        
        SetAllDoorsBlocking(true); // block all doors during transition
        CancelInvoke(nameof(UnlockDoors));
        
        StartCoroutine(TransitionToRoom(nextRoomId, doorId));
    }

    private IEnumerator TransitionToRoom(int nextRoomId, int entryDoorId)
    {
        // Fade out
        FadeTransitionManager.Instance.FadeOut();
        // wait for configured fade duration
        yield return new WaitForSeconds(FadeTransitionManager.Instance.FadeDuration);

        // Load the new room while screen is fully dark
        LoadRoom(nextRoomId, entryDoorId: entryDoorId);

        // Snap the camera to the new player position to avoid visible camera panning
        var camFollower = FindObjectOfType<CameraFollow2D>();
        if (camFollower != null)
            camFollower.SnapToTarget();

        // Fade in
        FadeTransitionManager.Instance.FadeIn();
        yield return new WaitForSeconds(FadeTransitionManager.Instance.FadeDuration);

        Invoke(nameof(UnlockDoors), 0.1f); // unlock after fade completes
    }

    private void LoadRoom(int roomId, int entryDoorId, bool useDefaultSpawn = false)
    {
        if (currentRoom != null)
            Destroy(currentRoom.gameObject);

        GameObject roomGO = Instantiate(roomPrefabs[roomId]);
        currentRoom = roomGO.GetComponent<RoomDefinition>();
        currentRoomId = roomId;
        
        
        

        Transform spawn = useDefaultSpawn ? currentRoom.playerSpawnDefault : currentRoom.GetEntrySpawn(entryDoorId);
        player.position = spawn.position;
        
        // Update key visibility based on current room
        UpdateKeyVisibility(roomId);

        // If a hint was scheduled, spawn the hint prefab in this room (once)
        if (spawnHintNextRoom && hintPrefab != null && !hintAlreadySpawned)
        {
            Transform hintSpawn = currentRoom.Keyspawn != null ? currentRoom.Keyspawn : currentRoom.playerSpawnDefault;
            Vector3 pos = hintSpawn != null ? hintSpawn.position : player.position;
            spawnedHint = Instantiate(hintPrefab, pos, Quaternion.identity);
            hintAlreadySpawned = true;
            spawnHintNextRoom = false;
            Debug.Log("Spawned hint paper in room " + currentRoomId);
        }

        // if (!hasKey && roomId == keyRoomId && keyPrefab != null)
        // {
        //     if (currentRoom.Keyspawn != null)
        //         Instantiate(keyPrefab, currentRoom.Keyspawn.position, Quaternion.identity);
        //     else
        //         Instantiate(keyPrefab, currentRoom.playerSpawnDefault.position, Quaternion.identity);
        // }
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

    /*
       public void CollectKey()
    {
        hasKey = true;
        Debug.Log("Key collected!");


        if (hud != null)
            hud.ShowKeyIcon();
    }
    */

    public void CollectHalfA()
    {
        hasHalfA = true;
        if (hud != null) hud.ShowHalfAIcon();
        if (hasHalfA && hasHalfB && hud != null)
            hud.ShowFullKeyIcon();
        Debug.Log("Half A collected!");
    }

    public void CollectHalfB()
    {
        hasHalfB = true;
        if (hud != null) hud.ShowHalfBIcon();
        if (hasHalfA && hasHalfB && hud != null)
            hud.ShowFullKeyIcon();
        Debug.Log("Half B collected!");
    }

    /*
       public void SpawnKeyInCurrentRoom(Vector3 position)
    {
        if (currentRoom == null) return;
        if (spawnedKey != null || keyAlreadySpawned || keyRoomId!=currentRoomId) return;
        
        spawnedKey = Instantiate(keyPrefab, position, Quaternion.identity);
        keyAlreadySpawned = true;
    }
    */

    public void SpawnHalfAInCurrentRoom(Vector3 position)
    {
        if (currentRoom == null) return;
        if (spawnedHalfA != null || halfAAlreadySpawned) return;
        if (currentRoomId != halfARoomId) return;

        spawnedHalfA = Instantiate(keyHalfAPrefab, position, Quaternion.identity);
        halfAAlreadySpawned = true;
    }

    public void SpawnHalfBInCurrentRoom(Vector3 position)
    {
        if (currentRoom == null) return;
        if (spawnedHalfB != null || halfBAlreadySpawned) return;
        if (currentRoomId != halfBRoomId) return;

        spawnedHalfB = Instantiate(keyHalfBPrefab, position, Quaternion.identity);
        halfBAlreadySpawned = true;
    }

    private void UpdateKeyVisibility(int newRoomId)
    {
        /*
         if (spawnedKey == null) return;
        spawnedKey.SetActive(keyRoomId == newRoomId);
        */
        if (spawnedHalfA != null)
            spawnedHalfA.SetActive(halfARoomId == newRoomId);

        if (spawnedHalfB != null)
            spawnedHalfB.SetActive(halfBRoomId == newRoomId);
    }

    public bool IsInteractableUsed(int roomId, string interactId)
    {
        return usedInteractables.Contains($"{roomId}:{interactId}");
    }

    public void MarkInteractableUsed(int roomId, string interactId)
    {
        usedInteractables.Add($"{roomId}:{interactId}");
    }

}
