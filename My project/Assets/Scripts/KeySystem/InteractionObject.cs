using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    private bool playerInRange = false;

    [SerializeField] private bool spawnsHalfA = true;
    [SerializeField] private GameObject glintObject;

    private bool used = false;
    [SerializeField] private string interactIdOverride = "";

    private void Start()
    {
        int roomId = RoomManager.Instance.GetCurrentRoomId();
        used = RoomManager.Instance.IsInteractableUsed(roomId, GetInteractId());

        if (glintObject != null)
            glintObject.SetActive(!used);
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

        if (spawnsHalfA)
            RoomManager.Instance.SpawnHalfAInCurrentRoom(spawnPosition);
        else
            RoomManager.Instance.SpawnHalfBInCurrentRoom(spawnPosition);
    }

    private string GetInteractId()
    {
        return string.IsNullOrEmpty(interactIdOverride) ? gameObject.name : interactIdOverride;
    }
}
