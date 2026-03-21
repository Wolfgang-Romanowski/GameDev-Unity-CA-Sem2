using UnityEngine;

public class GoalZone : MonoBehaviour
{
    private GameUI gameUI;

    void Awake()
    {
        gameUI = GetComponent<GameUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            gameUI.ShowWin();
    }
}