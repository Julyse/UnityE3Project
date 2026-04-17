# KinematicCharacterController — Context

Asset by Philippe St-Amand. Already imported at `Assets/KinematicCharacterController/`.

## Integration Pattern
1. Add `KinematicCharacterMotor` component to player GameObject
2. Implement `ICharacterController` interface in a new script (replaces `PlayerMovementAdvanced`)
3. Key callbacks: `UpdateVelocity()`, `UpdateRotation()`, `IsColliderValidForCollisions()`

## Folders
- `Core/` — `KinematicCharacterMotor.cs`, interfaces
- `ExampleCharacter/` — reference implementation to copy from
- `Examples/` — demo scenes showing usage

## Migration Notes
- Current `PlayerMovementAdvanced` uses `rb.AddForce` — KCC replaces this with direct velocity assignment in `UpdateVelocity()`
- Event bus (`GameEvents`) movement lock still applies — just zero velocity in the callback
- Zipline system uses a temporary Rigidbody; needs rework if player uses KCC
