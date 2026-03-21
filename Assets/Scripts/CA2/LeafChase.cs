using UnityEngine;

public class LeafChase : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float chaseSpeed;
    private float loseSightDelay;
    private float loseSightDeadline = -1f;

    public LeafChase(GuardAI guardAI, GuardMovement movement,
                     Blackboard blackboard, float chaseSpeed,
                     float loseSightDelay) : base("Chase")
    {
        this.guardAI        = guardAI;
        this.movement       = movement;
        this.blackboard     = blackboard;
        this.chaseSpeed     = chaseSpeed;
        this.loseSightDelay = loseSightDelay;
    }

    public override BTStatus Tick()
    {
        blackboard.ActiveBTNode = "Chase";
        guardAI.SetState(GuardState.Chase);
        movement.SetSpeed(chaseSpeed);

        if (blackboard.CanSeePlayer && blackboard.TargetTransform != null)
        {
            // actively seeing player — reset deadline and keep chasing
            loseSightDeadline = -1f;
            movement.SetGoalIfFarEnough(blackboard.TargetTransform.position);
            blackboard.CurrentGoal = blackboard.TargetTransform.position;
        }
        else
        {
            // start deadline if not already counting
            if (loseSightDeadline < 0f)
                loseSightDeadline = Time.time + loseSightDelay;

            movement.SetGoalIfFarEnough(blackboard.LastKnownPosition, 1f);
            blackboard.CurrentGoal = blackboard.LastKnownPosition;

            if (Time.time >= loseSightDeadline)
            {
                loseSightDeadline = -1f;
                return BTStatus.Failure;
            }
        }

        return BTStatus.Running;
    }

    public void ResetDeadline() => loseSightDeadline = -1f;
}