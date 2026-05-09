using Fusion;
using TMPro;
using UnityEngine;

namespace CA3.Networking
{
    public class NetworkPlayerScore : NetworkBehaviour
    {
        [Networked] public int Score { get; set; }

        private static TextMeshProUGUI scoreLabel;

        public override void Spawned()
        {
            if (HasInputAuthority)
                CreateScoreUI();
        }

        public override void Render()
        {
            if (HasInputAuthority && scoreLabel != null)
                scoreLabel.text = "Pickups: " + Score;
        }

        //called via RPC from NetworkPickup. Routed to the StateAuthority which legally write to the [Networked] Score property.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AwardScore()
        {
            Score += 1;
        }

        private void CreateScoreUI()
        {
            var canvas = new GameObject("ScoreCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();

            var label = new GameObject("ScoreText").AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(canvas.transform);
            label.fontSize = 36;
            label.color = Color.white;
            label.text = "Pickups: 0";

            var rt = label.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            rt.sizeDelta = new Vector2(300, 50);

            scoreLabel = label;
        }
    }
}