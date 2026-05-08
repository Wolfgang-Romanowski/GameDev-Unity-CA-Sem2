using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace CA3.Networking {
public class AuthBootstrap : MonoBehaviour
    {
        public static AuthBootstrap Instance { get; private set; }

        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; }
        public string AccessToken { get; private set; }

        public event Action OnSignInSuccess;
        public event Action<string> OnSignInFailed;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            await SignInAsync();
        }

        private async Task SignInAsync()
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("[Auth] Unity Services initialised.");
                }

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    HandleSignedIn();
                    return;
                }

                AuthenticationService.Instance.SignedIn += HandleSignedIn;
                AuthenticationService.Instance.SignInFailed += HandleSignInFailed;

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Auth] Sign-in error: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
            }
        }

        private void HandleSignedIn()
        {
            IsSignedIn = true;
            PlayerId = AuthenticationService.Instance.PlayerId;
            AccessToken = AuthenticationService.Instance.AccessToken;

            Debug.Log(
                $"[Auth] Signed in successfully. " +
                $"PlayerId={PlayerId}, " +
                $"TokenLength={AccessToken?.Length ?? 0}");

            OnSignInSuccess?.Invoke();
        }

        private void HandleSignInFailed(RequestFailedException e)
        {
            Debug.LogError(
                $"[Auth] Sign-in failed. Code={e.ErrorCode}, Message={e.Message}");
            OnSignInFailed?.Invoke(e.Message);
        }

        private void OnDestroy()
        {
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= HandleSignedIn;
                AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;
            }
        }
    }
}