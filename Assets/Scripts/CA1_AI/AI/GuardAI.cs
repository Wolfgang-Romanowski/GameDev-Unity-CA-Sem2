using UnityEngine;

public enum GuardState { Patrol, Investigate, Chase, Search }

public class GuardAI : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;

    [SerializeField] private float investigateTimeout = 4f;
    [SerializeField] private float searchTimeout = 6f;
    [SerializeField] private float loseSightDelay = 1.5f;

    // exposed for debug overlay and vision cone
    public GuardState CurrentState { get; private set; } = GuardState.Patrol;
    public string CurrentTarget { get; private set; } = "";
    public Vector3 CurrentDestination => movement.CurrentDestination;
    public bool CanSeePlayer => sensor.CanSeePlayer;

    private GuardSensor sensor;
    private GuardMovement movement;

    private int currentWaypointIndex = 0;
    private Vector3 lastKnownPosition;
    private float stateTimer;
    private float loseSightTimer;

    void Start()
    {
        sensor = GetComponent<GuardSensor>();
        movement = GetComponent<GuardMovement>();

        movement.SetSpeed(patrolSpeed);

        if (waypoints.Length > 0)
            movement.SetGoal(waypoints[0].position);
    }

    void Update()
    {
        CheckGlobalInterrupt();

        switch (CurrentState)
        {
            case GuardState.Patrol:      ExecutePatrol();      break;
            case GuardState.Investigate:  ExecuteInvestigate(); break;
            case GuardState.Chase:        ExecuteChase();       break;
            case GuardState.Search:       ExecuteSearch();      break;
        }

        HandleStuck();
        movement.UpdatePathLine(GetStateColor());
    }

    //hearing triggers chase from any non chase state as an interrupt
    void CheckGlobalInterrupt()
    {
        if (sensor.Player == null) return;

        if (sensor.PlayerInHearingRange && CurrentState != GuardState.Chase)
        {
            lastKnownPosition = sensor.Player.position;
            ChangeState(GuardState.Chase);
        }
    }

    void ExecutePatrol()
    {
        movement.SetSpeed(patrolSpeed);
        CurrentTarget = waypoints[currentWaypointIndex].name;

        if (movement.ReachedDestination)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            movement.SetGoal(waypoints[currentWaypointIndex].position);
        }

        if (sensor.CanSeePlayer)
        {
            lastKnownPosition = sensor.Player.position;
            ChangeState(GuardState.Investigate);
        }
    }

    void ExecuteInvestigate()
    {
        movement.SetSpeed(patrolSpeed);
        CurrentTarget = "Disturbance";
        stateTimer -= Time.deltaTime;

        if (sensor.CanSeePlayer)
        {
            lastKnownPosition = sensor.Player.position;
            ChangeState(GuardState.Chase);
            return;
        }

        if (stateTimer <= 0f || movement.NearDestination(1f))
            ChangeState(GuardState.Patrol);
    }

    void ExecuteChase()
    {
        movement.SetSpeed(chaseSpeed);
        CurrentTarget = "Player";

        if (sensor.CanSeePlayer)
        {
            lastKnownPosition = sensor.Player.position;
            loseSightTimer = loseSightDelay;
            movement.SetGoalIfFarEnough(sensor.Player.position);
        }
        else
        {
            movement.SetGoalIfFarEnough(lastKnownPosition, 1f);
            loseSightTimer -= Time.deltaTime;

            if (loseSightTimer <= 0f)
                ChangeState(GuardState.Search);
        }
    }

    void ExecuteSearch()
    {
        movement.SetSpeed(patrolSpeed);
        CurrentTarget = "Last Known Position";
        stateTimer -= Time.deltaTime;

        //wander near last known position
        if (movement.NearDestination(1.5f))
            movement.SetRandomGoalNear(lastKnownPosition, 5f);

        if (sensor.CanSeePlayer)
        {
            lastKnownPosition = sensor.Player.position;
            ChangeState(GuardState.Chase);
            return;
        }

        if (stateTimer <= 0f)
            ChangeState(GuardState.Patrol);
    }

    void ChangeState(GuardState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GuardState.Patrol:
                movement.SetGoal(waypoints[currentWaypointIndex].position);
                break;
            case GuardState.Investigate:
                stateTimer = investigateTimeout;
                movement.SetGoal(lastKnownPosition);
                break;
            case GuardState.Chase:
                loseSightTimer = loseSightDelay;
                break;
            case GuardState.Search:
                stateTimer = searchTimeout;
                movement.SetGoal(lastKnownPosition);
                break;
        }
    }

    //if movement reports stuck, skip to next waypoint or fall back to patrol
    void HandleStuck()
    {
        if (!movement.IsStuck) return;

        movement.ClearPath();

        if (CurrentState == GuardState.Patrol)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            movement.SetGoal(waypoints[currentWaypointIndex].position);
        }
        else if (CurrentState == GuardState.Search || CurrentState == GuardState.Investigate)
        {
            ChangeState(GuardState.Patrol);
        }
    }

    Color GetStateColor()
    {
        return CurrentState switch
        {
            GuardState.Patrol => Color.green,
            GuardState.Investigate => Color.yellow,
            GuardState.Chase => Color.red,
            GuardState.Search => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
    }
}