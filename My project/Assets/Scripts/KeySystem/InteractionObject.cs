using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [Header("Optional Mini-game")]
    [SerializeField] private bool spawnsMiniGame = true
    ;
    [SerializeField] private GameObject miniGamePrefab;
    
    private bool playerInRange = false;

    [SerializeField] private bool spawnsHalfA = true;
    [SerializeField] private GameObject glintObject;

    [Header("Audio")]
    [SerializeField] private AudioClip searchClip;
    [SerializeField, Range(0f, 1f)] private float searchVolume = 0.8f;
    [SerializeField] private AudioSource sfxSource;

    // NEW: result sounds
    [SerializeField] private AudioClip nothingFoundClip; // Sound A
    [SerializeField] private AudioClip keyFoundClip;     // Sound B
    [SerializeField, Range(0f, 1f)] private float resultVolume = 0.9f;

    private bool used = false;
    [SerializeField] private string interactIdOverride = "";
    private GameObject miniGameInstance = null;

    private void Start()
    {
        int roomId = RoomManager.Instance.GetCurrentRoomId();
        used = RoomManager.Instance.IsInteractableUsed(roomId, GetInteractId());

        if (glintObject != null)
            glintObject.SetActive(!used);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            // if player walks away while a mini-game is open, close it
            if (miniGameInstance != null)
            {
                var mg = miniGameInstance.GetComponent<MiniGridGame>();
                if (mg != null) mg.Close();
                else Destroy(miniGameInstance);
                miniGameInstance = null;
                if (glintObject != null) glintObject.SetActive(!used);
            }
        }
    }

    private void Update()
    {
        // If player presses interact while a mini-game is open, close it.
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (sfxSource != null && searchClip != null)
                sfxSource.PlayOneShot(searchClip, searchVolume);

            if (miniGameInstance != null)
            {
                var mg = miniGameInstance.GetComponent<MiniGridGame>();
                if (mg != null) mg.Close();
                else Destroy(miniGameInstance);
                miniGameInstance = null;
                if (glintObject != null) glintObject.SetActive(!used);
                return;
            }

            // If this interactable spawns a mini-game, allow opening the mini-game
            // only after both key halves have been collected. Ignore the "used"
            // flag for mini-games so they can be reopened repeatedly.
            if (spawnsMiniGame)
            {
                if (RoomManager.Instance != null && RoomManager.Instance.HasBothHalves())
                {
                    HandleInteraction();
                }
                else
                {
                    if (RoomManager.Instance != null)
                        SimpleHUD.Instance.ShowDialogue("Looks like a light panel, but i cant use it now!");
                }
            }
            else
            {
                if (!used)
                {
                    HandleInteraction();
                }
            }
        }
    }

    private void HandleInteraction()
    {
        if (used) return;

        Vector3 spawnPosition = transform.position + spawnOffset;

        if (spawnsMiniGame)
        {
            // Spawn mini-game without marking this interactable as used so it can be reopened.
            if (miniGamePrefab != null)
            {
                var inst = Instantiate(miniGamePrefab, spawnPosition, Quaternion.identity);
                if (inst.GetComponent<MiniGridGame>() == null)
                    inst.AddComponent<MiniGridGame>();
                miniGameInstance = inst;
            }
            else
            {
                var mgGO = new GameObject("MiniGridGame_Runtime");
                mgGO.AddComponent<MiniGridGame>();
                mgGO.transform.position = spawnPosition;
                miniGameInstance = mgGO;
            }

            if (glintObject != null)
                glintObject.SetActive(false);

            return;
        }

        // For pickups (half A / B) mark this interactable as used so it cannot be reused.
        used = true;
        int roomId = RoomManager.Instance.GetCurrentRoomId();
        RoomManager.Instance.MarkInteractableUsed(roomId, GetInteractId());

        if (glintObject != null)
            glintObject.SetActive(false);

        // Vector3 spawnPosition = transform.position + spawnOffset;

        bool spawned = false;

        if (spawnsHalfA)
            spawned = RoomManager.Instance.SpawnHalfAInCurrentRoom(spawnPosition);
        else
            spawned = RoomManager.Instance.SpawnHalfBInCurrentRoom(spawnPosition);

        if (sfxSource != null)
        {
            if (spawned && keyFoundClip != null)
                sfxSource.PlayOneShot(keyFoundClip, resultVolume);

            if (!spawned && nothingFoundClip != null)
                sfxSource.PlayOneShot(nothingFoundClip, resultVolume);
        }

        if (RoomManager.Instance != null)
        {
            if (spawned) SimpleHUD.Instance.ShowDialogue("I found a key piece!");
            else SimpleHUD.Instance.ShowDialogue("Hmm..., I tried searching....but found nothing");
        }
    }

    private string GetInteractId()
    {
        return string.IsNullOrEmpty(interactIdOverride) ? gameObject.name : interactIdOverride;
    }
}
