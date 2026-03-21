using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private GuardAI guard;
    [SerializeField] private Blackboard blackboard;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private GameObject overlayRoot;

    void Update()
    {
        // f1 toggles the debug panel
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            overlayRoot.SetActive(!overlayRoot.activeSelf);

        if (!overlayRoot.activeSelf || guard == null || blackboard == null) return;

        string stateColor = guard.CurrentState switch
        {
            GuardState.Patrol => "#4CAF50",
            GuardState.Investigate => "#FFC107",
            GuardState.Chase => "#F44336",
            GuardState.Search => "#FF9800",
            _ => "#FFFFFF"
        };

        Vector3 dest = blackboard.CurrentGoal;
        Vector3 lastKnown = blackboard.LastKnownPosition;
        string seePlayer = blackboard.CanSeePlayer
            ? "<color=#F44336>TRUE</color>"
            : "<color=#4CAF50>FALSE</color>";

        debugText.text =
            $"<b>GUARD AI DEBUG</b>\n" +
            $"<color=#888888>──────────────</color>\n" +
            $" State          <b><color={stateColor}>{guard.CurrentState}</color></b>\n" +
            $" BT Node        <b><color={stateColor}>{blackboard.ActiveBTNode}</color></b>\n" +
            $" Sees Player    {seePlayer}\n" +
            $" Suspicion      {SuspicionBar(blackboard.SuspicionLevel)} <b>{blackboard.SuspicionLevel:F2}</b>\n" +
            $" Destination    <b>({dest.x:F1}, {dest.z:F1})</b>\n" +
            $" Last Known     <b>({lastKnown.x:F1}, {lastKnown.z:F1})</b>\n" +
            $" Target         <b>{(blackboard.TargetTransform != null ? blackboard.TargetTransform.name : "None")}</b>\n" +
            $"<color=#888888>──────────────</color>";
    }

    string SuspicionBar(float value)
        {
    int filled = Mathf.RoundToInt(value * 10);
    return "<color=#F44336>" + new string('█', filled) + "</color>"
         + "<color=#444444>" + new string('░', 10 - filled) + "</color>";
        }
}