using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardBT : MonoBehaviour
{
    [SerializeField] private Blackboard blackboard;
    [SerializeField] private GuardAI guardAI;
    [SerializeField] private GuardMovement movement;
    [SerializeField] private SuspicionSystem suspicion;
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private float patrolSpeed         = 2f;
    [SerializeField] private float chaseSpeed          = 5f;
    [SerializeField] private float loseSightDelay      = 1.5f;
    [SerializeField] private float searchTimeout       = 10f;
    [SerializeField] private float btTickRate          = 0.1f;
    [SerializeField] private float investigateCooldown = 5f;

    private BTNode root;
    private LeafChase leafChase;

    void Update()
    {
        movement.UpdatePathLine(guardAI.GetStateColor());
        HandleStuck();
    }

    void HandleStuck()
    {
        if (!movement.IsStuck) return;

        movement.ClearPath();

        if (guardAI.CurrentState == GuardState.Patrol && waypoints != null && waypoints.Length > 0)
        {
            movement.SetGoal(waypoints[0].position);
            blackboard.CurrentGoal = waypoints[0].position;
        }
        else
        {
            blackboard.SuspicionLevel = 0f;
            blackboard.IsSearching = false;
            guardAI.SetState(GuardState.Patrol);
            if (waypoints != null && waypoints.Length > 0)
                movement.SetGoal(waypoints[0].position);
        }
    }

    void Start()
    {
        leafChase = new LeafChase(guardAI, movement, blackboard, chaseSpeed, loseSightDelay);

        var searchWithTimeout = new TimeoutDecorator("SearchTimeout",
            new LeafSearch(guardAI, movement, blackboard, patrolSpeed),
            searchTimeout);

        var chaseBranch = new ConditionalAbortDecorator("AbortIfNotAlert",
            leafChase,
            () => blackboard.CanSeePlayer
               || (blackboard.SuspicionLevel >= suspicion.ChaseThreshold && !blackboard.IsSearching));

        var investigateBranch = new CooldownDecorator("InvestigateCooldown",
            new ConditionalAbortDecorator("AbortIfNotSuspicious",
                new LeafInvestigate(guardAI, movement, blackboard, patrolSpeed),
                () => !blackboard.IsSearching && blackboard.SuspicionLevel >= suspicion.InvestigateThreshold),
            investigateCooldown);

        var searchBranch = new BTSequence("SearchBranch", new List<BTNode>
        {
            new ConditionNode("IsSearching", () => blackboard.IsSearching),
            searchWithTimeout
        });

        root = new BTSelector("Root", new List<BTNode>
        {
            chaseBranch,
            investigateBranch,
            searchBranch,
            new LeafPatrol(guardAI, movement, blackboard, waypoints, patrolSpeed)
        });

        if (waypoints != null && waypoints.Length > 0)
            movement.SetGoal(waypoints[0].position);

        StartCoroutine(TickTree());
    }

    IEnumerator TickTree()
    {
        while (true)
        {
            root.Tick();
            yield return new WaitForSeconds(btTickRate);
        }
    }
}