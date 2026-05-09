using UnityEngine;
using Fusion;

namespace CA3.Networking
{
    public class NetworkPlayerAppearance : NetworkBehaviour
    {
        [Networked] public int ColorIndex { get; set; }

        private static readonly Color[] palette =
        {
            Color.red,
            Color.green,
            Color.yellow,
            Color.blue
        };

        private Renderer bodyRenderer;
        private Material instanceMaterial;
        private int appliedIndex = -1;

        private void Awake()
        {
            bodyRenderer = GetComponentInChildren<Renderer>();
            if (bodyRenderer != null)
                instanceMaterial = bodyRenderer.material;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
                ColorIndex = Object.InputAuthority.PlayerId % palette.Length;
        }

        public override void Render()
        {
            if (instanceMaterial == null) return;
            if (appliedIndex == ColorIndex) return;

            instanceMaterial.color = palette[ColorIndex];
            appliedIndex = ColorIndex;
        }
    }
}