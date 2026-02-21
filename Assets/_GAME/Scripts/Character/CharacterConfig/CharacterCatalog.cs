using UnityEngine;

namespace Aventra.Game
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
    public sealed class CharacterCatalog : ScriptableObject
    {
        private const string FILE_NAME = nameof(CharacterCatalog);
        private const string MENU_NAME = "Aventra/Character/Character Catalog";

        [field: SerializeField] 
        public CharacterConfig[] CharacterConfigs { get; private set; }

        public CharacterConfig GetFirstCharacterConfig()
        {
            if (CharacterConfigs == null || CharacterConfigs.Length == 0)
            {
                Debug.LogError("Character Catalog is empty!");
                return default;
            }
            return CharacterConfigs[0];
        }

        public bool TryGetCharacterConfig(ulong id, out CharacterConfig config)
        {
            foreach (var characterConfig in CharacterConfigs)
            {
                if (characterConfig.Id == id)
                {
                    config = characterConfig;
                    return true;
                }
            }
            config = default;
            Debug.LogWarning($"Character with ID {id} not found in catalog.");
            return false;
        }

        public CharacterConfig GetCharacterConfig(ulong id)
        {
            if (TryGetCharacterConfig(id, out var config))
            {
                return config;
            }
            Debug.LogError($"Character with ID {id} not found. Returning default config.");
            return default;
        }
    }
}