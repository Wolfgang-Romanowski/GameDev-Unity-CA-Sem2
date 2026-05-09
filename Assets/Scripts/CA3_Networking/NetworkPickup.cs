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

            //shared mode colliding player asks for state authority and writes [Networked] flag increments score
            if (!Object.HasStateAuthority)
                Object.RequestStateAuthority();

            if (!Object.HasStateAuthority) return;

            IsCollected = true;
            score.Score += 1;
        }
    }
}