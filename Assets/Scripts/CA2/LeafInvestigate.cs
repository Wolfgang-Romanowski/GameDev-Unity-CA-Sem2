using UnityEngine;

public class LeafInvestigate : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float patrolSpeed;
    private bool goalSet = false;

    public LeafInvestigate(GuardAI guardAI, GuardMovement movement,
                           Blackboard blackboard, float patrolSpeed) : base("Investigate")
    {
        this.guardAI     = guardAI;
        this.movement    = movement;
        this.blackboard  = blackboard;
        this.patrolSpeed = patrolSpeed;
    }

    public override BTStatus Tick()
    {
        blackboard.ActiveBTNode = "Investigate";
        guardAI.SetState(GuardState.Investigate);
        movement.SetSpeed(patrolSpeed);

        if (!goalSet)
        {
            movement.SetGoal(blackboard.LastKnownPosition);
            blackboard.CurrentGoal = blackboard.LastKnownPosition;
            goalSet = true;
        }

        if (!movement.HasValidPath)
        {
            goalSet = false;
            return BTStatus.Failure;
        }

        if (movement.NearDestination(1f))
        {
            goalSet = false;
            blackboard.SuspicionLevel = 0.15f;
            return BTStatus.Success;
        }

        return BTStatus.Running;
    }
}