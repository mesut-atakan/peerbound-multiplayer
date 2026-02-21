using System;
using System.Threading.Tasks;
using Aventra.Game.Singleton;
using Aventra.Nugget.Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class CharacterStoreMenu : BaseMenu
    {
        [SerializeField] private Button btnBuy;
        [SerializeField] private TMP_Text lblBuyBtn;
        [SerializeField] private Button btnBack;
        [SerializeField] private CharacterCatalog characterCatalog;
        [SerializeField] private CharacterSelectionButton characterSelectionButtonPrefab;
        [SerializeField] private Transform layout;
        [SerializeField] private Image imgSelectedCharacter;
        [SerializeField] private MainMenu mainMenu;

        private CharacterConfig _selectedCharacter;
        private CharacterSelectionButton[] _buttons;

        protected override void Awake()
        {
            base.Awake();
            imgSelectedCharacter.enabled = false;
        }

        void OnEnable()
        {
            btnBack.onClick.AddListener(OnBackClicked);
            btnBuy.onClick.AddListener(OnBuyClicked);
            MultiplayerServiceEvents.OnLoginSuccess += PopulateCharacterStore;
        }

        void OnDisable()
        {
            foreach (CharacterSelectionButton btn in _buttons)
            {
                btn.OnCharacterSelected -= OnCharacterSelected;
            }
            btnBack.onClick.RemoveListener(OnBackClicked);
            btnBuy.onClick.RemoveListener(OnBuyClicked);
            MultiplayerServiceEvents.OnLoginSuccess -= PopulateCharacterStore;
        }

        private async void OnBuyClicked()
        {
            if (_selectedCharacter.Id == default)
            {
                Debug.LogWarning("No character selected for purchase.");
                return;
            }
            btnBuy.interactable = false;
            if (!await EconomyManager.CanAffordCharacterAsync(_selectedCharacter.Id))
            {
                btnBuy.interactable = true;
                Debug.LogWarning("Not enough currency to buy this character.");
                return;
            }

            try
            {
                await EconomyManager.BuyCharacterAsync(_selectedCharacter.Id);
                PlayerConfig.Instance.SelectedCharacterId = _selectedCharacter.Id;
                OnBuyButtonUpdateVisual();
                Debug.Log($"Character {_selectedCharacter.Name} purchased successfully.");
            }
            catch
            {
                Debug.LogError("Failed to purchase character. Please try again.");
            }
            btnBuy.interactable = true;
        }

        private async void PopulateCharacterStore()
        {
            if (characterCatalog == null || characterSelectionButtonPrefab == null)
            {
                Debug.LogError("CharacterCatalog or CharacterSelectionButtonPrefab is not assigned.");
                return;
            }

            if (_buttons != null && _buttons.Length > 0)
            {
                Debug.LogWarning("Character store is already populated.");
                MultiplayerServiceEvents.RaiseCompleteLoadStore();
                return;
            }

            _buttons = new CharacterSelectionButton[characterCatalog.CharacterConfigs.Length];
            for (int i = 0; i < characterCatalog.CharacterConfigs.Length; i++)
            {
                var character = characterCatalog.CharacterConfigs[i];
                var buttonInstance = Instantiate(characterSelectionButtonPrefab, layout, false);
                await buttonInstance.SetCharacterInfo(character);
                buttonInstance.OnCharacterSelected += OnCharacterSelected;
                _buttons[i] = buttonInstance;
            }
            MultiplayerServiceEvents.RaiseCompleteLoadStore();
        }

        private async void OnCharacterSelected(CharacterConfig config)
        {
            if (config.Id == default || config.Icon == null)
            {
                imgSelectedCharacter.enabled = false;
                return;
            }

            imgSelectedCharacter.enabled = true;
            _selectedCharacter = config;
            imgSelectedCharacter.sprite = config.Icon;

            OnBuyButtonUpdateVisual();
        }

        private void OnBackClicked()
        {
            mainMenu.OpenMenu();
            CloseMenu();
        }

        private async void OnBuyButtonUpdateVisual()
        {
            bool hasCharacter = await EconomyManager.HasCharacterAsync(_selectedCharacter.Id);
            bool isSelected = PlayerConfig.Instance.SelectedCharacterId == _selectedCharacter.Id;

            if (hasCharacter)
            {
                lblBuyBtn.text = isSelected ? "Purchased" : "Select";
                btnBuy.interactable = !isSelected;
            }
            else
            {
                lblBuyBtn.text = $"Buy for {await _selectedCharacter.GetCurrencyCostAsync()} $";
                btnBuy.interactable = true;
            }
        }
    }
}