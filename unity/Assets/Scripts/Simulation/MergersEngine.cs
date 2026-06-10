// Döner Empire 3D — Corporate: M&A (Konkurrenten aufkaufen)
// Port aus lib/services/corporate_engine.dart (acquisitionPrice, acquireCompetitor).

using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class MergersEngine
    {
        /// <summary>Akquisitionspreis: 60.000€ × Filialen × Reputations-Faktor.</summary>
        public static double AcquisitionPrice(Competitor c)
        {
            var repFactor = Math.Clamp(c.Reputation / 3.0, 0.7, 1.6);
            return c.ShopCount * 60000 * repFactor;
        }

        /// <summary>
        /// Kauft einen Konkurrenten auf: seine Filialen werden zu Player-Shops
        /// (Default-Menü, übernommene Reputation), der Konkurrent verschwindet.
        /// Mutiert State; No-Op wenn Cash nicht reicht.
        /// </summary>
        public static GameState AcquireCompetitor(GameState state, Competitor c)
        {
            var price = AcquisitionPrice(c);
            if (state.Cash < price) return state;

            var city = GameData.AllCities.FirstOrDefault(x => x.Id == c.CityId)
                       ?? GameData.AllCities[0];
            var locTemplates = GameCatalog.LocationTemplates.TryGetValue(city.Tier, out var t)
                ? t
                : GameCatalog.LocationTemplates[CityTier.Klein];

            for (var i = 0; i < c.ShopCount; i++)
            {
                var loc = locTemplates[i % locTemplates.Count];
                var ft = (int)Math.Round(city.FootTrafficBase * loc.FootTrafficFactor);
                var rent = city.RentBase * loc.RentFactor;

                var shop = new Shop
                {
                    Id = $"aq_{c.Id}_{i}",
                    Name = state.CompanyName,
                    CustomName = null,
                    CityId = c.CityId,
                    LocationName = loc.Name,
                    FootTraffic = ft,
                    WeeklyRent = rent,
                    Reputation = c.Reputation,
                    DayOpened = state.CurrentDay,
                    Personality = loc.Personality,
                    OriginalCompetitorName = c.Name,
                    WasAcquired = true,
                };
                shop.Menu.AddRange(GameData.AllProducts
                    .Where(p => p.IsDefault)
                    .Select(p => new ShopProduct { ProductId = p.Id, Price = p.BasePrice }));
                state.Shops.Add(shop);
            }

            state.Cash -= price;
            state.Competitors.RemoveAll(x => x.Id == c.Id);
            return state;
        }
    }
}
