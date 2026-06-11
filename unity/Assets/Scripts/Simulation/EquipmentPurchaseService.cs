using System;
using System.Linq;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct EquipmentPurchaseResult
    {
        public readonly bool Success;
        public readonly string ErrorMessage;
        public readonly double Cost;
        public readonly string EquipmentId;

        private EquipmentPurchaseResult(bool success, string errorMessage, double cost, string equipmentId)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Cost = cost;
            EquipmentId = equipmentId;
        }

        public static EquipmentPurchaseResult Purchased(double cost, string equipmentId) =>
            new(true, null, cost, equipmentId);

        public static EquipmentPurchaseResult Failed(string message) =>
            new(false, message, 0, null);
    }

    public sealed class EquipmentPurchaseService
    {
        public EquipmentPurchaseResult BuyEquipment(GameState state, string shopId, string equipmentId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(shopId))
            {
                return EquipmentPurchaseResult.Failed("Filiale nicht gefunden.");
            }

            Shop shop = state.Shops.FirstOrDefault(candidate => candidate.Id == shopId);
            if (shop == null)
            {
                return EquipmentPurchaseResult.Failed("Filiale nicht gefunden.");
            }

            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                return EquipmentPurchaseResult.Failed("Equipment nicht gefunden.");
            }

            EquipmentData equipment = GameCatalog.AllEquipment.FirstOrDefault(candidate => candidate.Id == equipmentId);
            if (equipment == null)
            {
                return EquipmentPurchaseResult.Failed("Equipment nicht gefunden.");
            }

            if (shop.HasEquipment(equipmentId))
            {
                return EquipmentPurchaseResult.Failed("Equipment ist bereits installiert.");
            }

            if (state.Cash < equipment.Price)
            {
                return EquipmentPurchaseResult.Failed("Nicht genug Kapital fuer dieses Equipment.");
            }

            state.Cash = Math.Round(state.Cash - equipment.Price, 2);
            shop.Equipment.Add(new ShopEquipment { EquipmentId = equipment.Id });
            return EquipmentPurchaseResult.Purchased(equipment.Price, equipment.Id);
        }
    }
}
