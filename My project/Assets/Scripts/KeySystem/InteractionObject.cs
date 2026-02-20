using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    private bool playerInRange = false;

    [SerializeField] private bool spawnsHalfA = true;
    [SerializeField] private GameObject glintObject;

    [Header("Audio")]
    [SerializeField] private AudioClip searchClip;
    [SerializeField, Range(0f, 1f)] private float searchVolume = 0.8f;
    [SerializeField] private AudioSource sfxSource;

    private bool used = false;
    [SerializeField] private string interactIdOverride = "";

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
        }
    }

    private void Update()
    {
        if (!used && playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (sfxSource != null && searchClip != null)
                sfxSource.PlayOneShot(searchClip, searchVolume);

            SpawnKey();
        }
    }

    private void SpawnKey()
    {
        if (used) return;

        used = true;

        int roomId = RoomManager.Instance.GetCurrentRoomId();
        RoomManager.Instance.MarkInteractableUsed(roomId, GetInteractId());

        if (glintObject != null)
            glintObject.SetActive(false);

        Vector3 spawnPosition = transform.position + spawnOffset;

        bool spawned = false;

        if (spawnsHalfA)
            spawned = RoomManager.Instance.SpawnHalfAInCurrentRoom(spawnPosition);
        else
            spawned = RoomManager.Instance.SpawnHalfBInCurrentRoom(spawnPosition);

        if (RoomManager.Instance != null)
        {
            if (spawned) SimpleHUD.Instance.ShowDialogue("You found a key piece!");
            else SimpleHUD.Instance.ShowDialogue("You searched... but found nothing.");
        }
    }

    private string GetInteractId()
    {
        return string.IsNullOrEmpty(interactIdOverride) ? gameObject.name : interactIdOverride;
    }
}
