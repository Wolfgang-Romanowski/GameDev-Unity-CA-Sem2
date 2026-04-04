using Fusion;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    //only state authority can change this value
    [Networked] public NetworkBool IsPickedUp { get; set; }

    private Renderer _renderer;
    private Collider _collider;

    public override void Spawned()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }

    //render() called every frame
    public override void Render()
    {
        //all clients read networked state and update visuals
        _renderer.enabled = !IsPickedUp;
        _collider.enabled = !IsPickedUp;
    }

    //called on the host when a player collider enters trigger
    private void OnTriggerEnter(Collider other)
    {
        //only the host processes pickups to prevent dupe pickups
        if (!Object.HasStateAuthority) return;

        if (IsPickedUp) return;

        //if the collider belongs to a player
        PlayerScore playerScore = other.GetComponent<PlayerScore>();
        if (playerScore != null)
        {
            IsPickedUp = true;
            playerScore.Score += 1;
        }
    }
}