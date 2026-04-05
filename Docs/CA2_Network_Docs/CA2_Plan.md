# CA2 Plan

## Feature Scope
**Option B — Networked Pickup / Interaction**

Pickup orbs placed in the scene that any client can collect. When picked up the orb disappears on all clients and the collecting player's score goes up. The score is displayed via a TextMeshPro UI element created at runtime for the local player only.

## Why This Scope
I went with Option B because it has a clear pass/fail condition: either the orb disappears on both screens or it doesn't. It's easy to verify in a two-client setup and naturally requires authority checks and networked properties, which are the core things being assessed. I didn't go for projectiles (Option D) because hit detection and latency would have been too much to debug reliably. The rubric says a smaller correct feature beats a bigger broken one so I kept it tight.

## Sync Approach
I used `[Networked]` properties instead of RPCs. `IsPickedUp` is a NetworkBool on PickupItem that all clients read every frame in `Render()` to toggle the orb's renderer and collider. `Score` is an int on PlayerScore that the local player's UI reads in `Render()` to display the pickup count. Both are persistent state that needs to be available to any client at any time, including late joiners. An RPC would fire once and be missed by anyone who joins after, so [Networked] is the right tool here. RPCs would make sense for something transient like a pickup sound effect but I didn't have any transient effects in scope.

## Authority Model
The host holds StateAuthority over all scene-placed NetworkObjects including the pickup orbs. In `PickupItem.OnTriggerEnter()` the first line is `if (!Object.HasStateAuthority) return;` which ensures only the host processes pickups and writes to the [Networked] properties. This prevents both clients from processing the same collision and doubling the score.

Each client provides their own WASD input through the `NetworkInputData` struct which implements `INetworkInput`. The input is polled in `Spawner.OnInput()` and consumed in `PlayerMovement.FixedUpdateNetwork()` via `GetInput()`, which only returns data for the player that has InputAuthority over that object. Player spawning uses `Runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player)` where the fourth parameter assigns InputAuthority to the joining player.