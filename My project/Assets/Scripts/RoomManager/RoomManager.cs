using System.Collections;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomManager : MonoBehaviour
{
    /* Previous Single key System, Replaced by 2 key halfs*/
    //[Header("Win / Key")]                                                       
    //[SerializeField] private GameObject keyPrefab;
    //private int keyRoomId;
    //private bool hasKey = false;
    //private GameObject spawnedKey;
    //private bool keyAlreadySpawned = false;

    [SerializeField] private int exitDoorId = 3;

    [Header("Key Halves")]
    [SerializeField] private GameObject keyHalfAPrefab;
    [SerializeField] private GameObject keyHalfBPrefab;

    [Header("Audio - Key Pickup")]
    [SerializeField] private AudioClip keyPickupClip;
    [SerializeField, Range(0f, 1f)] private float keyPickupVolume = 0.9f;
    [SerializeField] private AudioSource keySfxSource;

    [Header("Hint / Paper")]
    [SerializeField] private GameObject hintPrefab;

    [Header("Audio - Doors")]
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorLockedClip;
    [SerializeField, Range(0f, 1f)] private float doorVolume = 0.8f;
    [SerializeField] private AudioSource doorSfxSource;

    // When true, the next room loaded will receive the hint paper
    private bool spawnHintNextRoom = false;
    private GameObject spawnedHint;
    private bool hintAlreadySpawned = false;
   
    
    private int halfARoomId;
    private int halfBRoomId;

   
    private bool hasHalfA = false;
    private bool hasHalfB = false;

    
    private GameObject spawnedHalfA;
    private GameObject spawnedHalfB;

   
    private bool halfAAlreadySpawned = false;
    private bool halfBAlreadySpawned = false;

    public SimpleHUD GetHUD() => hud;

    private int exitRoomId;
    // whether the exit room lights should be off until re-enabled
    private bool exitRoomLightsOff = false;
    // store original intensities for global lights in the currently loaded exit room
    private System.Collections.Generic.Dictionary<Light2D, float> exitRoomOriginalIntensities = new System.Collections.Generic.Dictionary<Light2D, float>();
    // store original rotation for the exit room so we can restore after flipping
    private Quaternion exitRoomOriginalRotation = Quaternion.identity;
    private bool exitRoomOriginalRotationStored = false;
    // whether the player has seen the exit room while it's dark
    private bool hasSeenExitRoomDark = false;

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

    private readonly System.Collections.Generic.HashSet<string> usedInteractables // mujhe hashsets pata hai for 0(1) lookups but ye gpt ka tareeka hai bc mera nai, will refactor later.
    = new System.Collections.Generic.HashSet<string>();                           // bot isko roomId:interactId format me store kar raha hai, taki har room ke interactables alag track ho sake.
                                                                                  // will try a simpler bool tracking of 12 interactables per room later if this works fine for now.

    public int GetCurrentRoomId() => currentRoomId;

    // Returns true when both key halves have been collected
    public bool HasBothHalves() => hasHalfA && hasHalfB;
    // Returns true when the player has visited the exit room while it's dark
    public bool HasSeenDarkRoom() => hasSeenExitRoomDark;
    //private bool keyAlreadySpawned = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (keySfxSource == null)
            keySfxSource = GetComponent<AudioSource>();
        if(doorSfxSource == null)
    doorSfxSource = GetComponent<AudioSource>();
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

        // EXIT door check
        if (currentRoomId == exitRoomId && doorId == exitDoorId)
        {
            // blocked if exit-room lights are off
            if (exitRoomLightsOff)
            {
                if (doorSfxSource != null && doorLockedClip != null)
                    doorSfxSource.PlayOneShot(doorLockedClip, doorVolume);

                if (hud != null) hud.ShowDialogue("Exit is sealed while lights are off.");
                return;
            }

            if (!(hasHalfA && hasHalfB))
            {
                // play LOCKED sound
                if (doorSfxSource != null && doorLockedClip != null)
                    doorSfxSource.PlayOneShot(doorLockedClip, doorVolume);

                if (hud != null) hud.ShowDialogue("It seems like this exit is locked.");
                return;
            }

            // play OPEN sound (you are allowed to exit)
            if (doorSfxSource != null && doorOpenClip != null)
                doorSfxSource.PlayOneShot(doorOpenClip, doorVolume);

            hud.ShowWin();
            return;
        }

        // normal doors: play OPEN sound
        if (doorSfxSource != null && doorOpenClip != null)
            doorSfxSource.PlayOneShot(doorOpenClip, doorVolume);

        doorlockuntil = Time.time + doorCooldown;

        int nextRoomId = map[currentRoomId, doorId];

        SetAllDoorsBlocking(true);
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
        var camFollower = CameraFollow2D.FindAnyObjectByType<CameraFollow2D>();
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
        {
            Destroy(currentRoom.gameObject);
            exitRoomOriginalIntensities.Clear();
            exitRoomOriginalRotationStored = false;
        }


        GameObject roomGO = Instantiate(roomPrefabs[roomId]);
        currentRoom = roomGO.GetComponent<RoomDefinition>();
        currentRoomId = roomId;
        
        
        

        Transform spawn = useDefaultSpawn ? currentRoom.playerSpawnDefault : currentRoom.GetEntrySpawn(entryDoorId);
        player.position = spawn.position;
        
        // Update key visibility based on current room
        UpdateKeyVisibility(roomId);

        ApplyExitRoomLightsState();

        // If a hint was scheduled, spawn the hint prefab in this room (once)
        if (spawnHintNextRoom && hintPrefab != null && !hintAlreadySpawned)
        {
            Transform hintSpawn = currentRoom.Keyspawn != null ? currentRoom.Keyspawn : currentRoom.playerSpawnDefault;
            Vector3 pos = hintSpawn != null ? hintSpawn.position : player.position;
            spawnedHint = Instantiate(hintPrefab, pos, Quaternion.identity);
            hintAlreadySpawned = true;
            spawnHintNextRoom = false;
           
        }

        // if (!hasKey && roomId == keyRoomId && keyPrefab != null)
        // {
        //     if (currentRoom.Keyspawn != null)
        //         Instantiate(keyPrefab, currentRoom.Keyspawn.position, Quaternion.identity);
        //     else
        //         Instantiate(keyPrefab, currentRoom.playerSpawnDefault.position, Quaternion.identity);
        // }
       
        if (hud != null)
            hud.SetRoomCounter(currentRoomId);
        
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

        if (keySfxSource != null && keyPickupClip != null)
            keySfxSource.PlayOneShot(keyPickupClip, keyPickupVolume);

        if (hud != null) hud.ShowHalfAIcon();
        if (hasHalfA && hasHalfB && hud != null)
            hud.ShowFullKeyIcon();
        if (hud != null) hud.ShowDialogue("Looks like an upper key half..");

        if (hasHalfA && hasHalfB)
        {

            CameraShake2D.Instance?.Shake(0.25f, 0.12f);
            // Both halves collected: turn off lights in exit room until mini-game is won
            SetExitRoomLightsEnabled(false);
            

        }

    }

    public void CollectHalfB()
    {
        hasHalfB = true;

        if (keySfxSource != null && keyPickupClip != null)
            keySfxSource.PlayOneShot(keyPickupClip, keyPickupVolume);

        if (hud != null) hud.ShowHalfBIcon();
        if (hasHalfA && hasHalfB && hud != null)
            hud.ShowFullKeyIcon();
        if (hud != null) hud.ShowDialogue("Looks like a lower key half");

        if (hasHalfA && hasHalfB)
        {

            CameraShake2D.Instance?.Shake(0.25f, 0.12f);
            // Both halves collected: turn off lights in exit room until mini-game is won
            SetExitRoomLightsEnabled(false);
          
        }
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

    public bool SpawnHalfAInCurrentRoom(Vector3 position)
    {
        if (currentRoom == null) return false;
        if (spawnedHalfA != null || halfAAlreadySpawned) return false;
        if (currentRoomId != halfARoomId) return false;

        spawnedHalfA = Instantiate(keyHalfAPrefab, position, Quaternion.identity);
        halfAAlreadySpawned = true;
        return true;
    }

    public bool SpawnHalfBInCurrentRoom(Vector3 position)
    {
        if (currentRoom == null) return false;
        if (spawnedHalfB != null || halfBAlreadySpawned) return false;
        if (currentRoomId != halfBRoomId) return false;

        spawnedHalfB = Instantiate(keyHalfBPrefab, position, Quaternion.identity);
        halfBAlreadySpawned = true;
        return true;
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

    public void SetExitRoomLightsEnabled(bool enabled)
    {
        exitRoomLightsOff = !enabled;
        ApplyExitRoomLightsState();
    }

    private void ApplyExitRoomLightsState()
    {
        if (currentRoom == null) return;

        // Flip or restore the room rotation when exit-room lights are toggled
        if (currentRoomId == exitRoomId)
        {
            if (exitRoomLightsOff)
            {
                hasSeenExitRoomDark = true;
                hud.ShowDialogue("It's too dark here — turn the lights on first.");
                if (!exitRoomOriginalRotationStored)
                {
                    exitRoomOriginalRotation = currentRoom.transform.rotation;
                    exitRoomOriginalRotationStored = true;
                }
                currentRoom.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            }
            else
            {
                if (exitRoomOriginalRotationStored)
                {
                    currentRoom.transform.rotation = exitRoomOriginalRotation;
                    exitRoomOriginalRotationStored = false;
                }
            }
        }

        // Adjust player's personal Light2D intensity depending on room
        if (player != null)
        {
            var pLight = player.GetComponentInChildren<Light2D>(true);
            if (pLight != null)
            {
                float targetIntensity = (currentRoomId == exitRoomId && exitRoomLightsOff) ? 0f : 0.35f;
                pLight.intensity = targetIntensity;
            }

            // Inform player movement to invert controls when exit-room lights are turned off
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                bool invert = (currentRoomId == exitRoomId && exitRoomLightsOff);
                pm.SetInverted(invert);
            }
        }
    }

}
