using UnityEngine;
using Fusion;
using TMPro;

namespace CA3.Networking
{
    public class NetworkGameManager : NetworkBehaviour
    {
        [SerializeField] private int targetScore = 3;

        [Networked] public int    WinnerPlayerId { get; set; } = -1;
        [Networked] public NetworkBool GameOver  { get; set; }

        private TextMeshProUGUI winnerLabel;

        public override void Spawned()
        {
            CreateWinnerUI();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (GameOver) return;

            foreach (var p in Runner.ActivePlayers)
            {
                var playerObj = Runner.GetPlayerObject(p);
                if (playerObj == null) continue;

                var score = playerObj.GetComponent<NetworkPlayerScore>();
                if (score == null) continue;

                if (score.Score >= targetScore)
                {
                    WinnerPlayerId = p.PlayerId;
                    GameOver       = true;
                    return;
                }
            }
        }

        public override void Render()
        {
            if (winnerLabel == null) return;

            if (GameOver)
            {
                winnerLabel.text = "Player " + WinnerPlayerId + " wins!";
                winnerLabel.gameObject.SetActive(true);
            }
            else
            {
                winnerLabel.gameObject.SetActive(false);
            }
        }

        private void CreateWinnerUI()
        {
            var canvas = new GameObject("WinnerCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();

            var label = new GameObject("WinnerText").AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(canvas.transform);
            label.fontSize = 80;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.text = "";

            var rt = label.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 200);

            label.gameObject.SetActive(false);
            winnerLabel = label;
        }
    }
}