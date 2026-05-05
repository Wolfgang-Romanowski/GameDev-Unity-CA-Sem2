# CA1 / CA2 / CA3

## CA3 (Combined AI for Games and Advanced 3D Game Dev Submission)

**Unity version:** 6.3
**Networking:** Photon Fusion 2 (Shared mode)
**Authentication:** Unity Authentication SDK, Azure proxy, Fusion Custom Authorisation
**Working branch:** `feature/ca3-integration`

This single project satisfies both CA3 modules. See `Docs/CA3_Plan.md` for scope, authority model, and tag plan. Lecturer (G. De Francesco) confirmed that a combined submission is acceptable as long as both rubrics are met.

### Commit message convention
* `feat:` new feature
* `fix:` bug fix
* `net:` networking changes
* `ai:` AI, BT, or NavMesh changes
* `perf:` optimisation
* `docs:` documentation only
* `chore:` tooling or project config
* `refactor:` code restructure without behaviour change

---

## CA1 (Navigation and FSM Encounter)

### Packages
* AI Navigation (NavMesh)
* Input System
* TextMeshPro

### How to Run
Open the project in Unity 6.3, load the scene from `Assets/Scenes/` and press Play.

### Controls
* WASD to move
* Mouse to look around
* E to open the door when nearby
* F1 to toggle the debug overlay
* Space to restart after winning or losing

### Player Objective
The idea is to sneak past the guard and reach the goal zone at the end without getting caught. The guard patrols a set of waypoints and will investigate, chase, and search if it spots or hears you. There is a locked sliding door that creates a shortcut through the level but once you unlock it the guard can also use it so opening it is a bit of a risk/reward decision.

---

## CA2 (Networking and Version Control)

Photon Fusion 2 networked pickup feature. See `Docs/CA2_Network_Docs/` for the plan, branch strategy, learning journal, test matrix, and Unreal replication note.

---

## CA3 (How to Run)
*(to be completed when the vertical slice scene is finalised)*

## CA3 (Authentication Setup)
*(to be completed: Azure proxy endpoint, Fusion dashboard config, Unity Authentication setup)*

## Attribution
* Abandoned Factory (Lite) by Tirgames Assets for the environment and props
* Starter Assets - ThirdPerson by Unity Technologies for the third person character controller
* Photon Fusion 2 by Photon (Exit Games) for the networking framework
* TextMeshPro by Unity Technologies for text rendering
* *(CA3 additions to be listed: Unity Authentication, any Azure Functions templates, BT framework references)*