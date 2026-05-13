using UnityEngine;

public class GuardSensor : MonoBehaviour
{
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 55f;
    [Tooltip("NOTE: must match GuardVisionCone.hearingRange so the visualisation reflects gameplay.")]
    [SerializeField] private float hearingRange = 5f;
    [SerializeField] private Blackboard blackboard;

    [Header("Sight Confidence")]
    [Tooltip("How quickly confidence rises while LOS holds (per second).")]
    [SerializeField] private float sightConfidenceRiseRate = 1.5f;
    [Tooltip("How quickly confidence decays when LOS breaks (per second).")]
    [SerializeField] private float sightConfidenceDecayRate = 1.0f;

    [Header("Hearing")]
    [Tooltip("Player must be moving faster than this to be heard. Stationary or sneaking is silent.")]
    [SerializeField] private float hearingSpeedThreshold = 1.5f;

    [Header("Debug Gizmos")]
    [Tooltip("Chase commit range — drawn for editor inspection only, not gameplay-bound.")]
    [SerializeField] private float chaseRangeForGizmo = 5f;

    public bool CanSeePlayer { get; private set; }
    public bool PlayerInHearingRange { get; private set; }
    public Transform Player { get; private set; }

    private float sightConfidence = 0f;
    private float playerSearchTimer = 0f;
    private Vector3 lastPlayerPosition;

    void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) Player = playerObj.transform;
        if (Player != null) lastPlayerPosition = Player.position;
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

        //velocity  aware hearing stationary or sneaking players are silent
        float playerSpeed = (Player.position - lastPlayerPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPlayerPosition = Player.position;
        PlayerInHearingRange = distance < hearingRange && playerSpeed > hearingSpeedThreshold;

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

        //smooth binary LOS into a 0-1 confidence value so brief occlusions don't snap-break chase
        if (CanSeePlayer)
            sightConfidence = Mathf.Min(1f, sightConfidence + sightConfidenceRiseRate * Time.deltaTime);
        else
            sightConfidence = Mathf.Max(0f, sightConfidence - sightConfidenceDecayRate * Time.deltaTime);

        if (blackboard != null)
        {
            blackboard.CanSeePlayer = CanSeePlayer;
            blackboard.PlayerInHearingRange = PlayerInHearingRange;
            blackboard.SightConfidence = sightConfidence;

            if (CanSeePlayer || PlayerInHearingRange)
                blackboard.TargetTransform = Player;
            if (CanSeePlayer)
                blackboard.LastKnownPosition = Player.position;
        }
    }

    //retry periodically so we recover if the player reference breaks
    void TryRefindPlayer()
    {
    if (blackboard != null)
    {
        blackboard.CanSeePlayer = false;
        blackboard.PlayerInHearingRange = false;
        blackboard.SightConfidence = 0f;
    }

    playerSearchTimer -= Time.deltaTime;
    if (playerSearchTimer <= 0f)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) Player = playerObj.transform;
        playerSearchTimer = 1f;
    }}

    void OnDrawGizmosSelected()
    {
        //hearing range — yellow
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        //chase commit range — red
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, chaseRangeForGizmo);

        //sight cone edge rays — green
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Vector3 forward = transform.forward * sightRange;
        Quaternion leftRot = Quaternion.AngleAxis(-sightAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(sightAngle, Vector3.up);
        Gizmos.DrawRay(transform.position, leftRot * forward);
        Gizmos.DrawRay(transform.position, rightRot * forward);
    }
}