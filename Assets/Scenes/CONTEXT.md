# Scenes — Context

| Scene | Purpose |
|---|---|
| `JEU.unity` | Main game / active development scene |
| `DebutNiveau.unity` | Level 1 |
| `DeuxiemeNiveau.unity` | Level 2 |
| `TroisiemeNiveau.prefab` | Level 3 (prefab-based) |
| `Plateform.unity` | Platform mechanics test |
| `ZipImproving.unity` | Zipline test scene |
| `checkpoint.unity` | Checkpoint system test |
| `AnimationSInge.unity` | Animation preview |
| `FixCam.unity` | Camera tuning |

## Level Structure
Each level uses `Assets/Prefab/Monde/` prefabs:
- `Setting de base.prefab` — base level setup
- `Niveaux/` — level-specific prefabs
- `Elements/` — reusable world objects
