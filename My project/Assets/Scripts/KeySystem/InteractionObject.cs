using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [Header("Optional Mini-game")]
    [SerializeField] private bool spawnsMiniGame = false;
    [SerializeField] private GameObject miniGamePrefab;
    
    private bool playerInRange = false;

    [SerializeField] private bool spawnsHalfA = true;
    [SerializeField] private GameObject glintObject;

    private bool used = false;
    [SerializeField] private string interactIdOverride = "";
    private GameObject miniGameInstance = null;

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
        // If player presses interact while a mini-game is open, close it.
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (miniGameInstance != null)
            {
                var mg = miniGameInstance.GetComponent<MiniGridGame>();
                if (mg != null) mg.Close();
                else Destroy(miniGameInstance);
                miniGameInstance = null;
                return;
            }

            if (!used)
            {
                HandleInteraction();
            }
        }
    }

    private void HandleInteraction()
    {
        if (used) return;

        used = true;

        int roomId = RoomManager.Instance.GetCurrentRoomId();
        RoomManager.Instance.MarkInteractableUsed(roomId, GetInteractId());

        if (glintObject != null)
            glintObject.SetActive(false);

        Vector3 spawnPosition = transform.position + spawnOffset;

        if (spawnsMiniGame)
        {
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
            return;
        }

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
