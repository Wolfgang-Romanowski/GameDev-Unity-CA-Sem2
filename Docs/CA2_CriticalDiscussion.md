# Critical Discussion

## Design Choices

The main decision was using a flat priority Selector for the root rather than something more complex. I considered utility scoring where each branch competes on a numeric value but rejected it because transitions become harder to predict and debug. If a marker is watching the overlay they should be able to explain every state change immediately and utility scoring makes that much less obvious. I also thought about nested sub-trees where chase and search would be grouped into a pursuit cluster but for a single guard with four behaviours the extra abstraction felt like complexity for the sake of it. The flat Selector makes priority explicit and you can read it straight from the code.

Keeping perception separate from the BT was deliberate. GuardSensor writes raw detection to the Blackboard and SuspicionSystem processes it into a suspicion float and the BT just reads thresholds. I tried embedding raycasts directly inside LeafChase early on but that made it impossible to test chase logic independently of the sensor. Pulling perception out means I could swap the entire sensor without touching BT code.

The locked goal in LeafChase came from a real problem. When the guard lost sight but could still hear the player the hearing updates kept shifting the destination while the guard was mid-path around a wall. The NavMesh would recalculate and sometimes pick a completely different route so the guard flipped between going left and going right and barely made progress. Locking the destination forces commitment to one path. I assumed continuous updates would be better but in practice they made things worse because pathfinding and BT ticks operate on different timescales.

## Lessons Learned

The thing that surprised me most was how much the NavMeshAgent defaults affected how the AI felt. With the default acceleration of 8 and autoBraking on the guard looked sluggish even though the BT was correct. Every SetDestination triggered a slow down and turn and speed up cycle and with a moving player causing repaths every few ticks the guard was permanently decelerating. Setting acceleration to 40 and angular speed to 360 and turning off autoBraking made the same logic feel responsive. AI tuning is not just the decision layer. The movement parameters matter just as much.

The chase search oscillation took the longest to fix. Hearing kept pushing suspicion past the chase threshold during Search which pulled the guard into Chase where it would fail and return to Search. I tried cooldown timers and suspicion multipliers before realising the root cause was simpler. The chase condition just needed an IsSearching check to block suspicion only re-entry while still letting sight override.

## Carry-Forward to CA3

For CA3 I want to refactor the BT into composable sub-trees so new behaviours can be added without editing GuardBT. I would extract the chase and search cycle into a reusable pursuit sub-tree that different NPC types could share with different parameters. I also want to replace distance based hearing with a noise event system where player actions like footsteps and door interactions emit sound events with position and volume. This would give the guard directional information from hearing rather than just proximity awareness which would make investigation behaviour much more believable.
