# CA2 Test Matrix

All tests performed with a two-client setup using a standalone build as the host and the Unity editor as the joining client.

| # | Scenario | Expected | Actual | Pass/Fail |
|---|----------|----------|--------|-----------|
| 1 | Host starts session | Session created, host capsule spawns via OnPlayerJoined | Works as expected, capsule visible at spawn position | Pass |
| 2 | Client joins session | Second capsule appears on both screens, InputAuthority assigned to joining client | Both capsules visible on both clients | Pass |
| 3 | Host walks into orb | Orb disappears on both clients because IsPickedUp is set to true by StateAuthority, host score increments | Orb hidden on both screens, score updated to 1 | Pass |
| 4 | Client walks into orb | Same behaviour, StateAuthority (host) still processes the collision and sets IsPickedUp | Orb hidden on both screens, client score updated | Pass |
| 5 | Both players reach same orb simultaneously | Only one pickup registered because HasStateAuthority guard prevents client-side processing and IsPickedUp check prevents double-trigger on host | Host processed it first, single score increment, no desync | Pass |
| 6 | Player walks over already-collected orb | Nothing happens because `if (IsPickedUp) return;` prevents re-collection | No effect, orb already hidden, no score change | Pass |
| 7 | All orbs collected | All orbs hidden on both clients via Render(), scores reflect correct totals | Consistent on both screens | Pass |
| 8 | Host moves with WASD | Movement synced to client via NetworkTransform | Synced correctly | Pass |
| 9 | Client moves with WASD | Movement synced to host via NetworkTransform, input routed through INetworkInput | Synced correctly | Pass |
| 10 | Client disconnects mid-session | Host continues running, OnPlayerLeft fires and calls Runner.Despawn on the client's NetworkObject | Client capsule removed from host scene | Pass |
