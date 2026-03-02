# CA1 — Navigation + FSM Encounter

## Unity Version

Unity 6.3

## Packages

* AI Navigation (NavMesh)
* Input System
* TextMeshPro

## How to Run

Open the project in Unity 6.3, load the scene from `Assets/Scenes/` and press Play.

## Controls

- WASD to move
- Mouse to look around
- E to open the door when nearby
- F1 to toggle the debug overlay
- Space to restart after winning or losing

## Player Objective

The idea is to sneak past the guard and reach the goal zone at the end without getting caught. The guard patrols a set of waypoints and will investigate, chase, and search if it spots or hears you. There is a locked sliding door that creates a shortcut through the level but once you unlock it the guard can also use it so opening it is a bit of a risk/reward decision.

## Attribution

* Abandoned Factory (Lite) by Tirgames Assets for the environment and props
* Starter Assets - ThirdPerson by Unity Technologies for the third person character controller