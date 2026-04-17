# Menu / UI — Context

## Structure
```
Menu/
├── Pause/
│   ├── Pause_Menu.cs       — pause/unpause logic
│   ├── Checkpoint.cs       — checkpoint save/restore
│   ├── Données manager.cs  — data persistence (settings, progress)
│   ├── Niveaux_detection.cs — current level detection
│   ├── TriggerStart.cs     — level start trigger
│   ├── TriggerFinish.cs    — level finish trigger
│   ├── Toggles.cs          — settings toggles (audio, contrast)
│   └── Contrast.cs         — visual contrast setting
└── Son/                    — audio-related UI scripts
```

## Checkpoint System
- `BananaCheckpoint.cs` (in Scripts root) — collectible banana checkpoints
- `Checkpoint.cs` in Pause/ — save/restore player position
- Checkpoint prefab: `Assets/Prefab/Monde/CHECKPOINTS (1).prefab`

## Data Persistence
- `Données manager.cs` stores settings between sessions
