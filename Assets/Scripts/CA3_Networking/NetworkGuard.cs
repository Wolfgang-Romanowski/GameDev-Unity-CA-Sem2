using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace CA3.Networking
{
    public class NetworkGuard : NetworkBehaviour
    {
        public enum GuardState { Patrol, Chase }

        [Header("Patrol")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachedDistance = 1f;
        [SerializeField] private float patrolSpeed = 2f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float catchDistance = 1.5f;
        [SerializeField] private float loseSightDelay = 3f;

        [Header("Catch")]
        [SerializeField] private float catchCooldown = 2f;
        [SerializeField] private float stunDuration = 2f;

        [Header("Performance")]
        [SerializeField] private int perceptionTickInterval = 4;

        [Networked] public int        CurrentWaypointIndex { get; set; }
        [Networked] public GuardState State                { get; set; }

        private int perceptionTickCounter = 0;

        private NavMeshAgent agent;
        private NetworkObject currentTarget;
        private float lostSightAt = -1f;
        private float nextCatchAllowedAt = 0f;

        public override void Spawned()
        {
            agent = GetComponent<NavMeshAgent>();

            if (!HasStateAuthority)
            {
                agent.enabled = false;
                return;
            }

            agent.speed = patrolSpeed;
            if (waypoints != null && waypoints.Length > 0)
                agent.SetDestination(waypoints[CurrentWaypointIndex].position);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (agent == null || !agent.isOnNavMesh) return;

            if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.GameOver)
                {
                    if (agent.hasPath) agent.ResetPath();
                    return;
                }
                
            NetworkObject closestPlayer = null;
            perceptionTickCounter++;
            if (perceptionTickCounter >= perceptionTickInterval)
            {
                perceptionTickCounter = 0;
                closestPlayer = FindClosestPlayerInRange();
            }
            else if (State == GuardState.Chase && currentTarget != null)
            {
                //skip the cached target on off-ticks if they're stunned, otherwise we'd never break chase
                var cachedStun = currentTarget.GetComponent<NetworkPlayerStun>();
                if (cachedStun == null || !cachedStun.IsStunned)
                    closestPlayer = currentTarget;
            }

            if (closestPlayer != null)
            {
                EnterChase(closestPlayer);
            }
            else if (State == GuardState.Chase)
            {
                if (lostSightAt < 0f) lostSightAt = Time.time;
                if (Time.time - lostSightAt > loseSightDelay)
                    ReturnToPatrol();
            }

            if (State == GuardState.Chase)
                TickChase();
            else
                TickPatrol();
        }

        private void TickPatrol()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (!agent.pathPending && agent.remainingDistance < waypointReachedDistance)
            {
                CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[CurrentWaypointIndex].position);
            }
        }

        private void TickChase()
        {
            if (currentTarget == null)
            {
                ReturnToPatrol();
                return;
            }

            agent.SetDestination(currentTarget.transform.position);

            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distToTarget <= catchDistance && Time.time >= nextCatchAllowedAt)
                CatchPlayer(currentTarget);
        }

        private void EnterChase(NetworkObject target)
        {
            State = GuardState.Chase;
            currentTarget = target;
            agent.speed = chaseSpeed;
            lostSightAt = -1f;
        }

        private void ReturnToPatrol()
        {
            State = GuardState.Patrol;
            currentTarget = null;
            agent.speed = patrolSpeed;
            lostSightAt = -1f;

            if (waypoints != null && waypoints.Length > 0)
                agent.SetDestination(waypoints[CurrentWaypointIndex].position);
        }

        private void CatchPlayer(NetworkObject playerObject)
        {
            var stun = playerObject.GetComponent<NetworkPlayerStun>();
            //don't refresh stun on an already-stunned player — that's what was perma-locking them
            if (stun != null && stun.IsStunned) return;

            nextCatchAllowedAt = Time.time + catchCooldown;
            if (stun != null)
                stun.RPC_ApplyStun(stunDuration);

            //back off after a catch so the player gets their full stun window to recover
            ReturnToPatrol();
        }

        private NetworkObject FindClosestPlayerInRange()
        {
            NetworkObject best = null;
            float bestDist = chaseRange;

            foreach (var p in Runner.ActivePlayers)
            {
                var po = Runner.GetPlayerObject(p);
                if (po == null) continue;

                var stun = po.GetComponent<NetworkPlayerStun>();
                if (stun != null && stun.IsStunned) continue;

                float d = Vector3.Distance(transform.position, po.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = po;
                }
            }

            return best;
        }
    }
}