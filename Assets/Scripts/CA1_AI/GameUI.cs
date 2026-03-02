using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private TMP_Text subtitleText;

    private bool gameOver = false;

    public bool IsGameOver => gameOver;

    void Start()
    {
        endPanel.SetActive(false);
    }

    void Update()
    {
        if (!gameOver) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ShowWin()
    {
        if (gameOver) return;
        Show("ESCAPED", "you made it past the guard\npress space to restart", "#4CAF50");
    }

    public void ShowLose()
    {
        if (gameOver) return;
        Show("CAUGHT", "the guard got you\npress space to restart", "#F44336");
    }

    void Show(string title, string subtitle, string color)
    {
        gameOver = true;
        endPanel.SetActive(true);
        endText.text = $"<color={color}>{title}</color>";
        subtitleText.text = subtitle;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}