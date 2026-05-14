# Credits and Attribution

## Third-party packages and SDKs

**Photon Fusion 2** (v2.0.12). Photon Engine GmbH. Free tier licence. Used for all networked gameplay in CA2 and CA3 including session management, NetworkObject spawning, state replication and RPC routing. https://www.photonengine.com/fusion

**Unity Authentication SDK** (com.unity.services.authentication). Unity Technologies. Installed via Package Manager. Used in CA3 for anonymous sign-in and JWT access token retrieval before session join.

**AI Navigation** (com.unity.ai.navigation). Unity Technologies. Installed via Package Manager. Used across CA1 AI, CA2 AI and CA3 Network for NavMesh baking and NavMeshAgent pathfinding.

**TextMeshPro** (com.unity.textmeshpro). Unity Technologies. Installed via Package Manager. Used for all in-game UI text across all submissions.

**Input System** (com.unity.inputsystem). Unity Technologies. Installed via Package Manager. Used in CA1 AI for player input handling.

## Azure infrastructure

**CA3-AuthProxy**. Custom .NET 8 isolated Azure Functions project authored by Wolfgang Romanowski. Deployed to Azure App Service (`func-ca3-auth-t10alp`). Validates Unity Authentication JWTs for Photon Custom Authorization. Source is in the `CA3-AuthProxy/` folder of this repository.

Azure Functions worker and ASP.NET Core hosting packages (Microsoft.Azure.Functions.Worker and Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore). Microsoft. Used under standard NuGet licence terms.

## Environment and character assets

**Abandoned Factory (Lite)**. Tirgames Assets. Acquired from the Unity Asset Store under the standard Asset Store licence. Used in the CA1 AI and CA3 AI scenes for environment geometry and props.

**Starter Assets ThirdPerson**. Unity Technologies. Acquired from the Unity Asset Store under the standard Asset Store licence. Used as the basis for the third-person player controller in the CA1 AI scene.

## Original work

All scripts in `Assets/Scripts/CA3_Networking/` are original work by Wolfgang Romanowski, written for this submission. This includes `AuthBootstrap`, `NetworkBootstrap`, `NetworkGameManager`, `NetworkGuard`, `NetworkGuardVisuals`, `NetworkPickup`, `NetworkPlayerMovement`, `NetworkPlayerScore`, `NetworkPlayerStun`, and `NetworkPlayerAppearance`.

All scripts in `Assets/Scripts/CA3_AI/` are original work by Wolfgang Romanowski. This includes the custom Behaviour Tree framework (`BTSelector`, `BTSequence`, `ConditionNode`, `LeafPatrol`, `LeafInvestigate`, `LeafChase`, `LeafSearch`, `CooldownDecorator`, `TimeoutDecorator`, `ConditionalAbortDecorator`), `SuspicionSystem`, `GuardBT`, `GuardAI`, `GuardSensor`, `GuardMovement`, and `GuardVisionCone`. The BT node architecture was informed by reading the Behaviour Trees chapter of *Game AI Pro* (edited by Steve Rabin) but the implementation is entirely original.

The `SG_A2_DissolveBurn_Lit` ShaderGraph and the C# dissolve controller used in the CA1 Game Dev submission are original work. The noise-based threshold approach was informed by Unity's Shader Graph documentation.

## AI assistance

Claude (Anthropic) was used during development as a debugging aid and research assistant. This covered tracing authority model issues in the CA3 networking code, discussing NavMesh integration patterns with Fusion's Shared mode authority model and working through Azure Functions setup and the Photon Custom Authorization pipeline. All submitted code, scene content and written work are my own.
