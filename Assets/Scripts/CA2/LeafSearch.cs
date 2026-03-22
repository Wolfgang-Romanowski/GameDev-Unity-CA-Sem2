using UnityEngine;

public class LeafSearch : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float patrolSpeed;
    private float wanderRadius;
    private bool hasGoal = false;

    public LeafSearch(GuardAI guardAI, GuardMovement movement,
                      Blackboard blackboard, float patrolSpeed,
                      float wanderRadius = 5f) : base("Search")
    {
        this.guardAI      = guardAI;
        this.movement     = movement;
        this.blackboard   = blackboard;
        this.patrolSpeed  = patrolSpeed;
        this.wanderRadius = wanderRadius;
    }

    public override BTStatus Tick()
    {
        blackboard.ActiveBTNode = "Search";
        blackboard.IsSearching = true;
        guardAI.SetState(GuardState.Search);
        movement.SetSpeed(patrolSpeed);

        if (!hasGoal || movement.NearDestination(1.5f))
        {
            if (movement.SetRandomGoalNear(blackboard.LastKnownPosition, wanderRadius))
            {
                blackboard.CurrentGoal = movement.CurrentDestination;
                hasGoal = true;
            }
            else
            {
                return BTStatus.Failure;
            }
        }

        if (!movement.HasValidPath && !movement.NearDestination(2f))
        {
            hasGoal = false;
        }

        return BTStatus.Running;
    }
}