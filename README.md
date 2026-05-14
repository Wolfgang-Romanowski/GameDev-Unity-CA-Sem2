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

### Profiling evidence
See `Docs/CA3_ProfilingPack/` for the before/after captures (`CA3_Network_Profile_Before.png`, `CA3_Network_Profile_After.png`) and `ProfilingNotes.md` for the accompanying analysis. Profiling notes cover the guard perception tick-rate optimisation (64 Hz → 16 Hz) and the resulting frame time reduction.

---

## CA3 - AI for Games (Behaviour Tree Vertical Slice)

**Branch:** `feature/ca3-ai`
**Scene:** `Assets/Scenes/CA3.unity`
**Tag:** `ca3-submit-ai`

### Packages used
- AI Navigation (NavMesh) - guard movement
- TextMeshPro - debug overlay

### How to run
1. Open the project in Unity 6.3
2. Load `Assets/Scenes/CA3.unity`
3. Press Play
4. Press **F1** to toggle the debug overlay

### Controls
- **WASD** - move player
- **F1** - toggle debug overlay (BT node, Blackboard values, suspicion, LKP)

### Player objective
Reach the goal zone without being caught by the guard. The guard uses a Behaviour Tree (Patrol → Investigate → Chase → Search) driven by a suspicion accumulator. Sight and hearing feed the Blackboard; the BT selects branches based on suspicion thresholds.

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

## Attribution

### Networking and services
- **Photon Fusion 2** (v2.0.12) - Photon Engine GmbH. Free tier. https://www.photonengine.com/fusion
- **Unity Authentication SDK** - Unity Technologies. com.unity.services.authentication via Package Manager
- **Azure Functions (.NET 8 isolated worker)** - Microsoft Azure. CA3-AuthProxy project in `CA3-AuthProxy/`

### UI
- **TextMeshPro** - Unity Technologies. com.unity.textmeshpro via Package Manager

### Environment and characters (CA1 AI / CA3 AI scenes)
- **Abandoned Factory (Lite)** - Tirgames Assets. Unity Asset Store
- **Starter Assets - ThirdPerson** - Unity Technologies. Unity Asset Store

### AI and navigation
- **AI Navigation (NavMesh)** - Unity Technologies. com.unity.ai.navigation via Package Manager

### CA3 AI module - custom implementations
- Behaviour Tree framework (BTSelector, BTSequence, ConditionNode, ActionNode, CooldownDecorator, TimeoutDecorator, ConditionalAbortDecorator) - original implementation, Wolfgang Romanowski
- SuspicionSystem, GuardBT, GuardAI, GuardSensor, GuardMovement - original implementation, Wolfgang Romanowski

### CA1 Game Dev - shader
- **SG_A2_DissolveBurn_Lit** ShaderGraph - original implementation, Wolfgang Romanowski. Noise approach informed by Unity Shader Graph documentation

### AI assistance
Claude (Anthropic) was used during development for debugging assistance, assisted implementation of the networked guard NPC, and research support - particularly around Azure Functions setup and the Fusion Custom Authorization pipeline. All submitted code, scenes, and written work are my own.
