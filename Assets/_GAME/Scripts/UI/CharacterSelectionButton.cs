using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game
{
    public class CharacterSelectionButton : MonoBehaviour
    {
        public Action<CharacterConfig> OnCharacterSelected;
        [SerializeField] private TMP_Text lblCharacterName;
        [SerializeField] private TMP_Text lblCurrencyAmount;
        [SerializeField] private Image imgCharacterIcon;

        private CharacterConfig _characterConfig;

        private Button _btn;
        private long currency;

        void Awake()
        {
            _btn = GetComponent<Button>();
        }

        void OnEnable()
        {
            _btn.onClick.AddListener(OnButtonClicked);
        }


        void OnDisable()
        {
            _btn.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnCharacterSelected?.Invoke(_characterConfig);
        }

        public async Task SetCharacterInfo(CharacterConfig characterConfig)
        {
            _characterConfig = characterConfig;
            lblCharacterName.text = characterConfig.Name;
            imgCharacterIcon.sprite = characterConfig.Icon;

            bool hasCharacter = await EconomyManager.HasCharacterAsync(characterConfig.Id);
            currency = await characterConfig.GetCurrencyCostAsync();
            lblCurrencyAmount.text = hasCharacter ? $"Owned" : $"{currency} $";

            Debug.Log($"Character {characterConfig.Name} costs {currency} currency.");
            bool canAfford = await EconomyManager.CanAfford(currency);
        }
    }
}