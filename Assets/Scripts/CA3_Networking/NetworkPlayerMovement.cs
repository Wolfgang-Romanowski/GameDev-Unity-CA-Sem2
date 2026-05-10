using UnityEngine;
using Fusion;

namespace CA3.Networking
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 direction;
    }

    public class NetworkPlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float wallCheckDistance = 0.6f;
        [SerializeField] private LayerMask wallLayers = ~0;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.GameOver) return;
            var stun = GetComponent<NetworkPlayerStun>();
            if (stun != null && stun.IsStunned) return;
            if (GetInput(out NetworkInputData input))
            {
                Vector3 step = input.direction.normalized * moveSpeed * Runner.DeltaTime;
                Vector3 moved = TryMoveAxis(step);
                transform.position += moved;
            }
        }

        private Vector3 TryMoveAxis(Vector3 step)
        {
            Vector3 result = Vector3.zero;
            result += TryAxis(new Vector3(step.x, 0f, 0f));
            result += TryAxis(new Vector3(0f, 0f, step.z));
            return result;
        }

        private Vector3 TryAxis(Vector3 axisStep)
        {
            if (axisStep.sqrMagnitude < 0.0001f) return Vector3.zero;

            Vector3 dir = axisStep.normalized;
            float dist = axisStep.magnitude + wallCheckDistance;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, dist, wallLayers))
            {
                float allowed = Mathf.Max(0f, hit.distance - wallCheckDistance);
                return dir * allowed;
            }

            return axisStep;
        }
    }
}