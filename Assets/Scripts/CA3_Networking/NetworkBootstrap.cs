using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Photon.Realtime;

namespace CA3.Networking
{
    public class NetworkBootstrap : MonoBehaviour, INetworkRunnerCallbacks
    {
        public enum ConnectionState
        {
            WaitingForAuth,
            Connecting,
            Connected,
            Refused,
            Disconnected
        }

        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private string sessionName = "CA3-Heist";

        public ConnectionState State { get; private set; } = ConnectionState.WaitingForAuth;
        public string LastError { get; private set; }

        private NetworkRunner runner;
        private readonly Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();

        private void Start()
        {
            if (AuthBootstrap.Instance == null)
            {
                FailWith("AuthBootstrap missing in scene");
                return;
            }

            if (AuthBootstrap.Instance.IsSignedIn)
                ConnectToFusion();
            else
            {
                AuthBootstrap.Instance.OnSignInSuccess += ConnectToFusion;
                AuthBootstrap.Instance.OnSignInFailed  += reason => FailWith("Auth failed: " + reason);
            }
        }

        private void OnDestroy()
        {
            if (AuthBootstrap.Instance != null)
                AuthBootstrap.Instance.OnSignInSuccess -= ConnectToFusion;
        }

        private async void ConnectToFusion()
        {
            if (runner != null) return;

            State = ConnectionState.Connecting;
            Debug.Log("[Net] Connecting to Photon (Shared mode)...");

            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);

            var auth = new AuthenticationValues
            {
                AuthType = CustomAuthenticationType.Custom,
                UserId   = AuthBootstrap.Instance.PlayerId
            };
            auth.AddAuthParameter("token", AuthBootstrap.Instance.AccessToken);

            var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode     = GameMode.Shared,
                SessionName  = sessionName,
                Scene        = sceneRef,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                AuthValues   = auth,
                PlayerCount  = 4
            });

            if (result.Ok)
            {
                State = ConnectionState.Connected;
                Debug.Log("[Net] Connected as " + runner.LocalPlayer);
            }
            else
            {
                FailWith("Refused: " + result.ShutdownReason);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player != runner.LocalPlayer) return;
            if (playerPrefab == default)      return;

            var obj = runner.Spawn(playerPrefab, GetSpawnPosition(player), Quaternion.identity, inputAuthority: player);
            spawnedPlayers[player] = obj;
            Debug.Log("[Net] Spawned local player");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (spawnedPlayers.TryGetValue(player, out var obj))
            {
                if (obj != null && obj.HasStateAuthority)
                    runner.Despawn(obj);
                spawnedPlayers.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;
            input.Set(data);
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
            => FailWith("Connect failed: " + reason);

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            State = ConnectionState.Disconnected;
            LastError = "Disconnected: " + reason;
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            State = ConnectionState.Disconnected;
            LastError = "Shutdown: " + shutdownReason;
        }

        private Vector3 GetSpawnPosition(PlayerRef player)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var sp = spawnPoints[player.RawEncoded % spawnPoints.Length];
                if (sp != null) return sp.position;
            }
            return new Vector3(player.RawEncoded % 4 * 2f - 3f, 1f, 0f);
        }

        private void FailWith(string reason)
        {
            State = ConnectionState.Refused;
            LastError = reason;
            Debug.LogError("[Net] " + reason);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}