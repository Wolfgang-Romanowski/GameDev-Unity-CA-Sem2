using Fusion;
using UnityEngine;

namespace CA3.Networking
{
    public class NetworkGuardVisuals : NetworkBehaviour
    {
        [SerializeField] private Renderer guardRenderer;

        // Patrol: nearly black emission so guard reads as base colour
        [SerializeField] private Color patrolEmission = new Color(0.02f, 0.02f, 0.02f);

        // Chase: pure red HDR. RGB(191,0,0) base × ~5 intensity ≈ HDR colour shown in your picker
        [ColorUsage(true, true)]
        [SerializeField] private Color chaseEmission = new Color(11.5f, 0f, 0f);

        // Higher = snappier ramp. 6 gives ~0.4s to reach 95% of target
        [SerializeField] private float rampInSpeed = 6f;
        [SerializeField] private float rampOutSpeed = 3f;

        private Material instanceMaterial;
        private Color currentEmission;
        private NetworkGuard guard;

        public override void Spawned()
        {
            if (guardRenderer == null) guardRenderer = GetComponentInChildren<Renderer>();
            if (guardRenderer != null)
            {
                instanceMaterial = guardRenderer.material;
                instanceMaterial.EnableKeyword("_EMISSION");
            }
            guard = GetComponent<NetworkGuard>();
            currentEmission = patrolEmission;
        }

        public override void Render()
        {
    if (instanceMaterial == null || guard == null) return;

    bool gameOver = NetworkGameManager.Instance != null && NetworkGameManager.Instance.GameOver;
    if (gameOver) return;

    bool chasing = guard.State == NetworkGuard.GuardState.Chase;
    Color targetEmission = chasing ? chaseEmission : patrolEmission;
    float speed = chasing ? rampInSpeed : rampOutSpeed;

    currentEmission = Color.Lerp(currentEmission, targetEmission, Time.deltaTime * speed);
    instanceMaterial.SetColor("_EmissionColor", currentEmission);
        }
    }
}