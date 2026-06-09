// Döner Empire 3D — Upgrade-Effekte für Shop & Konzern
// Port aus lib/services/game_engine.dart (_effectiveUpgradeIds,
// _upgradeCustomerBoost, _upgradeAvgOrderBoost, _upgradeReputationPerDay,
// _upgradeBrandPerDay, _upgradeDailyCost, globalUpgradeDailyCost,
// _deliveryCommissionCost, _hasDeliveryChannel).

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class UpgradeService
    {
        /// <summary>
        /// Effektive Upgrade-IDs: Shop-eigene + aktive globale Konzern-Upgrades
        /// (dedupliziert, verhindert Doppelzählung bei Legacy-Saves).
        /// </summary>
        public static List<string> EffectiveUpgradeIds(Shop shop, GameState state)
        {
            var ids = new HashSet<string>(shop.UpgradeIds);
            if (state != null) ids.UnionWith(state.GlobalUpgradeIds);
            return ids.ToList();
        }

        public static double CustomerBoost(Shop shop, GameState state)
            => EffectiveUpgradeIds(shop, state)
                .Select(UpgradeCatalog.ById).Where(u => u != null).Sum(u => u.CustomerBoost);

        public static double AvgOrderBoost(Shop shop, GameState state)
            => EffectiveUpgradeIds(shop, state)
                .Select(UpgradeCatalog.ById).Where(u => u != null).Sum(u => u.AvgOrderValueBoost);

        public static double ReputationPerDay(Shop shop, GameState state)
            => EffectiveUpgradeIds(shop, state)
                .Select(UpgradeCatalog.ById).Where(u => u != null).Sum(u => u.ReputationPerDay);

        public static double BrandPerDay(Shop shop, GameState state)
            => EffectiveUpgradeIds(shop, state)
                .Select(UpgradeCatalog.ById).Where(u => u != null).Sum(u => u.BrandPerDay);

        /// <summary>Tageskosten der NICHT-globalen Upgrades dieser Filiale.</summary>
        public static double ShopUpgradeDailyCost(Shop shop)
            => shop.UpgradeIds.Select(UpgradeCatalog.ById)
                .Where(u => u != null && !u.IsGlobal).Sum(u => u.DailyCost);

        /// <summary>Tageskosten aller globalen Upgrades (einmalig konzernweit).</summary>
        public static double GlobalUpgradeDailyCost(GameState state)
            => state.GlobalUpgradeIds.Select(UpgradeCatalog.ById)
                .Where(u => u != null).Sum(u => u.DailyCost);

        public static bool HasDeliveryChannel(Shop shop, GameState state)
            => EffectiveUpgradeIds(shop, state)
                .Select(UpgradeCatalog.ById).Any(u => u != null && u.IsDelivery);

        /// <summary>
        /// Tägliche Liefer-Provision = revenue × deliveryRevenueFraction × rate.
        /// Mit globalem "eigen_lieferdienst" sinkt die Rate auf 8 %.
        /// </summary>
        public static double DeliveryCommissionCost(Shop shop, double revenue, GameState state)
        {
            double cost = 0;
            var hasOwnApp = state?.GlobalUpgradeIds.Contains("eigen_lieferdienst") ?? false;
            foreach (var id in EffectiveUpgradeIds(shop, state))
            {
                var u = UpgradeCatalog.ById(id);
                if (u == null || !u.IsDelivery) continue;
                var rate = hasOwnApp ? 0.08 : u.DeliveryCommissionRate;
                cost += revenue * u.DeliveryRevenueFraction * rate;
            }
            return cost;
        }
    }
}
