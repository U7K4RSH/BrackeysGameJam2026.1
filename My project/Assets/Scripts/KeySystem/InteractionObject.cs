using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    private bool playerInRange = false;

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
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpawnKey();
        }
    }

    private void SpawnKey()
    {
        Vector3 spawnPosition = transform.position + spawnOffset;
        RoomManager.Instance.SpawnKeyInCurrentRoom(spawnPosition);
    }
}
