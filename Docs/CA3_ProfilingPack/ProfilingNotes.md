# CA3 AI - Profiling Notes

## Captures
Located in `Docs/ProfilerCaptures/AI_Profiler/`:
- `CA3_AI_Profile_Before.png` - baseline state, vision cone reallocating its mesh buffers each frame
- `CA3_AI_Profile_After.png` - after cached-buffer fix, vision cone holds onto its arrays between frames
- `CA3_AI_Profile_Fixed.png` - final state after subsequent hearing-gate tuning and BT tick-rate verification, used as the reference for the submission build

## What was measured
The Unity Profiler was run in Play Mode with the player active and the guard NPC running its full Patrol / Investigate / Chase / Search loop. The headline concern was `GuardVisionCone.DrawVisionCone()`, which fires in `LateUpdate` every frame and originally allocated two arrays inside the method body:

```csharp
Vector3[] vertices  = new Vector3[rayCount + 2];
int[]     triangles = new int[rayCount * 3];
```

With `rayCount = 80` (configurable; rubric range is 10-80), that is 82 `Vector3` plus 240 `int` allocated 60+ times per second. The before capture shows this as a steady GC Allocated In Frame stripe with regular spikes whenever the generational collector runs to reclaim them. Beyond the GC pressure, the cone also fires 240 raycasts per frame (`rayCount * HeightSteps`); this cost is acknowledged in the script tooltip and is acceptable for a single guard but would need LOD culling at scale.

A secondary concern was the BT update path: ticking the whole tree on every `Update` would compound the cone cost. This was already gated by a coroutine running at `btTickRate = 0.1f` (10 Hz) and verified during profiling, so it did not need changing.

## What was changed
**Primary fix** - `GuardVisionCone` was reworked to keep its mesh buffers as fields and only reallocate when `rayCount` actually changes (e.g. Inspector tweak). The relevant change in `DrawVisionCone`:

```csharp
if (coneVertices == null || lastRayCount != rayCount)
{
    coneVertices  = new Vector3[rayCount + 2];
    coneTriangles = new int[rayCount * 3];
    lastRayCount  = rayCount;
}
```

This removes the per-frame allocation entirely. The mesh is still rebuilt every frame (it has to be - the cone geometry depends on raycast hit distances), but it is now rebuilt into preallocated arrays rather than fresh ones.

**Secondary tuning** - velocity-aware hearing was added to `GuardSensor` so that a stationary or sneaking player produces no hearing events. Below the `hearingSpeedThreshold` (1.5 m/s default) the hearing check is suppressed, which trimmed the rate at which `SuspicionSystem` was performing its accumulate/decay work even though the saving here is marginal compared to the cone fix.

**Tick-rate management decisions** - the BT runs at 10 Hz via coroutine; the sensor runs every frame because perception responsiveness matters more than its cost; the vision cone mesh rebuild runs every frame because it follows the guard's rotation; the floor-finding raycast in `GuardVisionCone` runs every frame but is a single raycast.

## Outcome
The after and fixed captures show the GC Allocated In Frame stripe for the cone path replaced with a flat line - the cone is no longer a per-frame allocator. The remaining GC noise visible in the fixed capture is incidental work elsewhere in the engine and not part of the AI stack.

Frame time stays stable in editor and standalone build at the target framerate. The cone is still the most expensive single component in the AI stack (240 raycasts per frame is genuinely the cost it advertises), but it is no longer the GC hotspot it was.

## Edge-case robustness encountered during profiling
- **LKP off-mesh**: investigation could fail instantly when the player's transform sat just below the NavMesh surface. Resolved by snapping `LastKnownPosition` through `NavMesh.SamplePosition` inside `GuardSensor` before writing it to the Blackboard.
- **Path resolution latency**: a fresh `SetDestination` does not always produce `HasValidPath == true` on the next tick. Resolved with a 1-second grace window in `LeafInvestigate` before declaring path failure, preventing premature Patrol fallback.
- **Stunned-target loop (network module, kept here as note)**: a separate but related profiling observation - the network guard's catch logic refreshed the stun deadline on every catch cooldown cycle. Fixed by guarding `CatchPlayer` against re-stunning an already-stunned target and forcing `ReturnToPatrol` after a successful catch.
