# CA1 / CA2 / CA3 - Advanced 3D Game Development + AI for Games

**Student:** Wolfgang Romanowski | **Student Number:** 20101931
**Unity version:** 6.3
**Repository:** https://github.com/Wolfgang-Romanowski/GameDev-Unity-CA-Sem2

---

## Commit message convention

| Prefix | Purpose |
|---|---|
| `feat:` | new feature |
| `fix:` | bug fix |
| `net:` | networking changes |
| `ai:` | AI, BT, or NavMesh changes |
| `perf:` | optimisation |
| `docs:` | documentation only |
| `chore:` | tooling or project config |
| `tweak:` | tuning / inspector adjustments |
| `refactor:` | code restructure without behaviour change |

---

## CA3 - AI for Games (Behaviour Tree Vertical Slice)

**Branch:** `feature/ca3-ai`
**Scene:** `Assets/Scenes/02_VerticalSlice/CA3.unity` *(module-standard location per CA3 handout §6)*
**Tag:** `ca3-submit-ai` *(applied at final submission)*

### Packages used
- AI Navigation (NavMesh) - guard pathfinding and dynamic obstacle carving
- TextMeshPro - debug overlay text rendering
- Input System - player movement and overlay toggle
- Universal Render Pipeline + Post Processing - cinematic lighting pass

### How to run
1. Open the project in Unity 6.3
2. Load `Assets/Scenes/02_VerticalSlice/CA3.unity`
3. Press Play
4. Press **F1** to toggle the debug overlay

### Controls
- **WASD** - move player
- **Mouse** - look around
- **E** - open the sliding door when near it
- **F1** - toggle debug overlay (active BT node, Blackboard values, Suspicion/Sight Confidence bars, current goal/LKP)

### Player objective
Reach the goal zone at the end of the level without being caught by the guard. The guard runs a four-branch Behaviour Tree (Patrol → Investigate → Chase → Search) driven by a suspicion accumulator. Two perception streams feed the Blackboard: hearing (proximity-based, velocity-gated so sneaking is silent) raises suspicion gradually; sight (raycast through a vision cone) raises a separate Sight Confidence value with hysteresis. The 30-80% suspicion band drives Investigate (walk to last-known-position, look around); >80% suspicion or >70% sight confidence triggers Chase. Confidence >0.3 keeps chase running through brief occlusions. A sliding door acts as a dynamic NavMesh obstacle - the player can unlock it as a shortcut, but the guard can also use it once unlocked.

### Architecture summary
- **Sensing layer**: `GuardSensor` runs each frame, doing LOS raycasts and hearing-range checks; results write to `Blackboard`. `SuspicionSystem` integrates these into a clamped suspicion float and a separate sight-confidence value.
- **Decision layer**: `GuardBT` builds a root Selector with Chase / Investigate / Search / Patrol branches. Decorators (`ConditionalAbort`, `Cooldown`, `Timeout`) gate each branch. Tree ticks at 10 Hz via a coroutine, not every frame.
- **Action layer**: `GuardMovement` wraps `NavMeshAgent` with stuck detection, path visualisation (LineRenderer recoloured per FSM state), repath thresholds, and a look-around rotation mode for Investigate's linger phase.
- **Debug overlay**: `DebugOverlay` renders the live BT node, Blackboard values, both perception bars, and the current goal in-scene. `GuardVisionCone` draws the sight cone (cached mesh buffers; no per-frame GC) and a translucent hearing ring on the floor.

### Profiling evidence
See `Docs/CA3_ProfilingPack/` for the before/after/fixed captures and `ProfilingNotes.md` for the accompanying analysis. The captures cover the vision-cone mesh allocation hotspot and the resulting GC reduction.

---

## CA3 - Advanced 3D Game Development (Networking Vertical Slice)

**Branch:** `feature/ca3-network`
**Scene:** `Assets/Scenes/CA3_Network.unity`
**Tag:** `ca3-submit` *(applied at final submission)*

### Packages used
- Photon Fusion 2 (v2.0.12) - Shared mode networking
- Unity Authentication SDK - anonymous sign-in
- TextMeshPro - score and winner UI
- AI Navigation (NavMesh) - guard pathfinding

