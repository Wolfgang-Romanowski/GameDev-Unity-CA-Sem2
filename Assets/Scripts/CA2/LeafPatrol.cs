using UnityEngine;
using UnityEngine.AI;

public class LeafPatrol : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private Transform[] waypoints;
    private int currentIndex = 0;
    private float patrolSpeed;
    private float unreachableTimer = 0f;
    private float unreachableTimeout = 2f;

    public LeafPatrol(GuardAI guardAI, GuardMovement movement,
                      Blackboard blackboard, Transform[] waypoints,
                      float patrolSpeed) : base("Patrol")
    {
        this.guardAI     = guardAI;
        this.movement    = movement;
        this.blackboard  = blackboard;
        this.waypoints   = waypoints;
        this.patrolSpeed = patrolSpeed;
    }

    public override BTStatus Tick()
    {
        blackboard.IsSearching = false;

        if (waypoints == null || waypoints.Length == 0)
            return BTStatus.Failure;

        blackboard.ActiveBTNode = "Patrol";
        guardAI.SetState(GuardState.Patrol);
        movement.SetSpeed(patrolSpeed);

        if (movement.ReachedDestination)
        {
            unreachableTimer = 0f;
            AdvanceWaypoint();
        }

        if (!movement.HasValidPath)
        {
            unreachableTimer += 0.1f;
            if (unreachableTimer > unreachableTimeout)
            {
                unreachableTimer = 0f;
                AdvanceWaypoint();
            }
        }
        else
        {
            unreachableTimer = 0f;
        }

        return BTStatus.Running;
    }

    void AdvanceWaypoint()
    {
        currentIndex = (currentIndex + 1) % waypoints.Length;
        movement.SetGoal(waypoints[currentIndex].position);
        blackboard.CurrentGoal = waypoints[currentIndex].position;
    }
}