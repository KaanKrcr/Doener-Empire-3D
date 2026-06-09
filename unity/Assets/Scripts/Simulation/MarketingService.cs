// Döner Empire 3D — Marketing-Kampagnen-Effekte
// Port aus lib/services/game_engine.dart (_activeCampaignBoost,
// _activeCampaignAvgOrderMod, _activeCityCampaignBoost, _activeGlobalCampaignBoost
// + Reputations-/Brand-Beiträge).

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class MarketingService
    {
        // ── Kundenzahl-Boost ─────────────────────────────────────────────────

        public static double ShopCampaignBoost(Shop shop, int day)
            => ActiveCampaignSum(shop.ActiveCampaigns, day, MarketingCatalog.ShopCampaigns,
                                 c => c.CustomerBoost);

        public static double ShopCampaignAvgOrderMod(Shop shop, int day)
            => ActiveCampaignSum(shop.ActiveCampaigns, day, MarketingCatalog.ShopCampaigns,
                                 c => c.AvgOrderValueMod);

        public static double CityCampaignBoost(Shop shop, int day, GameState state)
        {
            if (state == null) return 0;
            if (!state.ActiveCityCampaigns.TryGetValue(shop.CityId, out var list)) return 0;
            return ActiveCampaignSum(list, day, MarketingCatalog.All, c => c.CustomerBoost);
        }

        public static double GlobalCampaignBoost(int day, GameState state)
        {
            if (state == null) return 0;
            return ActiveCampaignSum(state.ActiveGlobalCampaigns, day, MarketingCatalog.All,
                                     c => c.CustomerBoost);
        }

        /// <summary>Gesamter Kunden-Boost aus allen Kampagnen-Ebenen für diesen Shop.</summary>
        public static double TotalCustomerBoost(Shop shop, int day, GameState state)
            => ShopCampaignBoost(shop, day)
             + CityCampaignBoost(shop, day, state)
             + GlobalCampaignBoost(day, state);

        // ── Reputations-Beitrag pro Tag ──────────────────────────────────────

        public static double ReputationPerDay(Shop shop, GameState state, int day)
        {
            double sum = ActiveCampaignSum(shop.ActiveCampaigns, day, MarketingCatalog.ShopCampaigns,
                                           c => c.ReputationBoostPerDay);
            if (state != null)
            {
                if (state.ActiveCityCampaigns.TryGetValue(shop.CityId, out var list))
                    sum += ActiveCampaignSum(list, day, MarketingCatalog.All, c => c.ReputationBoostPerDay);
                sum += ActiveCampaignSum(state.ActiveGlobalCampaigns, day, MarketingCatalog.All,
                                         c => c.ReputationBoostPerDay);
            }
            return sum;
        }

        // ── Brand-Awareness-Delta pro Tag (city + global) ────────────────────

        public static double BrandAwarenessDelta(GameState state, int day, IEnumerable<Shop> shops)
        {
            if (state == null) return 0;
            double sum = 0;
            var processedCities = new HashSet<string>();
            foreach (var shop in shops)
            {
                if (!processedCities.Add(shop.CityId)) continue;
                if (state.ActiveCityCampaigns.TryGetValue(shop.CityId, out var list))
                    sum += ActiveCampaignSum(list, day, MarketingCatalog.All, c => c.BrandAwarenessDelta);
            }
            sum += ActiveCampaignSum(state.ActiveGlobalCampaigns, day, MarketingCatalog.All,
                                     c => c.BrandAwarenessDelta);
            return sum;
        }

        // ── Helper ───────────────────────────────────────────────────────────

        private static double ActiveCampaignSum(
            IEnumerable<ActiveCampaign> active,
            int day,
            IReadOnlyList<MarketingCampaign> catalog,
            System.Func<MarketingCampaign, double> selector)
        {
            double sum = 0;
            foreach (var ac in active)
            {
                if (!ac.IsActive(day)) continue;
                var campaign = catalog.FirstOrDefault(c => c.Id == ac.CampaignId);
                if (campaign != null) sum += selector(campaign);
            }
            return sum;
        }
    }
}
