using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            gameUI.ShowWin();
    }
}