using System;
using Aventra.Game.Multiplayer;
using Aventra.Nugget.Common.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class MultiplayerManagerMenu : BaseMenu
    {
        [SerializeField] private P2PSessionService p2pSessionService;
        [SerializeField] private Button btnBackToMainMenu;
        [SerializeField] private BaseMenu mainMenu;
        [Header("Create Host")]
        [SerializeField] private Button btnCreateLobby;

        [Header("Join Lobby")]
        [SerializeField] private TMP_InputField inputJoinCode;
        [SerializeField] private Button btnJoinLobby;

        [Header("Feedback")]
        [SerializeField] private TMP_Text txtLobbyCode;
        [SerializeField] private TMP_Text txtStatus;

        private bool _isBusy;

        protected override void Awake()
        {
            base.Awake();

            if (p2pSessionService == null)
            {
                p2pSessionService = FindAnyObjectByType<P2PSessionService>();
            }
        }

        void OnEnable()
        {
            if (btnCreateLobby != null)
                btnCreateLobby.onClick.AddListener(OnCreateLobbyClicked);

            if (btnJoinLobby != null)
                btnJoinLobby.onClick.AddListener(OnJoinLobbyClicked);

            if (btnBackToMainMenu != null)
                btnBackToMainMenu.onClick.AddListener(OnBackToMainMenuClicked);
        }

        void OnDisable()
        {
            if (btnCreateLobby != null)
                btnCreateLobby.onClick.RemoveListener(OnCreateLobbyClicked);

            if (btnJoinLobby != null)
                btnJoinLobby.onClick.RemoveListener(OnJoinLobbyClicked);

            if (btnBackToMainMenu != null)
                btnBackToMainMenu.onClick.RemoveListener(OnBackToMainMenuClicked);
            }

        private void OnBackToMainMenuClicked()
        {
            if (mainMenu != null)
                mainMenu.OpenMenu();
            CloseMenu();
        }

        private async void OnCreateLobbyClicked()
        {
            if (_isBusy || IsNetworkAlreadyRunning())
                return;

            if (!TryValidateService())
                return;

            SetBusy(true);

            try
            {
                string joinCode = await p2pSessionService.StartHostAsync();
                SetLobbyCodeText(joinCode);
                SetStatus($"Host created. Share join code: {joinCode}");
                LoadGameSceneAsHost();
            }
            catch (Exception ex)
            {
                SetStatus($"Lobby create failed: {ex.GetBaseException().Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnJoinLobbyClicked()
        {
            if (_isBusy || IsNetworkAlreadyRunning())
                return;

            if (!TryValidateService())
                return;

            string joinCode = inputJoinCode != null ? inputJoinCode.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                SetStatus("Enter a valid join code.");
                return;
            }

            SetBusy(true);

            try
            {
                await p2pSessionService.StartClientAsync(joinCode);
                SetStatus("Joining lobby...");
            }
            catch (Exception ex)
            {
                SetStatus($"Join failed: {ex.GetBaseException().Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool TryValidateService()
        {
            if (p2pSessionService != null)
                return true;

            SetStatus("P2P session service is missing.");
            return false;
        }

        private bool IsNetworkAlreadyRunning()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return false;

            SetStatus("Network session is already running.");
            return true;
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;

            if (btnCreateLobby != null)
                btnCreateLobby.interactable = !value;

            if (btnJoinLobby != null)
                btnJoinLobby.interactable = !value;
        }

        private void LoadGameSceneAsHost()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            var loadStatus = NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            if (loadStatus != SceneEventProgressStatus.Started)
            {
                SetStatus($"Game scene load failed to start: {loadStatus}");
            }
        }

        private void SetLobbyCodeText(string joinCode)
        {
            if (txtLobbyCode == null)
                return;

            txtLobbyCode.text = joinCode;
        }

        private void SetStatus(string status)
        {
            Debug.Log(status);

            if (txtStatus == null)
                return;

            txtStatus.text = status;
        }
    }
}