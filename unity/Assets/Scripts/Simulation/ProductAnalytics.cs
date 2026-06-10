// Döner Empire 3D — Produkt-Profitabilität (Stats-Screen)
// Port aus lib/services/game_engine.dart (ProductProfit, productProfitBreakdown).
//
// Verteilt die Tageskunden je Shop anteilig nach Preis-Nachfrage-Gewichtung
// auf die aktiven Produkte und summiert konzernweit. Brutto-Zutatenkosten
// (Ersparnisse wirken gleichmäßig und ändern das Ranking nicht).

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class ProductProfit
    {
        public string ProductId;
        public double Units;
        public double Revenue;
        public double IngredientCost;

        public double Profit => Revenue - IngredientCost;
        public double Margin => Revenue > 0 ? Profit / Revenue : 0;
    }

    public static class ProductAnalytics
    {
        public static List<ProductProfit> ProductProfitBreakdown(GameState state)
        {
            var agg = new Dictionary<string, ProductProfit>();
            foreach (var shop in state.Shops)
            {
                var stats = GameEngineCore.CalculateShopStats(shop, state.CurrentDay, state);
                var customers = stats.ActualCustomers;
                if (customers <= 0) continue;

                var activeMenu = shop.Menu.Where(p => p.IsActive).ToList();
                var weights = new Dictionary<string, double>();
                double totalW = 0;
                foreach (var sp in activeMenu)
                {
                    var pd = ProductData(sp.ProductId);
                    if (pd == null) continue;
                    var w = DemandUtils.PriceDemandFactor(sp.Price, pd.BasePrice, state.Difficulty);
                    weights[sp.ProductId] = w;
                    totalW += w;
                }
                if (totalW <= 0) continue;

                foreach (var sp in activeMenu)
                {
                    var pd = ProductData(sp.ProductId);
                    if (pd == null) continue;
                    var w = weights.TryGetValue(sp.ProductId, out var wv) ? wv : 0;
                    var units = customers * (w / totalW);
                    var revenue = units * sp.Price;
                    var ingredientCost = units * pd.IngredientCostPerUnit;

                    if (!agg.TryGetValue(sp.ProductId, out var entry))
                    {
                        entry = new ProductProfit { ProductId = sp.ProductId };
                        agg[sp.ProductId] = entry;
                    }
                    entry.Units += units;
                    entry.Revenue += revenue;
                    entry.IngredientCost += ingredientCost;
                }
            }
            var list = agg.Values.ToList();
            list.Sort((a, b) => b.Profit.CompareTo(a.Profit));
            return list;
        }

        private static ProductData ProductData(string productId)
            => GameData.AllProducts.FirstOrDefault(p => p.Id == productId);
    }
}
