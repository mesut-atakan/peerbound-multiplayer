using System;
using System.Collections.Generic;
using Aventra.Nugget.Common.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class MainMenu : BaseMenu
    {
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnStore;
        [SerializeField] private Button btnLogout;
        [SerializeField] private Button btnExit;
        [SerializeField] private List<BaseMenu> menus = new List<BaseMenu>();
        [SerializeField] private MultiplayerManagerMenu multiplayerManagerMenu;
        [SerializeField] private CharacterStoreMenu characterStoreMenu;
        [SerializeField] private AuthenticationMenu authenticationMenu;
        [SerializeField] private MainMenuUpperBar mainMenuUpperBar;

        private bool _loadedStore = false;

        protected override void Awake()
        {
            base.Awake();
            if (menus == null || menus.Count == 0)
            {
                Debug.LogWarning("No menus assigned to MainMenu. Please assign menus in the inspector.");
                return;
            }
            menus.Remove(this);
        }

        void OnEnable()
        {
            MultiplayerServiceEvents.OnLoginSuccess += OnLogin;
        }

        void OnDisable()
        {
            MultiplayerServiceEvents.OnLoginSuccess -= OnLogin;
        }


        public override void OpenMenu(float openMenuDelay = 0, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            base.OpenMenu(openMenuDelay, openMenuIncreaseDelta, OnStart, OnComplete);
            foreach (BaseMenu menu in menus)
            {
                if (menu == null)
                {
                    continue;
                }
                menu.OpenMenu();
            }
            MultiplayerServiceEvents.OnCompleteLoadStore += StoreLoadComplete;
            btnPlay.onClick.AddListener(OnPlayClicked);
            btnStore.onClick.AddListener(OnStoreClicked);
            btnLogout.onClick.AddListener(OnLogoutClicked);
            btnExit.onClick.AddListener(OnExitClicked);

            base._canvasGroup.interactable = _loadedStore;
        }

        public override void CloseMenu(float openMenuDelay = 0, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            base.CloseMenu(openMenuDelay, openMenuIncreaseDelta, OnStart, OnComplete);
            foreach (BaseMenu menu in menus)
            {
                if (menu == null)
                {
                    continue;
                }
                menu.CloseMenu();
            }
            MultiplayerServiceEvents.OnCompleteLoadStore -= StoreLoadComplete;
            btnPlay.onClick.RemoveListener(OnPlayClicked);
            btnStore.onClick.RemoveListener(OnStoreClicked);
            btnLogout.onClick.RemoveListener(OnLogoutClicked);
            btnExit.onClick.RemoveListener(OnExitClicked);
        }

        private void OnLogin()
        {
            base._canvasGroup.interactable = false;
        }

        private void StoreLoadComplete()
        {
            _loadedStore = true;
            base._canvasGroup.interactable = true;
        }

        private void OnPlayClicked()
        {
            multiplayerManagerMenu.OpenMenu();
            CloseMenu();
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnStoreClicked()
        {
            CloseMenu();
            characterStoreMenu.OpenMenu();
            mainMenuUpperBar.OpenMenu();
        }

        private async void OnLogoutClicked()
        {
            try
            {
                await AuthenticationManager.Logout();
                Debug.Log("Logout successful.");
                authenticationMenu.OpenMenu();
                CloseMenu();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Logout failed: {ex.Message}");
            }
        }
    }
}