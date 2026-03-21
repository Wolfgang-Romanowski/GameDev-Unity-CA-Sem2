using UnityEngine;

public class Blackboard : MonoBehaviour
{
    public bool CanSeePlayer { get; set; }
    public bool PlayerInHearingRange { get; set; }
    public float SuspicionLevel { get; set; }
    public Transform TargetTransform { get; set; }
    public Vector3 LastKnownPosition { get; set; }
    public Vector3 CurrentGoal { get; set; }
    public string ActiveBTNode { get; set; } = "None";
}

public static class BlackboardKeys
{
    public const string CanSeePlayer         = "CanSeePlayer";
    public const string PlayerInHearingRange = "PlayerInHearingRange";
    public const string SuspicionLevel       = "SuspicionLevel";
    public const string TargetTransform      = "TargetTransform";
    public const string LastKnownPosition    = "LastKnownPosition";
    public const string CurrentGoal          = "CurrentGoal";
    public const string ActiveBTNode         = "ActiveBTNode";
}