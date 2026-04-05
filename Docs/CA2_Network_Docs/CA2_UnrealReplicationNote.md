# Unreal Replication Terminology

## Authority Roles

In Fusion, authority is split into two roles. StateAuthority is the entity permitted to write to `[Networked]` properties, which in a Host/Client topology is always the host. InputAuthority designates which client provides input for a given NetworkObject, so each player only controls their own character. In my implementation `Spawner.OnPlayerJoined()` calls `Runner.Spawn()` with the joining player as the fourth parameter, which assigns InputAuthority to that client.

Unreal maps these concepts to three roles. The Server holds authority over all replicated actors and is the only entity that can authoritatively modify replicated state, which is functionally identical to Fusion's StateAuthority. The Owning Client owns a PlayerController and its associated Pawn and is the only client permitted to send input to the server for that actor, which maps directly to InputAuthority. Simulated Proxies are all other clients observing a remotely-controlled actor. They receive replicated state but cannot write to it or provide input. Fusion does not have an explicit Simulated Proxy role. Instead any client without InputAuthority on an object simply receives its replicated state through `[Networked]` properties and NetworkTransform.

## [Networked] vs Replicated UPROPERTY

Fusion's `[Networked]` attribute marks a property for automatic synchronisation from StateAuthority to all clients. In my PickupItem script `[Networked] public NetworkBool IsPickedUp` is automatically replicated whenever the host changes it. Unreal's equivalent is a UPROPERTY marked with the Replicated specifier, combined with registering the property in `GetLifetimeReplicatedProps()` using the `DOREPLIFETIME` macro. Both achieve the same thing but Unreal requires more boilerplate through the registration macro and function override, where Fusion handles this through code generation at compile time.

Unreal also offers RepNotify which triggers a callback function on clients when a replicated property changes. Fusion has a similar mechanism with the `OnChangedRender` attribute that can react to `[Networked]` property changes, though I did not use it in my implementation since I check the property value directly in `Render()` each frame.

## RPCs

Fusion supports RPCs that can target StateAuthority, InputAuthority, or all clients. Unreal provides three RPC types that map to these. A Server RPC marked with `UFUNCTION(Server)` is called on a client and executed on the server, equivalent to a Fusion RPC targeting StateAuthority. A Client RPC marked with `UFUNCTION(Client)` is called on the server and executed on the owning client, mapping to a Fusion RPC targeting InputAuthority. A NetMulticast RPC marked with `UFUNCTION(NetMulticast)` is called on the server and executed on all clients, equivalent to a Fusion RPC targeting all.

In both engines RPCs should be used for transient events like sound triggers or visual effects rather than as a substitute for replicated state, since RPCs are fire-and-forget and will not be received by late-joining clients.

## Gotcha

The most significant difference I found is in the simulation model. Fusion runs a tick-based deterministic simulation where all networked logic must go in `FixedUpdateNetwork()` at a fixed tick rate. Clients can run prediction ahead of the server. Unreal uses an actor-based replication model where the server replicates entire actor states at a variable rate determined by NetUpdateFrequency and clients do not predict by default.

In practice this means Fusion developers must be careful to put all game logic in `FixedUpdateNetwork()` instead of `Update()`. In my PlayerMovement script movement is calculated in `FixedUpdateNetwork()` because that is part of the deterministic simulation, while `Update()` runs at the render frame rate and would cause inconsistencies between clients. In Unreal this separation does not exist in the same way because game logic runs in `Tick()` on both server and client and replication handles synchronisation. Porting networking code between the two engines would mean rethinking where logic lives rather than just translating API names.