using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Aventra.Game
{
    [System.Serializable]
    public struct CharacterConfig
    {
        [field: SerializeField]
        public ulong Id { get; private set; }

        [SerializeField] private string name;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;
        public string Name => name;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;

        public async Task<long> GetCurrencyCostAsync()
        {
            try
            {
                return await EconomyManager.GetCharacterGoldCostAsync(Id);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get currency cost for character {Name} (ID: {Id}): {e}");
                return long.MaxValue; // Return a very high cost to indicate it's not purchasable
            }
        }
    }
}