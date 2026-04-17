# CLAUDE.md — UnityE3Project

## Project
3D platformer in Unity (URP). Player is a monkey character (SINGE). 3 levels + menus.

## Stack
- Unity URP, C#, New Input System (`Assets/Settings/InputSystem_Actions.cs`)
- Rigidbody-based player physics (`Assets/Scripts/PlayerMovement.cs`)
- KCC asset available at `Assets/KinematicCharacterController/` (not yet integrated)
- Dialogue via DialogueEditor (`Assets/Import/DialogueEditor/`)

## Key Scripts
| Script | Purpose |
|---|---|
| `Assets/Scripts/PlayerMovement.cs` | `PlayerMovementAdvanced` — main player controller |
| `Assets/Scripts/ThirdPersonCam.cs` | Camera + input orientation |
| `Assets/Scripts/GameEvents.cs` | Global event bus (movement lock, camera lock) |
| `Assets/Scripts/Zipline/` | Zipline traversal system |
| `Assets/Scripts/platforme/` | Moving platforms, elevators, pistons |
| `Assets/Scripts/Menu/` | Pause menu, checkpoints, data persistence |

## Patterns
- Movement locking via `GameEvents.TriggerPlayerMovementLock(bool)` — not direct calls
- Never use `Debug.Log` in Update() — causes per-frame spam
- Ground check: `Physics.CheckSphere` with `whatIsGround` LayerMask

## Context Files
Load only what you need:
- `Assets/Scripts/CONTEXT.md` — script architecture detail
- `Assets/Scripts/platforme/CONTEXT.md` — platform mechanics
- `Assets/Scripts/Zipline/CONTEXT.md` — zipline system
- `Assets/Scripts/Menu/CONTEXT.md` — UI/menu system
- `Assets/KinematicCharacterController/CONTEXT.md` — KCC integration notes
- `Assets/Scenes/CONTEXT.md` — scene inventory
