using Fusion;
using UnityEngine;
using TMPro;

public class PlayerScore : NetworkBehaviour
{
    //synced to all clients
    [Networked] public int Score { get; set; }

    private static TextMeshProUGUI _scoreText;

    public override void Spawned()
    {
        //only create UI for local player
        if (Object.HasInputAuthority)
        {
            CreateScoreUI();
        }
    }

    public override void Render()
    {
        //update UI only for local player
        if (Object.HasInputAuthority && _scoreText != null)
        {
            _scoreText.text = "Pickups: " + Score;
        }
    }

    private void CreateScoreUI()
    {
        GameObject canvasObj = new GameObject("ScoreCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvasObj.transform);
        _scoreText = textObj.AddComponent<TextMeshProUGUI>();
        _scoreText.text = "Pickups: 0";
        _scoreText.fontSize = 36;
        _scoreText.color = Color.white;

        RectTransform rt = _scoreText.rectTransform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(300, 50);
    }
}