# Zipline — Context

## Files
- `ZiplinePlayer.cs` — detects nearby ziplines via `Physics.OverlapSphere`, shows UI prompt, triggers start/end
- `Zipline.cs` — executes movement: creates temp Rigidbody, uses `AddForce(dir * zipSpeed, ForceMode.Acceleration)`

## Flow
1. `ZiplinePlayer.CheckForZipline()` detects zipline → shows UI prompt
2. Player input → `StartZiplineAnimation()` → calls `GameEvents.TriggerPlayerControlsLock(true)`
3. `Zipline.StartZipline(player)` — moves player along path
4. On arrive: `ResetZipline()` → `GameEvents.TriggerPlayerControlsLock(false)`

## Key Detail
- Player parented to zipline point during traversal: `localZip.position + Vector3.down * verticalOffset`
- Rope rendered with `LineRenderer`
- Player Y-rotation only aligned toward destination
