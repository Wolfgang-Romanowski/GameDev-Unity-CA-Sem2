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

    [SerializeField] private float patrolSpeed    = 2f;
    [SerializeField] private float chaseSpeed     = 5f;
    [SerializeField] private float loseSightDelay = 1.5f;
    [SerializeField] private float searchTimeout  = 10f;
    [SerializeField] private float btTickRate     = 0.1f;

    private BTNode root;
    private LeafChase leafChase;

    void Update()
    {
        movement.UpdatePathLine(guardAI.GetStateColor());
    }

    void Start()
    {
        leafChase = new LeafChase(guardAI, movement, blackboard, chaseSpeed, loseSightDelay);

        var searchWithTimeout = new TimeoutDecorator("SearchTimeout",
            new LeafSearch(guardAI, movement, blackboard, patrolSpeed),
            searchTimeout);

        var chaseBranch = new BTSequence("ChaseBranch", new List<BTNode>
        {
            new ConditionalAbortDecorator("AbortIfNotAlert",
                leafChase,
                () => blackboard.SuspicionLevel >= suspicion.ChaseThreshold
                   || blackboard.CanSeePlayer),
        });

        var investigateBranch = new BTSequence("InvestigateBranch", new List<BTNode>
        {
            new ConditionalAbortDecorator("AbortIfNotSuspicious",
                new LeafInvestigate(guardAI, movement, blackboard, patrolSpeed),
                () => blackboard.SuspicionLevel >= suspicion.InvestigateThreshold),
        });

        var searchBranch = new BTSequence("SearchBranch", new List<BTNode>
        {
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