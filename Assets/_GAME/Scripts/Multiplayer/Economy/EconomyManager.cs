using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;

namespace Aventra.Game
{
    public static class EconomyManager
    {
        private const string GOLD_CURRENCY_ID = "GOLD";
        private const string CHARACTER_ID = "CHARACTER_";
        private const string PURCHASE_ID = "PURCHASE";

        public static async Task<bool> CanAfford(long amount)
        {
            long goldBalance = await GetGoldAsync();
            if (goldBalance < amount)
            {
                return false;
            }
            return true;
        }

        public static async Task<long> GetGoldAsync()
        {
            try
            {
                GetBalancesResult r = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
                return r.Balances.FirstOrDefault(x => x.CurrencyId == GOLD_CURRENCY_ID)?.Balance ?? 0;
            }
            catch (EconomyException e)
            {
                throw new System.Exception($"Failed to get gold balance: {e}");
            }
        }

        public static async Task BuyCharacterAsync(ulong characterId)
        {
            try
            {
                string purchaseId = CHARACTER_ID + characterId + PURCHASE_ID;
                await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(purchaseId);
            }
            catch (EconomyException e)
            {
                throw new System.Exception($"Failed to purchase character {characterId}: {e}");
            }
        }

        public static async Task<bool> HasCharacterAsync(ulong characterId)
        {
            string itemId = CHARACTER_ID + characterId;
            try
            {
                GetInventoryResult r = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();
                return r.PlayersInventoryItems.Any(x => x.InventoryItemId == itemId);
            }
            catch (EconomyException e)
            {
                throw new System.Exception($"Failed to check if player has character {characterId}: {e}");
            }
        }

        public static async Task<int> GetCharacterGoldCostAsync(ulong characterIndex)
        {

            try
            {
                string purchaseId = CHARACTER_ID + characterIndex + PURCHASE_ID;
                Debug.Log($"Fetching gold cost for character {characterIndex} with purchase ID <color=yellow>{purchaseId}</color>");
                await EconomyService.Instance.Configuration.SyncConfigurationAsync();
                VirtualPurchaseDefinition purchase = EconomyService.Instance.Configuration.GetVirtualPurchase(purchaseId);

                if (purchase == null)
                {
                    throw new System.Exception($"Purchase definition not found for character {characterIndex}");
                }

                PurchaseItemQuantity goldCost = purchase.Costs.FirstOrDefault(
                    c => c.Item.GetReferencedConfigurationItem().Id == GOLD_CURRENCY_ID);
                
                if (goldCost == null)
                {
                    throw new System.Exception($"Gold cost not found for character {characterIndex}");
                }

                return goldCost.Amount;
            }
            catch (EconomyException e)
            {
                throw new System.Exception($"Failed to get gold cost for character {characterIndex}: {e}");
            }
        }

        public static async Task<bool> CanAffordCharacterAsync(ulong characterId)
        {
            int goldCost = await GetCharacterGoldCostAsync(characterId);
            return await CanAfford(goldCost);
        }
    }
}