using Fusion;
using UnityEngine;

// This struct defines what input data we send to the server
public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
}

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    public override void FixedUpdateNetwork()
    {
        //only process input if client has input authority over object
        if (GetInput(out NetworkInputData data))
        {
            Vector3 move = data.direction.normalized * moveSpeed * Runner.DeltaTime;
            transform.position += move;
        }
    }
}