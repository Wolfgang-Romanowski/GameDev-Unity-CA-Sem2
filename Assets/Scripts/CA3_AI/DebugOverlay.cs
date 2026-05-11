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
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            overlayRoot.SetActive(!overlayRoot.activeSelf);

        if (!overlayRoot.activeSelf || guard == null || blackboard == null) return;

        string sc = guard.CurrentState switch
        {
            GuardState.Patrol      => "#4CAF50",
            GuardState.Investigate => "#FFC107",
            GuardState.Chase       => "#F44336",
            GuardState.Search      => "#FF9800",
            _                      => "#FFFFFF"
        };

        Vector3 d = blackboard.CurrentGoal;
        Vector3 lk = blackboard.LastKnownPosition;

        string see  = blackboard.CanSeePlayer        ? "<color=#F44336>\u25CF</color>" : "<color=#666>\u25CB</color>";
        string hear = blackboard.PlayerInHearingRange ? "<color=#FFC107>\u25CF</color>" : "<color=#666>\u25CB</color>";
        string srch = blackboard.IsSearching          ? "<color=#FF9800>\u25CF</color>" : "<color=#666>\u25CB</color>";

        string tgt = blackboard.TargetTransform != null
            ? blackboard.TargetTransform.name : "---";

        string div = "<color=#555>\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500</color>";

        debugText.text =
            $"<b>GUARD AI</b>  <b><color={sc}>{guard.CurrentState}</color></b>\n" +
            $"{div}\n" +
            $"Node   <b><color={sc}>{blackboard.ActiveBTNode}</color></b>\n" +
            $"Target <b>{tgt}</b>\n" +
            $"{div}\n" +
            $"{see} See {hear} Hear {srch} Srch\n" +
            $"<size=85%><color=#888>Suspicion</color></size>\n" +
            $"{SuspicionBar(blackboard.SuspicionLevel)} {blackboard.SuspicionLevel:P0}\n" +
            $"<size=85%><color=#888>Sight Confidence</color></size>\n" +
            $"{ConfidenceBar(blackboard.SightConfidence)} {blackboard.SightConfidence:P0}\n" +
            $"{div}\n" +
            $"Goal  <b>({d.x:F1}, {d.z:F1})</b>\n" +
            $"LKP   <b>({lk.x:F1}, {lk.z:F1})</b>\n" +
            $"<size=75%><color=#999>F1 Toggle</color></size>";
    }

    string SuspicionBar(float value)
    {
        int total = 10;
        int filled = Mathf.RoundToInt(value * total);
        string c = value < 0.3f ? "#4CAF50" : value < 0.8f ? "#FFC107" : "#F44336";
        return "<color=" + c + ">" + new string('\u2588', filled) + "</color>"
             + "<color=#333>" + new string('\u2588', total - filled) + "</color>";
    }

    string ConfidenceBar(float value)
    {
        int total = 10;
        int filled = Mathf.RoundToInt(value * total);
        string c = value < 0.3f ? "#3F51B5" : value < 0.7f ? "#00BCD4" : "#E3F2FD";
        return "<color=" + c + ">" + new string('\u2588', filled) + "</color>"
             + "<color=#333>" + new string('\u2588', total - filled) + "</color>";
    }
}