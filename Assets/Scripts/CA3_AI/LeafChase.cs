using UnityEngine;

public class LeafChase : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private float chaseSpeed;
    private float loseSightDelay;
    private float loseSightDeadline = -1f;

    private Vector3 lockedGoal;
    private bool goalLocked = false;

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
        blackboard.IsSearching = false;
        blackboard.ActiveBTNode = "Chase";
        guardAI.SetState(GuardState.Chase);
        movement.SetSpeed(chaseSpeed);

        //confidence > 0.3 means we've had 0.2s of clean LOS brief occlusions don't snapbreak chase
        if (blackboard.SightConfidence > 0.3f && blackboard.TargetTransform != null)
        {
            loseSightDeadline = -1f;
            goalLocked = false;
            movement.SetGoalIfFarEnough(blackboard.TargetTransform.position, 2f);
            blackboard.CurrentGoal = blackboard.TargetTransform.position;
            return BTStatus.Running;
        }

        if (!goalLocked)
        {
            lockedGoal = blackboard.LastKnownPosition;
            goalLocked = true;
            movement.SetGoal(lockedGoal);
            blackboard.CurrentGoal = lockedGoal;
        }

        if (movement.NearDestination(2f))
        {
            loseSightDeadline = -1f;
            goalLocked = false;
            blackboard.IsSearching = true;
            return BTStatus.Failure;
        }

        if (blackboard.PlayerInHearingRange)
        {
            loseSightDeadline = -1f;
        }
        else
        {
            if (loseSightDeadline < 0f)
                loseSightDeadline = Time.time + loseSightDelay;

            if (Time.time >= loseSightDeadline)
            {
                loseSightDeadline = -1f;
                goalLocked = false;
                blackboard.IsSearching = true;
                return BTStatus.Failure;
            }
        }

        return BTStatus.Running;
    }

    public void ResetDeadline() => loseSightDeadline = -1f;
}