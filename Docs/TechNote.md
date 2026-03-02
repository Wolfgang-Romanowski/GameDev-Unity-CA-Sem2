# FSM States and Transitions

The guard NPC uses four states: patrol, Investigate, Chase, and Search. Each state has clear entry conditions and the FSM never changes state without a specific guard being met.

**Patrol:** Guard cycles through a set of waypoints. If the guard sees the player (line of sight check within a cone angle and range) it transitions to Investigate and records the player position as the last known position.

**Investigate:** the guard moves toward the last known position at patrol speed. If it gets a confirmed visual on the player during this approach it then goes to Chase. If it reaches the position or the investigate timer runs out without seeing the player again it will return back to patrol.

**Chase:** the guard moves at full speed toward the player. It continuously updates the last known position while the player is still visible and when line of sight breaks a lose sight timer starts counting down. If the timer expires without regaining sight, the guard transitions to it's search mode. this delay prevents the guard from giving up instantly when the player ducks behind cover briefly.

**Search:** the guard wanders to random valid NavMesh points near the last known position but if it spots the player again it reenters chase and if the search timer runs out it returns to Patrol.

There is one global interrupt: if the player enters hearing range the guard immediately transitions to Chase from any other state it might have so this makes sure that the player cannot stand behind the guard undetected at close range.

For the sake of robustness the sensor periodically reacquires the player reference if it becomes null and the movement component tracks whether the agent is stuck. If stuck for more than 3 seconds the FSM either skips to the next waypoint (during Patrol) or falls back to Patrol (during Search or Investigate).

# Navigation Approach

The project uses Unity NavMesh system with a NavMeshAgent on the guard. The main navigation complication is a sliding door that acts as a dynamic obstacle. Before the player unlocks the door a NavMeshObstacle with carving enabled blocks that route entirely, forcing the guard to path around. Once the player presses E to unlock the door the obstacle is disabled and the NavMesh updates to include the doorway. The guards path is then recalculated allowing it to use the shorter route through the door. After unlocking the guard can also open the door on its own by approaching it which means the players decision to unlock a shortcut also benefits the guard.

The guard also uses a repath threshold during Chase so it only recalculates its path when the player moves a meaningful distance avoiding unnecessary SetDestination calls every frame.

# Design Trade-Off

I chose not to implement a separate state for when the guard catches the player as i didn't think it would make sense so instead i added a simple distance check on the GuardCatch component triggers the lose condition through the GameUI. This keeps the FSM focused purely on navigation and detection behaviour rather than mixing in game over logic too as for the sake of the demo I just didn't think it was necessary for anything more extravagant. The trade off is that the guard has no dedicated "capture" behaviour but it keeps the FSM cleaner and avoids adding a state that would only run for a single frame before the game freezes.