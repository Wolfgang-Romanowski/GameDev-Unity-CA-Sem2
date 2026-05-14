# CA3 Network - Profiling Notes

## Captures
- `CA3_Network_Profile_Before.png` - baseline, guard perception running every tick
- `CA3_Network_Profile_After.png` - optimised, perception throttled to every 4th tick

## What was measured
The Unity Profiler was run in Play Mode with a single client active and the guard NPC in its patrol/chase loop. The primary concern was `NetworkGuard.FixedUpdateNetwork()`, which at baseline called `FindClosestPlayerInRange()` on every Fusion tick (64 Hz). This method iterates `Runner.ActivePlayers`, calls `Vector3.Distance` per player, and accesses the `NetworkPlayerStun` component - generating per-frame allocations visible as dense orange spikes in the GC Allocated in Frame row of the before capture.

## What was changed
A `perceptionTickInterval` field (default 4, exposed as `[SerializeField]`) was added to `NetworkGuard`. The perception scan now runs once every four ticks (16 Hz effective rate). Between scans, the guard retains its current `currentTarget` reference so active chase is not interrupted on off-ticks. The `agent.SetDestination` call continues every tick to ensure smooth pathfinding updates toward the confirmed target.

## Outcome
The before capture shows CPU frame time spiking to 66ms (15fps worst case) with a dense GC allocation pattern throughout the session. The after capture shows CPU frame time settling around 5ms (200fps equivalent) with the allocation spikes largely eliminated. The perceptual quality of guard detection is unchanged - at chase speed (5 m/s) a player moves approximately 0.31 m between perception ticks at 16 Hz, which is below the detection threshold granularity. The scene sustains above 60fps in both editor and standalone build with two active clients.
