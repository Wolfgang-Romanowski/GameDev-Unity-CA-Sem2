using UnityEngine;

public enum GuardState { Patrol, Investigate, Chase, Search }

public class GuardAI : MonoBehaviour
{
    public GuardState CurrentState { get; private set; } = GuardState.Patrol;

    public void SetState(GuardState newState)
    {
        CurrentState = newState;
    }

    public Color GetStateColor()
    {
        return CurrentState switch
        {
            GuardState.Patrol      => Color.green,
            GuardState.Investigate => Color.yellow,
            GuardState.Chase       => Color.red,
            GuardState.Search      => new Color(1f, 0.5f, 0f),
            _                      => Color.white
        };
    }
}