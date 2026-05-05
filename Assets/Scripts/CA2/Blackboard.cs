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
    public bool IsSearching { get; set; }
}