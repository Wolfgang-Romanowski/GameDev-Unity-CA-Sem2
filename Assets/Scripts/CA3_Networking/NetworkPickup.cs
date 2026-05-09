using Fusion;
using UnityEngine;

namespace CA3.Networking
{
    public class NetworkPickup : NetworkBehaviour
    {
        [Networked] public NetworkBool IsCollected { get; set; }

        private Renderer bodyRenderer;
        private Collider trigger;

        public override void Spawned()
        {
            bodyRenderer = GetComponent<Renderer>();
            trigger      = GetComponent<Collider>();
            ApplyVisibility();
        }

        public override void Render()
        {
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (bodyRenderer != null) bodyRenderer.enabled = !IsCollected;
            if (trigger != null)      trigger.enabled      = !IsCollected;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCollected) return;

            var score = other.GetComponent<NetworkPlayerScore>();
            if (score == null) return;
            if (!score.HasInputAuthority) return;

            RPC_RequestPickup(score.Object.InputAuthority);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestPickup(PlayerRef collector)
        {
            if (IsCollected) return;

            var playerObj = Runner.GetPlayerObject(collector);
            if (playerObj == null) return;

            var score = playerObj.GetComponent<NetworkPlayerScore>();
            if (score == null) return;

            IsCollected = true;
            score.RPC_AwardScore();
        }
    }
}