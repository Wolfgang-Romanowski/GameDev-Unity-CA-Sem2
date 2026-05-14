# CA3 AI - Profiling Notes

## Captures
Located in `Docs/ProfilerCaptures/AI_Profiler/`:
- `CA3_AI_Profile_Before.png` - baseline state, vision cone reallocating its mesh buffers each frame
- `CA3_AI_Profile_After.png` - intermediate state, allocation still present but smoothed
- `CA3_AI_Profile_Fixed.png` - final state with the cached buffer pattern, zero allocation

## What was measured
The Unity Profiler was run in Play Mode with the player active and the guard NPC running its full Patrol / Investigate / Chase / Search loop. The headline concern was `GuardVisionCone.LateUpdate()` (which invokes `DrawVisionCone()`). In the unoptimised state the method body allocated two arrays every frame:

```csharp
Vector3[] vertices  = new Vector3[rayCount + 2];   // 82 elements at rayCount=80
int[]     triangles = new int[rayCount * 3];       // 240 elements at rayCount=80
```

With the BT ticking at 10 Hz via coroutine but the cone redrawing every `LateUpdate` at frame rate, this allocation pattern was the dominant per-frame GC contributor in the AI stack. The baseline capture shows a frame with `GuardVisionCone.LateUpdate` consuming **1.3 KB GC Alloc** per call and a visible **21.28 ms GC pause spike** in the CPU trace, pushing that frame to 23.34 ms total. Stable frames in the same baseline capture sit around the 16 ms (60 fps) line, so the spike is a single-collection event rather than continuous pressure - but the allocation is what feeds it.

The 240 raycasts per frame (`rayCount * HeightSteps`, where `HeightSteps = 3` so the cone clips correctly against walls of varying height) are a deliberate quality-cost trade-off and were left as is. The tooltip on `rayCount` explicitly acknowledges that LOD culling would be required at scale.

## What was changed
`GuardVisionCone` was reworked to keep its mesh buffers as fields and reallocate only when `rayCount` actually changes (Inspector tweak):

```csharp
if (coneVertices == null || lastRayCount != rayCount)
{
    coneVertices  = new Vector3[rayCount + 2];
    coneTriangles = new int[rayCount * 3];
    lastRayCount  = rayCount;
}

coneVertices[0] = Vector3.zero;
// ... populate the cached arrays directly ...
mesh.vertices  = coneVertices;
mesh.triangles = coneTriangles;
```

The mesh is still rebuilt every frame (cone geometry depends on raycast hit distances, which change with guard rotation), but it is now rebuilt into preallocated arrays rather than fresh ones.

## Outcome
The Fixed capture shows `GuardVisionCone.LateUpdate` at **0 B GC Alloc** per call, **0.14 ms time / 0.09 ms self**, total 3.9% of the CPU frame. The GC pause spikes in the baseline trace no longer correlate with cone updates. Frame time settles around 3.67 ms (≈270 fps headroom) in editor.

| Metric | Before (baseline) | Fixed (cached buffers) |
|---|---|---|
| GC Alloc per cone update | 1.0–1.3 KB | **0 B** |
| Cone `Self ms` per call | 0.08–0.12 ms | 0.09 ms |
| Frame time around cone call | Spike to 21+ ms during GC | Stable, no spike attributable to cone |
| Steady-state CPU | 3.47 ms | 3.67 ms |

The per-call time is essentially unchanged (the work was always cheap; the allocation was the problem). What changed is that the cone no longer feeds the GC, so collection pauses caused by cone allocation no longer appear in the CPU trace.

## Tick-rate management
Justifications committed in code and tooltip:
- **BT**: 10 Hz coroutine - decision-making does not need frame-rate granularity, and the BT writes the active-node string only.
- **Sensor**: every frame - perception responsiveness matters; the LOS raycast is one shot per frame.
- **Vision cone mesh rebuild**: every frame - geometry is rotation-dependent; this is the cost the profiling measured.
- **Floor-snap raycast** (cone + hearing ring positioning): every frame - one raycast, negligible cost.

## Edge-case robustness encountered during development
- **LKP off-mesh**: investigation could fail instantly when the player's transform sat fractionally below the NavMesh surface. `agent.SetDestination` would accept the off-mesh point and pathing returned `PathPartial`. Resolved by snapping `LastKnownPosition` through `NavMesh.SamplePosition` inside `GuardSensor` before writing it to the Blackboard.
- **Path resolution latency**: even after the snap fix, `HasValidPath` could return false on the BT tick immediately after `SetDestination`. Resolved with a 1-second `pathWaitDeadline` grace window in `LeafInvestigate` before declaring path failure, preventing premature Patrol fallback.
- **Investigate gate starvation**: with `CooldownDecorator` outside `ConditionalAbortDecorator`, every below-threshold suspicion tick was arming the cooldown and locking Investigate out for the next 5 seconds. Resolved by inverting the wrap order so the condition is checked first and the cooldown only ticks (and only arms) when the condition is true.
