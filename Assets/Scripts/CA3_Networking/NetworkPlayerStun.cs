using Fusion;
using UnityEngine;

namespace CA3.Networking
{
    public class NetworkPlayerStun : NetworkBehaviour
    {
        [Networked] public float StunUntilTime { get; set; }

        public bool IsStunned => StunUntilTime > Runner.SimulationTime;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ApplyStun(float duration)
        {
            StunUntilTime = (float)Runner.SimulationTime + duration;
        }
    }
}