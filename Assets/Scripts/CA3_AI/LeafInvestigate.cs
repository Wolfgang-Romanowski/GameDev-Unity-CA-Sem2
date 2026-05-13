using UnityEngine;

public class LeafInvestigate : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float patrolSpeed;
    private float lingerDuration;

    private bool goalSet = false;
    private float lingerEndTime = -1f;
    private float pathWaitDeadline = -1f;

    public LeafInvestigate(GuardAI guardAI, GuardMovement movement,
                           Blackboard blackboard, float patrolSpeed,
                           float lingerDuration = 2f) : base("Investigate")
    {
        this.guardAI        = guardAI;
        this.movement       = movement;
        this.blackboard     = blackboard;
        this.patrolSpeed    = patrolSpeed;
        this.lingerDuration = lingerDuration;
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

        //grace window: a fresh SetDestination needs a frame or two before HasValidPath turns true
        if (!movement.HasValidPath)
        {
            if (pathWaitDeadline < 0f) pathWaitDeadline = Time.time + 1f;
            if (Time.time < pathWaitDeadline)
                return BTStatus.Running;
            Reset();
            return BTStatus.Failure;
        }
        pathWaitDeadline = -1f;

        if (movement.NearDestination(1f))
        {
            if (lingerEndTime < 0f)
            {
                lingerEndTime = Time.time + lingerDuration;
                movement.StartLookAround();
            }

            if (Time.time < lingerEndTime)
                return BTStatus.Running;

            blackboard.SuspicionLevel = 0.15f;
            Reset();
            return BTStatus.Success;
        }

        return BTStatus.Running;
    }

    void Reset()
    {
        movement.StopLookAround();
        goalSet = false;
        lingerEndTime = -1f;
        pathWaitDeadline = -1f;
    }
}
