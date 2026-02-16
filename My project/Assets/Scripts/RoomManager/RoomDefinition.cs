using UnityEngine;

public class RoomDefinition : MonoBehaviour
{
    public int roomId = 0;

    [Header("Markers")]
    public Transform playerSpawnDefault;

    [Header("Entry spawns for DoorId 0-3")]
    public Transform[] entrySpawns = new Transform[4];

    public Transform GetEntrySpawn(int doorId)
    {
        if (doorId < 0 || doorId > 3) return playerSpawnDefault;
        if (entrySpawns != null && entrySpawns.Length == 4 && entrySpawns[doorId] != null)
            return entrySpawns[doorId];
        return playerSpawnDefault;
    }
}

