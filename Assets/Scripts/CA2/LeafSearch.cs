using UnityEngine;

public class LeafSearch : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float patrolSpeed;
    private float wanderRadius;

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
        guardAI.SetState(GuardState.Search);
        movement.SetSpeed(patrolSpeed);

        if (movement.NearDestination(1.5f))
        {
            movement.SetRandomGoalNear(blackboard.LastKnownPosition, wanderRadius);
            blackboard.CurrentGoal = blackboard.LastKnownPosition;
        }

        return BTStatus.Running;
    }
}