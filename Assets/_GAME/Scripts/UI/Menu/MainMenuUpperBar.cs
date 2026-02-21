using System;
using Aventra.Nugget.Common.UI;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

namespace Aventra.Game
{
    public sealed class MainMenuUpperBar : BaseMenu
    {
        [SerializeField] private TMP_Text lblUserName;
        [SerializeField] private TMP_Text lblCurrencyAmount;

        public string UserName
        {
            get => lblUserName.text;
            set => lblUserName.text = value;
        }

        public string CurrencyAmount
        {
            get => lblCurrencyAmount.text;
            set => lblCurrencyAmount.text = value;
        }

        void OnEnable()
        {
            MultiplayerServiceEvents.OnLoginSuccess += UpdateUserInfo;
            MultiplayerServiceEvents.OnLogoutSuccess += ClearUserInfo;
        }

        void OnDisable()
        {
            MultiplayerServiceEvents.OnLoginSuccess -= UpdateUserInfo;
            MultiplayerServiceEvents.OnLogoutSuccess -= ClearUserInfo;
        }

        private void ClearUserInfo()
        {
            lblUserName.text = "Guest";
            lblCurrencyAmount.text = "0 $";
        }

        private async void UpdateUserInfo()
        {
            PlayerInfo userInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            string userName = userInfo?.Username ?? "User";
            lblUserName.text = userName;
            long currenxyGold = await EconomyManager.GetGoldAsync();
            CurrencyAmount = $"{currenxyGold} $";
        }
    }
}