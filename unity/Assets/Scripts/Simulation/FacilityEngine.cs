// Döner Empire 3D — Corporate: Produktionsanlagen
// Port aus lib/services/corporate_engine.dart (Facility-Teil:
// buildFacility, facilityDailyCosts, facilityB2BRevenue, facilitySavingForShop).

using System;
using System.Collections.Generic;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class FacilityEngine
    {
        /// <summary>Baut eine Anlage, wenn Cash reicht. Mutiert State.</summary>
        public static GameState BuildFacility(GameState state, FacilityTemplate template)
        {
            if (state.Cash < template.BuildCost) return state;
            state.Cash -= template.BuildCost;
            state.Facilities.Add(new ProductionFacility
            {
                Id = $"fac_{DateTime.UtcNow.Ticks}",
                Type = template.Type,
                Tier = template.Tier,
                DayBuilt = state.CurrentDay,
            });
            return state;
        }

        /// <summary>Summe der täglichen Betriebskosten aller Anlagen.</summary>
        public static double FacilityDailyCosts(GameState state)
        {
            double total = 0;
            foreach (var f in state.Facilities)
                total += FacilityCatalog.Find(f.Type, f.Tier).DailyOperatingCost;
            return total;
        }

        /// <summary>Tagesumsatz aus B2B-Verkäufen (skaliert mit Marktgröße).</summary>
        public static double FacilityB2BRevenue(GameState state)
        {
            double total = 0;
            var marketDemand = Math.Clamp(state.Competitors.Count / 10.0, 0.3, 1.5);
            foreach (var f in state.Facilities)
                total += FacilityCatalog.Find(f.Type, f.Tier).B2BRevenuePerDay * marketDemand;
            return total;
        }

        /// <summary>
        /// Zutaten-Ersparnis (0..0.7) für eine Filiale durch die eigene
        /// Lieferkette. Pro Produktionstyp zählt der beste Tier; bei mehr
        /// Filialen als Tier-Kapazität wird anteilig abgedeckt.
        /// </summary>
        public static double FacilitySavingForShop(GameState state, Shop shop)
        {
            if (state.Facilities.Count == 0) return 0;

            var bestPerType = new Dictionary<ProductionType, FacilityTier>();
            foreach (var f in state.Facilities)
            {
                if (!bestPerType.TryGetValue(f.Type, out var cur) || (int)f.Tier > (int)cur)
                    bestPerType[f.Type] = f.Tier;
            }

            double totalSaving = 0;
            foreach (var kv in bestPerType)
            {
                var type = kv.Key;
                var tier = kv.Value;
                var maxShops = FacilityTierInfo.MaxShops(tier);
                if (state.Shops.Count <= maxShops)
                {
                    totalSaving += ProductionInfo.CostShareCovered(type)
                                 * FacilityTierInfo.IngredientSaving(tier);
                }
                else
                {
                    var coverage = (double)maxShops / state.Shops.Count;
                    totalSaving += ProductionInfo.CostShareCovered(type)
                                 * FacilityTierInfo.IngredientSaving(tier) * coverage;
                }
            }
            return Math.Clamp(totalSaving, 0.0, 0.7);
        }
    }
}
