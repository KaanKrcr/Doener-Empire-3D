// Tests für MarketingService + MarketingCatalog + Integration.
// Spiegelt lib/services/game_engine.dart (_activeCampaign*Boost/Mod) + Katalog.

using System.Collections.Generic;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class MarketingServiceTests
    {
        private static GameState State()
            => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop Shop(string cityId = "fulda")
        {
            var s = new Shop { Id = "s1", CityId = cityId, IsOpen = true, Reputation = 3.0 };
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            return s;
        }

        private static ActiveCampaign Active(string id, int start = 0, int end = 100)
            => new() { CampaignId = id, StartDay = start, EndDay = end };

        // ── Catalog ──────────────────────────────────────────────────────────

        [Fact]
        public void CatalogCombinesAllScopes()
        {
            Assert.Equal(
                MarketingCatalog.ShopCampaigns.Count
                + MarketingCatalog.CityCampaigns.Count
                + MarketingCatalog.GlobalCampaigns.Count,
                MarketingCatalog.All.Count);
        }

        [Fact]
        public void ByIdFindsKnownAndUnknown()
        {
            Assert.NotNull(MarketingCatalog.ById("flyer_local"));
            Assert.NotNull(MarketingCatalog.ById("tv_werbung"));
            Assert.Null(MarketingCatalog.ById("nope"));
        }

        [Fact]
        public void CostPerDayComputed()
        {
            var social = MarketingCatalog.ById("social_media"); // 1500 / 5
            Assert.Equal(300, social.CostPerDay, precision: 5);
        }

        // ── Shop campaign boost ──────────────────────────────────────────────

        [Fact]
        public void ShopCampaignBoostOnlyWhenActive()
        {
            var shop = Shop();
            shop.ActiveCampaigns.Add(Active("social_media", 0, 5)); // boost 0.30
            Assert.Equal(0.30, MarketingService.ShopCampaignBoost(shop, 2), precision: 5);
            Assert.Equal(0.0, MarketingService.ShopCampaignBoost(shop, 5)); // expired (exclusive)
        }

        [Fact]
        public void ShopCampaignAvgOrderModNegativeForDeal()
        {
            var shop = Shop();
            shop.ActiveCampaigns.Add(Active("two_for_one", 0, 2)); // aov -0.40
            Assert.Equal(-0.40, MarketingService.ShopCampaignAvgOrderMod(shop, 1), precision: 5);
        }

        [Fact]
        public void MultipleShopCampaignsStack()
        {
            var shop = Shop();
            shop.ActiveCampaigns.Add(Active("flyer_local")); // 0.15
            shop.ActiveCampaigns.Add(Active("radio_spot"));  // 0.35
            Assert.Equal(0.50, MarketingService.ShopCampaignBoost(shop, 1), precision: 5);
        }

        // ── City + global ────────────────────────────────────────────────────

        [Fact]
        public void CityCampaignBoostAppliesToCityShops()
        {
            var state = State();
            var shop = Shop("fulda");
            state.ActiveCityCampaigns["fulda"] = new List<ActiveCampaign> { Active("city_plakat") }; // 0.15
            Assert.Equal(0.15, MarketingService.CityCampaignBoost(shop, 1, state), precision: 5);

            // Shop in anderer Stadt profitiert nicht
            var other = Shop("berlin");
            Assert.Equal(0.0, MarketingService.CityCampaignBoost(other, 1, state));
        }

        [Fact]
        public void GlobalCampaignBoostAppliesEverywhere()
        {
            var state = State();
            state.ActiveGlobalCampaigns.Add(Active("tv_werbung")); // 0.20
            Assert.Equal(0.20, MarketingService.GlobalCampaignBoost(1, state), precision: 5);
        }

        [Fact]
        public void TotalCustomerBoostSumsAllScopes()
        {
            var state = State();
            var shop = Shop("fulda");
            shop.ActiveCampaigns.Add(Active("flyer_local"));                              // 0.15
            state.ActiveCityCampaigns["fulda"] = new List<ActiveCampaign> { Active("city_plakat") }; // 0.15
            state.ActiveGlobalCampaigns.Add(Active("tv_werbung"));                        // 0.20
            Assert.Equal(0.50, MarketingService.TotalCustomerBoost(shop, 1, state), precision: 5);
        }

        // ── Reputation + Brand ───────────────────────────────────────────────

        [Fact]
        public void ReputationPerDaySumsScopes()
        {
            var state = State();
            var shop = Shop("fulda");
            shop.ActiveCampaigns.Add(Active("social_media")); // rep 0.08
            state.ActiveGlobalCampaigns.Add(Active("brand_launch")); // rep 0.04
            var rep = MarketingService.ReputationPerDay(shop, state, 1);
            Assert.Equal(0.12, rep, precision: 5);
        }

        [Fact]
        public void BrandDeltaCountsCityOncePerCity()
        {
            var state = State();
            var shops = new List<Shop> { Shop("fulda"), Shop("fulda") }; // selbe Stadt
            state.ActiveCityCampaigns["fulda"] = new List<ActiveCampaign> { Active("city_social") }; // 0.3
            state.ActiveGlobalCampaigns.Add(Active("tv_werbung")); // 1.5
            var delta = MarketingService.BrandAwarenessDelta(state, 1, shops);
            // city nur 1× trotz 2 Shops + global → 0.3 + 1.5
            Assert.Equal(1.8, delta, precision: 5);
        }

        // ── Integration ──────────────────────────────────────────────────────

        [Fact]
        public void CampaignRaisesCustomersInShopStats()
        {
            var stateNo = State();
            var shop = Shop();
            shop.FootTraffic = 3000; shop.Reputation = 3.5;
            shop.Employees.Add(new Employee { Id = "e", TypeId = "kassierer",
                Speed = 8, Friendliness = 6, Reliability = 6, Experience = 5, SalaryPerDay = 65 });
            stateNo.Shops.Add(shop);

            var stateYes = stateNo.Clone();
            stateYes.Shops[0].ActiveCampaigns.Add(Active("radio_spot", 0, 100)); // 0.35

            double rno = 0, ryes = 0;
            for (var d = 1; d <= 20; d++)
            {
                rno += GameEngineCore.CalculateShopStats(stateNo.Shops[0], d, stateNo).ActualCustomers;
                ryes += GameEngineCore.CalculateShopStats(stateYes.Shops[0], d, stateYes).ActualCustomers;
            }
            Assert.True(ryes >= rno, $"no={rno} yes={ryes}");
        }

        [Fact]
        public void TwoForOneLowersAvgOrderValue()
        {
            var state = State();
            var shop = Shop();
            shop.FootTraffic = 2000;
            shop.Employees.Add(new Employee { Id = "e", TypeId = "kassierer",
                Speed = 8, Friendliness = 6, Reliability = 6, Experience = 5, SalaryPerDay = 65 });
            state.Shops.Add(shop);

            var aovBefore = GameEngineCore.CalculateShopStats(shop, 1, state).AvgOrderValue;
            shop.ActiveCampaigns.Add(Active("two_for_one", 0, 100)); // aov -0.40
            var aovAfter = GameEngineCore.CalculateShopStats(shop, 1, state).AvgOrderValue;
            Assert.True(aovAfter < aovBefore, $"before={aovBefore} after={aovAfter}");
        }

        [Fact]
        public void GlobalCampaignBrandShowsInUpdateBrand()
        {
            var state = State();
            var shops = new List<Shop> { Shop() };
            var baseBrand = DayProcessing.UpdateBrand(state, 1000, 100, shops);
            state.ActiveGlobalCampaigns.Add(Active("influencer_national", 0, 100)); // brand 2.0
            var withCampaign = DayProcessing.UpdateBrand(state, 1000, 100, shops);
            Assert.True(withCampaign.BrandAwareness > baseBrand.BrandAwareness);
        }

        [Fact]
        public void StateCloneCopiesCampaigns()
        {
            var state = State();
            state.ActiveCityCampaigns["fulda"] = new List<ActiveCampaign> { Active("city_plakat") };
            state.ActiveGlobalCampaigns.Add(Active("tv_werbung"));
            var clone = state.Clone();
            Assert.NotSame(state.ActiveGlobalCampaigns, clone.ActiveGlobalCampaigns);
            Assert.Single(clone.ActiveGlobalCampaigns);
            Assert.True(clone.ActiveCityCampaigns.ContainsKey("fulda"));
            Assert.Single(clone.ActiveCityCampaigns["fulda"]);
        }
    }
}
