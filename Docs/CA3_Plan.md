\## Combined submission



This single Unity project should satisfy both CA3 AI for Games and

CA3 Advanced 3D Game Dev.



\## Scope

Co-op heist vertical slice. Two networked players (Photon Fusion 2

Shared mode) collect 3 pickups while evading a single BT-driven

guard NPC, then reach an extraction zone. Catch ends round in loss.

Full extraction with all pickups collected ends round in win.



\## Systems carried forward from CA1 / CA2

\- AI CA1: GuardMovement (NavMesh wrapper, stuck detection,

&#x20; path-line visualisation)

\- AI CA2: BT framework (Selector / Sequence / Decorators / Blackboard),

&#x20; SuspicionSystem, vision + hearing perception, debug overlay

\- GameDev CA2: PickupItem, PlayerScore, Spawner — currently

&#x20; Host/Client mode. To be refactored to Shared mode for CA3.



\## Systems to add for CA3

\- Refactor networking from Host/Client to Fusion 2 Shared mode

\- Unity Authentication SDK + Azure proxy + Fusion Custom Authorisation,

&#x20; with demonstrable rejection of unauthenticated clients

\- Networked guard awareness: sight checks against both players,

&#x20; guard state replicated so both clients see consistent BT branch

&#x20; and vision-cone colour

\- Win/lose UI driven by networked game state

\- Fix CA1 catch bug: clean round-end on both clients via

&#x20; proper UI handoff rather than Time.timeScale freeze



\## Authority model (Shared mode)

\- Each player: StateAuthority over own character (movement, look)

\- Guard: StateAuthority on master client; replicates position

&#x20; and BT state enum so cone colour is consistent across clients

\- Pickups: StateAuthority transfers to collecting player on trigger;

&#x20; IsCollected \[Networked] bool flips; master client increments

&#x20; shared score via RPC

\- Score: master client holds, drives win condition; replicated

&#x20; to all clients via \[Networked] property



\## Branch strategy

\- main: stable baseline only (CA2 final + Fusion restore)

\- feature/ca3-integration: all CA3 work

\- Merge to main at ca3-submit



\## Tag plan

\- ca3-start: branch creation. Note: applied retrospectively. The

&#x20; compressed CA3 timeline will be discussed honestly in the

&#x20; reflective document, consistent with the approach the CA2 marker

&#x20; rewarded.

\- ca3-alpha: auth + Shared mode session + guard in scene

\- ca3-beta: heist loop runs end-to-end on two clients

\- ca3-profiling-pack: profiler captures committed

\- ca3-submit: final

