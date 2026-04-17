# Platforms — Context

## Scripts
| File | Class | Behavior |
|---|---|---|
| `MovingPlatforme.cs` | `MovingPlatforme` | Moves A→B via `Vector3.MoveTowards`, coroutine loop |
| `Ascenseur.cs` | `Ascenseur` | Elevator variant, same pattern |
| `PlatformCollision.cs` | `PlatformCollision` | Detects player on platform, exposes `GetPlatformVelocity()` |
| `PlateformeChute.cs` | — | Falling platform trigger |
| `SpiningCylinder.cs` | — | Rotating obstacle |
| `Orbit/` | — | Orbital platform movement |
| `Piston/` | — | Piston-style push mechanic |

## Key Pattern
`PlatformCollision` is non-invasive — it doesn't touch the player directly. Other systems query `GetPlatformVelocity()` and `IsPlayerOnPlatform()` to apply platform velocity to the player.
