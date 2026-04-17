# Scripts — Context

## Player Movement (`PlayerMovement.cs`)
- Class: `PlayerMovementAdvanced` (Rigidbody, `rb.useGravity = false`)
- States: `idling | walking | sprinting | crouching | air | ledgeGrab`
- Ground check: `Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround)`
- Coyote time: 0.15s window post-ground-leave for jump
- Gravity: `ApplyCustomGravity()` — only fires when airborne, adds `(fallMultiplier-1) * gravity`
- Lock: `LockMovement(bool)` — also subscribes to `GameEvents.OnPlayerMovementLockChanged`

## Camera (`ThirdPersonCam.cs`)
- Reads legacy `Input.GetAxis` for orientation
- Locks via `GameEvents.OnCameraLockChanged` and `OnPlayerControlsLockChanged`
- `DirectionFaceZip(bool)` — rotates player toward zipline

## Event Bus (`GameEvents.cs`)
- `TriggerPlayerMovementLock(bool)` — freezes player horizontal velocity
- `TriggerCameraLock(bool)` — freezes camera rotation
- `TriggerPlayerControlsLock(bool)` — both at once

## Audio (`Sound_Music.cs`)
- Tag: `"Audio"` — retrieved via `GameObject.FindGameObjectWithTag`
- Footsteps use timed cooldown (walk: 0.5s, sprint: 0.25s)

## Input
- `PlayerMovement.cs` uses legacy `Input.GetAxisRaw` / `Input.GetKey`
- `Movement_builtin.cs` uses New Input System callbacks (alternative, not active)
