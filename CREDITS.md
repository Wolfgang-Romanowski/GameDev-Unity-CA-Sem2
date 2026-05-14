# Credits and Attribution

## Third-party packages and SDKs

**Photon Fusion 2** (v2.0.12). Photon Engine GmbH. Free tier licence. Used for all networked gameplay in CA2 and CA3 Network including session management, NetworkObject spawning, state replication and RPC routing. https://www.photonengine.com/fusion

**Unity Authentication SDK** (com.unity.services.authentication). Unity Technologies. Installed via Package Manager. Used in CA3 Network for anonymous sign-in and JWT access token retrieval before session join.

**AI Navigation** (com.unity.ai.navigation). Unity Technologies. Installed via Package Manager. Used across CA1 AI, CA2 AI and CA3 AI / CA3 Network for NavMesh baking and NavMeshAgent pathfinding.

**TextMeshPro** (com.unity.textmeshpro). Unity Technologies. Installed via Package Manager. Used for all in-game UI text across all submissions.

**Input System** (com.unity.inputsystem). Unity Technologies. Installed via Package Manager. Used in CA1 AI and CA3 AI for player input handling and debug overlay toggle.

**Universal Render Pipeline** (com.unity.render-pipelines.universal). Unity Technologies. Installed via Package Manager. Used in CA3 AI for the cinematic lighting and post-processing pass (ACES tonemapping, bloom, vignette).

## Azure infrastructure

**CA3-AuthProxy**. Custom .NET 8 isolated Azure Functions project authored by Wolfgang Romanowski. Deployed to Azure App Service (`func-ca3-auth-t10alp`). Validates Unity Authentication JWTs for Photon Custom Authorization. Source is in the `CA3-AuthProxy/` folder of this repository.

Azure Functions worker and ASP.NET Core hosting packages (Microsoft.Azure.Functions.Worker and Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore). Microsoft. Used under standard NuGet licence terms.

## Environment and character assets

**Abandoned Factory (Lite)**. Tirgames Assets. Acquired from the Unity Asset Store under the standard Asset Store licence. Used in the CA1 AI and CA3 AI scenes for environment geometry and props.

**Starter Assets ThirdPerson**. Unity Technologies. Acquired from the Unity Asset Store under the standard Asset Store licence. Used as the basis for the third-person player controller in the CA1 AI and CA3 AI scenes.

## Original work

### CA3 AI module
All AI gameplay scripts are original work by Wolfgang Romanowski.

`Assets/Scripts/CA3_AI/` (scripts edited or added for CA3):
- `Blackboard` - shared data store keyed off by perception/BT
- `BTDecorator` - `CooldownDecorator`, `TimeoutDecorator`, `ConditionalAbortDecorator`
- `DebugOverlay` - in-scene FSM/BT/Blackboard inspector with Suspicion + Sight Confidence bars
- `GuardBT` - root Selector composition and coroutine-driven tick loop
- `GuardMovement` - NavMeshAgent wrapper with stuck detection, look-around rotation, path-line visualisation
- `GuardSensor` - line-of-sight raycasting, hearing range with velocity gate, sight confidence accumulator
- `GuardVisionCone` - cached vision cone mesh + translucent hearing ring (floor-snapped)
- `LeafChase`, `LeafInvestigate`, `LeafSearch` - BT action leaves with their own internal state machines

`Assets/Scripts/CA2/` (CA2 framework reused unchanged, original work by Wolfgang Romanowski):
- `BTNode` (base class + `ConditionNode`)
- `BTComposite` (`BTSequence`, `BTSelector`)
- `LeafPatrol` - waypoint cycle leaf
- `GuardAI` - lightweight FSM state enum + colour mapping
- `suspicionSystem.cs` (`SuspicionSystem`) - sight/hearing accumulator with decay

`Assets/Scripts/CA1_AI/` (CA1 supporting scripts, original work):
- `GameUI`, `GoalZone`, `SlidingDoor`, `AI/GuardCatch`

The BT node architecture was informed by reading the Behaviour Trees chapter of *Game AI Pro* (edited by Steve Rabin) but the implementation is entirely original.

### CA3 Network module
All scripts in `Assets/Scripts/CA3_Networking/` are original work by Wolfgang Romanowski. This includes `AuthBootstrap`, `NetworkBootstrap`, `NetworkGameManager`, `NetworkGuard`, `NetworkGuardVisuals`, `NetworkPickup`, `NetworkPlayerMovement`, `NetworkPlayerScore`, `NetworkPlayerStun`, and `NetworkPlayerAppearance`.

### CA1 Game Dev module
The `SG_A2_DissolveBurn_Lit` ShaderGraph and the C# dissolve controller used in the CA1 Game Dev submission are original work. The noise-based threshold approach was informed by Unity's Shader Graph documentation.

## AI assistance

Claude (Anthropic) was used during development as a debugging aid and research assistant. On the AI module this covered tracing condition-decorator composition issues, debugging NavMesh path resolution timing, and discussing perception-stream separation patterns. On the network module this covered tracing authority model issues, NavMesh integration patterns with Fusion's Shared mode authority model, and Azure Functions setup for the Photon Custom Authorization pipeline. All submitted code, scene content and written work are my own.
