using UnityEngine;

public class LeafChase : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float chaseSpeed;
    private float loseSightDelay;
    private float loseSightTimer;

    public LeafChase(GuardAI guardAI, GuardMovement movement,
                     Blackboard blackboard, float chaseSpeed,
                     float loseSightDelay) : base("Chase")
    {
        this.guardAI        = guardAI;
        this.movement       = movement;
        this.blackboard     = blackboard;
        this.chaseSpeed     = chaseSpeed;
        this.loseSightDelay = loseSightDelay;
        this.loseSightTimer = loseSightDelay;
    }

    public override BTStatus Tick()
    {
        blackboard.ActiveBTNode = "Chase";
        guardAI.SetState(GuardState.Chase);
        movement.SetSpeed(chaseSpeed);

        if (blackboard.CanSeePlayer && blackboard.TargetTransform != null)
        {
            loseSightTimer = loseSightDelay;
            movement.SetGoalIfFarEnough(blackboard.TargetTransform.position);
            blackboard.CurrentGoal = blackboard.TargetTransform.position;
        }
        else
        {
            loseSightTimer -= Time.deltaTime;
            movement.SetGoalIfFarEnough(blackboard.LastKnownPosition, 1f);
            blackboard.CurrentGoal = blackboard.LastKnownPosition;

            if (loseSightTimer <= 0f)
                return BTStatus.Failure;
        }

        return BTStatus.Running;
    }

    public void ResetTimer() => loseSightTimer = loseSightDelay;
}