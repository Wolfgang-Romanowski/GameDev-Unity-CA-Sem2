using UnityEngine;

public class SuspicionSystem : MonoBehaviour
{
    [SerializeField] private Blackboard blackboard;
    [SerializeField] private float sightAccumulateRate   = 0.8f;
    [SerializeField] private float hearingAccumulateRate = 0.4f;
    [SerializeField] private float decayRate             = 0.3f;

    public float InvestigateThreshold => investigateThreshold;
    public float ChaseThreshold       => chaseThreshold;

    [SerializeField] private float investigateThreshold = 0.3f;
    [SerializeField] private float chaseThreshold       = 0.8f;

    void Update()
    {
        if (blackboard == null) return;

        float suspicion = blackboard.SuspicionLevel;

        if (blackboard.CanSeePlayer)
            suspicion += sightAccumulateRate * Time.deltaTime;
        else if (blackboard.PlayerInHearingRange)
            suspicion += hearingAccumulateRate * Time.deltaTime;
        else
            suspicion -= decayRate * Time.deltaTime;

        blackboard.SuspicionLevel = Mathf.Clamp01(suspicion);
    }
}