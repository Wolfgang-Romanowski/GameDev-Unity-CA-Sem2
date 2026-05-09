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

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            if (GetInput(out NetworkInputData input))
            {
                Vector3 step = input.direction.normalized * moveSpeed * Runner.DeltaTime;
                transform.position += step;
            }
        }
    }
}