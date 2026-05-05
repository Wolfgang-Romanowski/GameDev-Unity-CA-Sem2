# BT Structure

The guard runs on a custom Behaviour Tree that ticks every 0.1 seconds. The root is a Selector with four branches evaluated in priority order.

**Chase** is the highest priority and is wrapped in a ConditionalAbortDecorator that checks for direct sight or high suspicion when the guard is not already searching. When the guard sees the player it tracks their position directly. On losing sight it locks the last known position as its goal and commits to that path. This locking was necessary because without it hearing would shift the destination while the guard was pathing around a wall and the NavMesh would flip between two routes. Arriving at the locked position without regaining sight sets IsSearching on the Blackboard and returns Failure which hands off to Search.

**Investigate** has a CooldownDecorator with a 5 second cooldown and a ConditionalAbortDecorator requiring suspicion above the investigate threshold. The guard walks to the last known position at patrol speed and drops suspicion on arrival so it naturally falls back to patrol.

**Search** is a Sequence with two children. A ConditionNode gates on IsSearching and a TimeoutDecorator wrapping LeafSearch limits the behaviour to 10 seconds. The guard wanders randomly near the last known position and uses a hasGoal flag to make sure it reaches each point before picking another.

**Patrol** is the fallback. It cycles waypoints and skips any that are unreachable for more than 2 seconds which handles cases like the door carve blocking a route. All tunable values like speeds and thresholds and timeouts are exposed as SerializeField so the encounter can be adjusted from the Inspector without code changes.

# Perception

GuardSensor runs every frame and does a line of sight check using an angle cone and raycast plus a hearing range check based on distance. Results get written to the Blackboard. Only sight updates LastKnownPosition because hearing tells the guard someone is nearby but not where exactly. This prevents the guard from trying to path to positions behind walls it cannot reach.

SuspicionSystem reads the Blackboard and accumulates suspicion from sight at 0.8 per second or hearing at 0.4 per second and decays at 0.3 per second when neither is active. Both thresholds are SerializeField for Inspector tuning.

# Design Trade-Off

I went with a flat four branch Selector rather than nested sub-trees. Sub-trees would help with modularity if there were multiple NPC types but for one guard the flat structure keeps the priority order explicit and the debug overlay immediately traceable. The downside is that adding a new behaviour means editing GuardBT directly rather than composing a new sub-tree which is something I plan to refactor for CA3.
