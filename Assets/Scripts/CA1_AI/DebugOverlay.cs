using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private GuardAI guard;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private GameObject overlayRoot;

    void Update()
    {
        // f1 toggles the debug panel
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            overlayRoot.SetActive(!overlayRoot.activeSelf);

        if (!overlayRoot.activeSelf || guard == null) return;

        string stateColor = guard.CurrentState switch
        {
            GuardState.Patrol => "#4CAF50",
            GuardState.Investigate => "#FFC107",
            GuardState.Chase => "#F44336",
            GuardState.Search => "#FF9800",
            _ => "#FFFFFF"
        };

        Vector3 dest = guard.CurrentDestination;
        string seePlayer = guard.CanSeePlayer
            ? "<color=#F44336>TRUE</color>"
            : "<color=#4CAF50>FALSE</color>";

        debugText.text =
            $"<b>GUARD AI DEBUG</b>\n" +
            $"<color=#888888>──────────────</color>" +
            $" State            <b><color={stateColor}>{guard.CurrentState}</color></b>\n" +
            $" Target           <b>{guard.CurrentTarget}</b>\n" +
            $" Destination   <b>({dest.x:F1}, {dest.z:F1})</b>\n" +
            $" Sees Player   {seePlayer}\n" +
            $"<color=#888888>──────────────</color>";
    }
}