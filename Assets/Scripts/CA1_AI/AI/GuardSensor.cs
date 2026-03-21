using UnityEngine;

public class GuardSensor : MonoBehaviour
{
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 55f;
    [SerializeField] private float hearingRange = 5f;
    [SerializeField] private Blackboard blackboard;
    
    public bool CanSeePlayer { get; private set; }
    public bool PlayerInHearingRange { get; private set; }
    public Transform Player { get; private set; }

    private float playerSearchTimer = 0f;

    void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) Player = playerObj.transform;
    }

    void Update()
    {
        CanSeePlayer = false;
        PlayerInHearingRange = false;

        if (Player == null)
        {
            TryRefindPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, Player.position);

        PlayerInHearingRange = distance < hearingRange;

        if (distance <= sightRange)
        {
            Vector3 directionToPlayer = (Player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle <= sightAngle)
            {
                Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
                if (Physics.Raycast(eyePosition, directionToPlayer, out RaycastHit hit, sightRange))
                {
                    if (hit.transform.CompareTag("Player"))
                        CanSeePlayer = true;
                }
            }
        }

        // always write to blackboard regardless of sight angle
        if (blackboard != null)
        {
            blackboard.CanSeePlayer = CanSeePlayer;
            blackboard.PlayerInHearingRange = PlayerInHearingRange;
            blackboard.TargetTransform = Player;
            if (CanSeePlayer) blackboard.LastKnownPosition = Player.position;
        }
    }

    //retry periodically so we recover if the player reference breaks
    void TryRefindPlayer()
    {
        playerSearchTimer -= Time.deltaTime;
        if (playerSearchTimer <= 0f)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
            playerSearchTimer = 1f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}