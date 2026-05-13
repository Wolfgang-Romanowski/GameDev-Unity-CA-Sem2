using UnityEngine;
using UnityEngine.AI;

public class GuardMovement : MonoBehaviour
{
    [SerializeField] private LineRenderer pathLine;
    [SerializeField] private float stuckThreshold = 3f;

    public Vector3 CurrentDestination => agent.destination;
    public bool ReachedDestination => agent.remainingDistance < 0.5f && !agent.pathPending;
    public bool NearDestination(float dist) => agent.remainingDistance < dist && !agent.pathPending;
    public bool IsStuck { get; private set; }
    //pathpartial means reached closest reachable point, path invalid means no path exists
    public bool HasValidPath => agent.pathPending || (agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete);

    private NavMeshAgent agent;
    private float stuckTimer;
    private bool isLookingAround;
    private float lookAroundSpeed = 90f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 40f;
        agent.angularSpeed = 360f;
        agent.autoBraking = false;
    }


    void Update()
    {
        UpdateStuckDetection();

        if (isLookingAround)
            transform.Rotate(0f, lookAroundSpeed * Time.deltaTime, 0f);
    }

    //pivots the guard in place — visually distinct from Search's wandering
    public void StartLookAround()
    {
        if (isLookingAround) return;
        isLookingAround = true;
        agent.updateRotation = false;
    }

    public void StopLookAround()
    {
        if (!isLookingAround) return;
        isLookingAround = false;
        agent.updateRotation = true;
    }

    public void SetGoal(Vector3 position)
    {
        StopLookAround();
        agent.SetDestination(position);
    }

    //only repaths if the new goal is far enough from the current one
    public void SetGoalIfFarEnough(Vector3 position, float minDelta = 2f)
    {
        StopLookAround();
        if (agent.pathPending) return;

        if (Vector3.Distance(agent.destination, position) > minDelta)
            agent.SetDestination(position);
    }

    // forces an immediate repath to the current destination
    // bypasses pathPending — used when the navmesh changes (e.g. door opens)
    public void ForceRepath()
    {
        StopLookAround();
        Vector3 dest = agent.destination;
        agent.ResetPath();
        agent.SetDestination(dest);
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void ClearPath()
    {
        StopLookAround();
        agent.ResetPath();
        stuckTimer = 0f;
        IsStuck = false;
    }

    //tries a random navmesh point near the given position
    public bool SetRandomGoalNear(Vector3 center, float radius)
    {
        StopLookAround();
        if (agent.pathPending) return true;

        Vector3 randomPoint = center + Random.insideUnitSphere * radius;
        randomPoint.y = center.y;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            return true;
        }
        return false;
    }

    void UpdateStuckDetection()
    {
        bool stalled = agent.velocity.magnitude < 0.1f
            && agent.remainingDistance > 1.5f
            && !agent.pathPending;

        if (stalled)
        {
            stuckTimer += Time.deltaTime;
            IsStuck = stuckTimer > stuckThreshold;
        }
        else
        {
            stuckTimer = 0f;
            IsStuck = false;
        }
    }

    //colour coded path line for debug visualisation
    public void UpdatePathLine(Color color)
    {
        if (pathLine == null) return;

        if (agent.hasPath)
        {
            pathLine.positionCount = agent.path.corners.Length;
            pathLine.SetPositions(agent.path.corners);
            pathLine.startColor = color;
            pathLine.endColor = color;
        }
        else
        {
            pathLine.positionCount = 0;
        }
    }
}