// Döner Empire 3D — Combo-Effekte für einen Shop
// Port aus lib/services/game_engine.dart (shopSupportsCombo, _effectiveCombos,
// _comboCustomerBoost, _comboAvgOrderBoost, _comboReputationPerDay).

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class ComboService
    {
        /// <summary>True, wenn der Shop ALLE Produkte des Combos aktiv im Menü hat.</summary>
        public static bool ShopSupportsCombo(Shop shop, MenuCombo combo)
        {
            var active = shop.Menu.Where(p => p.IsActive).Select(p => p.ProductId).ToHashSet();
            return combo.ProductIds.All(active.Contains);
        }

        /// <summary>Aktive Combos, die in dieser Filiale tatsächlich greifen.</summary>
        public static IEnumerable<MenuCombo> EffectiveCombos(Shop shop, GameState state)
        {
            if (state == null) yield break;
            foreach (var id in state.ActiveComboIds)
            {
                var c = ComboData.ById(id);
                if (c != null && ShopSupportsCombo(shop, c)) yield return c;
            }
        }

        public static double CustomerBoost(Shop shop, GameState state)
            => EffectiveCombos(shop, state).Sum(c => c.CustomerBoost);

        public static double AvgOrderBoost(Shop shop, GameState state)
            => EffectiveCombos(shop, state).Sum(c => c.AvgOrderBoost);

        public static double ReputationPerDay(Shop shop, GameState state)
            => EffectiveCombos(shop, state).Sum(c => c.ReputationPerDay);
    }
}
