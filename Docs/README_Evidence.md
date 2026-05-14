# CA3 AI - Evidence Index

This is the index of supporting evidence for the CA3 AI submission. Items are grouped by where they appear in the Reflective Document.

## State screenshots - Section 1 (Integrated technical account)

`Docs/Screenshots/CA3_AI_Screenshots/`
- `CA3_State_Patrol.png` - guard cycling waypoints; overlay shows `Node: Patrol`, low suspicion bar
- `CA3_State_Investigate.png` - guard walking to last-known-position; overlay shows `Node: Investigate`, suspicion in the 30-80% band
- `CA3_State_Chase.png` - guard pursuing player; overlay shows `Node: Chase`, high Sight Confidence
- `CA3_State_Search.png` - guard wandering near LKP after losing sight; overlay shows `Node: Search`, `IsSearching = True`

These four cover the four distinct BT branches in operation with the debug overlay active.

## Profiler captures - Section 2 (Performance and robustness evaluation)

`Docs/ProfilerCaptures/AI_Profiler/`
- `CA3_AI_Profile_Before.png` - baseline GC allocation pattern from vision cone mesh path
- `CA3_AI_Profile_After.png` - allocations eliminated by cached buffer pattern
- `CA3_AI_Profile_Fixed.png` - final submission-state capture

Interpretation in `Docs/CA3_ProfilingPack/ProfilingNotes.md`.

## CA2 evidence carried forward

`Docs/CA2_TechNote.md` - the CA2 technical note covering the BT framework, perception, and design trade-offs that underpin the CA3 AI work.

`Docs/CA2_CriticalDiscussion.md` - reflection on the design choices and the chase/search oscillation fix that fed directly into CA3 thresholds.

`Docs/CA2_Evidencelog.md` - timestamped CA2 development log.

`Docs/Screenshots/ca2_patrol.png`, `ca2_chase.png`, `ca2_search.png` - CA2-era captures retained as evidence of iterative development from CA2 to CA3.

## CA1 evidence carried forward

`Docs/TechNote.md` - the CA1 technical note on FSM transitions and the sliding-door navigation complication.

`Docs/Screenshots/patrol.png`, `chase.png`, `search.png`, `door.png` - CA1-era captures of the original FSM scene retained as the starting point for the CA3 work.

## Planning

`Docs/CA3_Plan.md` - the planning document committed alongside the `ca3-start` tag, documenting the intended scope, authority model (network side), branch strategy and tag plan. Some items in the plan (notably the combined-submission branching) were later revised; those revisions are discussed in Section 1 of the Reflective Document.

## Reflective Document and video

Reflective Document and video are submitted directly to Moodle; see the top-level `README.md` for context.

## Slides

Exported slide deck from the video presentation should be placed in `Docs/Slides/` if the file size is repository-friendly; otherwise reference the deck location in the Reflective Document.
