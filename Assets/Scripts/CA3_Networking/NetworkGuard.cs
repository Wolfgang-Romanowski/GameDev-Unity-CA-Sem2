using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace CA3.Networking
{
    public class NetworkGuard : NetworkBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachedDistance = 1f;

        [Networked] public int CurrentWaypointIndex { get; set; }

        private NavMeshAgent agent;

        public override void Spawned()
        {
            agent = GetComponent<NavMeshAgent>();

            if (!HasStateAuthority)
            {
                agent.enabled = false;
                return;
            }

            if (waypoints != null && waypoints.Length > 0)
                agent.SetDestination(waypoints[CurrentWaypointIndex].position);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (waypoints == null || waypoints.Length == 0) return;
            if (agent == null || !agent.isOnNavMesh) return;

            if (!agent.pathPending && agent.remainingDistance < waypointReachedDistance)
                AdvanceWaypoint();
        }

        private void AdvanceWaypoint()
        {
            CurrentWaypointIndex = (CurrentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[CurrentWaypointIndex].position);
        }
    }
}