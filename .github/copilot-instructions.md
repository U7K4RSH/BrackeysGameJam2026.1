# Copilot Instructions for BrackeysGameJam2026.1

## Purpose
Short, actionable guidance so an AI coding agent (or new dev) can be productive quickly in this Unity game jam project.

## Big-picture architecture
- Feature-folder layout under `Assets/Scripts` (e.g., `RoomManager`, `DoorSystem`, `UISystem`).
- Runtime-driven rooms: the game composes the level by instantiating/destroying room prefabs at runtime rather than changing Unity scenes. See `RoomManager` for map generation and spawn logic.
- Camera is a smooth follower (`CameraFollow2D`) using `Vector3.SmoothDamp`; it stores internal velocity so abrupt player teleports may produce visible camera panning unless the velocity is reset.

## Key components (quick links)
- `Assets/Scripts/RoomManager/RoomManager.cs` — room mapping, spawn, door enter flow (`EnterDoor`), and transitions (`TransitionToRoom`).
- `Assets/Scripts/DoorSystem/DoorTriggers.cs` — triggers player entry and calls `RoomManager.EnterDoor(doorId)`.
- `Assets/Scripts/UISystem/FadeTransitionManager.cs` — singleton that creates a runtime fullscreen black overlay and exposes `FadeIn()`/`FadeOut()` and `FadeDuration`.
- `Assets/Scripts/Camera system/CameraFollower.cs` — smooth camera follow; use `SnapToTarget()` to instantly place the camera and clear smoothing velocity when swapping rooms while faded out.
- `Assets/Scripts/UISystem/HUDSystem.cs` — HUD helpers (room counter, pause, restart, win panel).

## Project-specific conventions and patterns
- Singletons: managers expose a static `Instance` (e.g., `RoomManager.Instance`, `FadeTransitionManager.Instance`) and often `DontDestroyOnLoad` for persistent behavior.
- Prefab-first design: Rooms are prefabs listed in `RoomManager.roomPrefabs` (index equals room id). Add rooms by creating prefabs and adding them to the array in the inspector.
- Transitions: transitions are coroutine-driven. `RoomManager.TransitionToRoom()` performs `FadeOut`, `LoadRoom`, `SnapToTarget()` (camera), then `FadeIn`. Respect `FadeTransitionManager.FadeDuration` when waiting.
- Door blocking: doors are temporarily blocked during transitions via `SetAllDoorsBlocking(true)` and later unlocked with `UnlockDoors()`.

## Camera & transition gotchas (explicit, actionable)
- If the player is teleported (e.g., entering right door → spawn on left in new room), the camera's `SmoothDamp` velocity will carry-over causing an unwanted pan. Use `CameraFollow2D.SnapToTarget()` immediately after `LoadRoom()` and while the screen is fully faded out to eliminate this.
- Alternative: during transition you can disable camera updates entirely by setting a boolean on the camera follower or temporarily clearing the `target` and restoring it after fade; prefer `SnapToTarget()` for minimal changes.

## Developer workflows
- Play in the Unity Editor `Play` mode. There are no project CLI build or test scripts — use Unity's standard build pipeline.
- To add a room: create a prefab under `Assets/` and add it to `RoomManager.roomPrefabs` (index position matters for mapping).

## Integration points & extension notes
- Key system: `RoomManager.SpawnKeyInCurrentRoom()` and `KeyPickup` are used to place/collect keys; the `exitDoorId`/`exitRoomId` logic enforces win condition.
- UI overlays are runtime-created; modifying `FadeTransitionManager` and `HUDSystem` is the intended approach rather than changing scenes.

## Files to inspect when working on features
- Room flow: `Assets/Scripts/RoomManager/RoomManager.cs`
- Door behavior: `Assets/Scripts/DoorSystem/DoorTriggers.cs`
- Camera: `Assets/Scripts/Camera system/CameraFollower.cs`
- Transitions/fades: `Assets/Scripts/UISystem/FadeTransitionManager.cs`
- HUD / pause: `Assets/Scripts/UISystem/HUDSystem.cs`

## Quick examples
- To implement a new transition: modify `FadeTransitionManager` and update `RoomManager.TransitionToRoom()` to call any new behavior while the screen is faded out.
- To avoid camera panning during room swaps: call `FindObjectOfType<CameraFollow2D>()?.SnapToTarget()` immediately after `LoadRoom()` while faded out.

---

If anything is unclear or you want stricter guardrails for AI edits (naming, tests, code formatting), tell me which areas to expand and I'll update this file.
