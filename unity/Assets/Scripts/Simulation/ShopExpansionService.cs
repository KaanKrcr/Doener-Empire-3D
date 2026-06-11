using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct ShopExpansionResult
    {
        public readonly bool Success;
        public readonly string ErrorMessage;
        public readonly double Cost;
        public readonly ShopSizeTier NewTier;

        private ShopExpansionResult(bool success, string errorMessage, double cost, ShopSizeTier newTier)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Cost = cost;
            NewTier = newTier;
        }

        public static ShopExpansionResult Expanded(double cost, ShopSizeTier newTier) =>
            new(true, null, cost, newTier);

        public static ShopExpansionResult Failed(string message) =>
            new(false, message, 0, ShopSizeTier.Klein);
    }

    public sealed class ShopExpansionService
    {
        public ShopExpansionResult ExpandToNextTier(GameState state, string shopId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(shopId))
            {
                return ShopExpansionResult.Failed("Filiale nicht gefunden.");
            }

            Shop shop = state.Shops.FirstOrDefault(candidate => candidate.Id == shopId);
            if (shop == null)
            {
                return ShopExpansionResult.Failed("Filiale nicht gefunden.");
            }

            ShopSizeTier? nextTier = ShopSizing.NextTier(shop.SizeTier);
            if (nextTier == null)
            {
                return ShopExpansionResult.Failed("Diese Filiale ist bereits maximal ausgebaut.");
            }

            CityTier cityTier = CityTierFor(shop.CityId);
            int cityCap = ShopSizing.CityEmployeeCap(cityTier);
            int nextTierCap = ShopSizing.ConfigFor(nextTier.Value).EmployeeCap;
            if (nextTierCap > cityCap)
            {
                return ShopExpansionResult.Failed("Diese Stadt traegt die naechste Ausbaustufe noch nicht.");
            }

            double cost = ShopSizing.ExpansionCost(shop.SizeTier);
            if (state.Cash < cost)
            {
                return ShopExpansionResult.Failed("Nicht genug Kapital fuer diesen Ausbau.");
            }

            state.Cash = Math.Round(state.Cash - cost, 2);
            shop.SizeTier = nextTier.Value;
            shop.Morale = Math.Clamp(shop.Morale + ShopSizing.ConfigFor(nextTier.Value).MoraleDeltaOnUpgrade, 0.2, 1.0);
            return ShopExpansionResult.Expanded(cost, nextTier.Value);
        }

        private static CityTier CityTierFor(string cityId)
        {
            CityData city = GameData.AllCities.FirstOrDefault(candidate => candidate.Id == cityId);
            return city?.Tier ?? CityTier.Klein;
        }
    }
}
