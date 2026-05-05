using UnityEngine;

public class GuardCatch : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private float catchDistance = 2.0f;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (gameUI == null || gameUI.IsGameOver || player == null) return;

        if (Vector3.Distance(transform.position, player.position) < catchDistance)
            gameUI.ShowLose();
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameUI == null || gameUI.IsGameOver) return;

        if (other.CompareTag("Player"))
            gameUI.ShowLose();
    }
}