### How to run
1. Open the project in Unity 6.3
2. Load `Assets/Scenes/CA3_Network.unity`
3. Press Play - Unity Authentication signs in automatically (anonymous, no credentials required)
4. For two-client testing: build a standalone Windows player (File → Build and Run), then press Play in the Editor. Both clients join the same Photon session automatically

### Controls
- **WASD** - move
- **Two clients join the same session** - Photon session name defaults to `CA3-Heist` (`sessionName` field in `NetworkBootstrap.cs` - Inspector-overridable)

### Player objective
Collect three pickup orbs before the other player. A guard NPC patrols the arena and will chase any player that enters its detection range - if caught, the player is stunned for 2 seconds. First player to reach 3 pickups wins.

### Authentication setup
Authentication runs automatically on Play. The pipeline is:
1. `AuthBootstrap.cs` calls `SignInAnonymouslyAsync()` via Unity Authentication SDK
2. On success, the access token is passed to Photon via `AuthenticationValues` with `CustomAuthenticationType.Custom`
3. Photon forwards the token to the Azure Function proxy (URL configured in the Photon App dashboard under Custom Server Provider)
4. The proxy validates the JWT issuer (`player-auth.services.api.unity.com`) and expiry, returns `ResultCode: 1` on success or `ResultCode: 2/3` on failure
5. Photon admits or refuses the client based on the proxy response

**Note:** The Azure Function and Photon App ID are active for submission. Both will be taken down after marking.

---

## CA1 - AI for Games (Navigation + FSM)

**Tag:** `ca1-submit` (AI module)
**Scene:** `Assets/Scenes/CA_Warehouse.unity`

### Packages used
- AI Navigation (NavMesh)
- Input System
- TextMeshPro

### How to run
Open the project in Unity 6.3, load `Assets/Scenes/CA_Warehouse.unity` and press Play.

### Controls
- **WASD** - move
- **Mouse** - look
- **E** - open door when nearby
- **F1** - toggle debug overlay
- **Space** - restart after win/lose

### Player objective
Reach the goal zone without being caught. The guard patrols waypoints and transitions through Investigate → Chase → Search states on detection. A sliding door creates a shortcut but opens the route for the guard too.

---

## CA1 - Advanced 3D Game Development (Rendering Foundations)

**Tag:** `ca1-submit`
**Scene:** `Assets/Scenes/02_VerticalSlice/CA1_RenderingFoundation.unity`

Dissolve/burn-out ShaderGraph effect, dusk lighting pass, and post-processing. See `Docs/` for the CA1 PDF report.

---

## CA2 - Advanced 3D Game Development (Networking + Version Control)

**Branch:** `feature/ca2-network` (merged to main)
**Tag:** `ca2-submit`
**Scene:** `Assets/Scenes/CA2_NetworkTest.unity`

Photon Fusion 2 Host/Client pickup feature. See `Docs/CA2_Network_Docs/` for plan, branch strategy, learning journal, test matrix, and Unreal replication note.

---

## Attribution summary
Full credits and licensing in `CREDITS.md`. Headline items:
- **Photon Fusion 2** (v2.0.12) - Photon Engine GmbH, free tier (CA2 / CA3 Network)
- **Unity Authentication SDK** - Unity Technologies (CA3 Network)
- **AI Navigation (NavMesh)** - Unity Technologies (all AI modules)
- **TextMeshPro / Input System** - Unity Technologies
- **Abandoned Factory (Lite)** - Tirgames Assets, Unity Asset Store (CA1 AI / CA3 AI scene)
- **Starter Assets - ThirdPerson** - Unity Technologies, Unity Asset Store (CA1 AI / CA3 AI player controller)
- All scripts in `Assets/Scripts/CA3_AI/`, `Assets/Scripts/CA2/`, `Assets/Scripts/CA1_AI/`, `Assets/Scripts/CA3_Networking/`, and the Azure proxy in `CA3-AuthProxy/` are original work by Wolfgang Romanowski

### AI assistance
Claude (Anthropic) was used during development for debugging assistance, assisted implementation work, and research support. All submitted code, scenes, and written work are my own.
