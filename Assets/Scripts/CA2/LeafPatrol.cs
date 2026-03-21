using UnityEngine;

public class LeafPatrol : BTNode
{
    private GuardAI guardAI;
    private GuardMovement movement;
    private Blackboard blackboard;
    private Transform[] waypoints;
    private int currentIndex = 0;
    private float patrolSpeed;

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
        if (waypoints == null || waypoints.Length == 0)
            return BTStatus.Running;

        blackboard.ActiveBTNode = "Patrol";
        guardAI.SetState(GuardState.Patrol);
        movement.SetSpeed(patrolSpeed);

        if (movement.ReachedDestination)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            movement.SetGoal(waypoints[currentIndex].position);
            blackboard.CurrentGoal = waypoints[currentIndex].position;
        }

        return BTStatus.Running;
    }
}