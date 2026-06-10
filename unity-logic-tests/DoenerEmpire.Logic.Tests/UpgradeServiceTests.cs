// Tests für UpgradeService + UpgradeCatalog + Integration in GameEngineCore.
// Spiegelt lib/services/game_engine.dart (Upgrade-Helfer + Delivery-Provision).

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class UpgradeServiceTests
    {
        private static GameState State()
            => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop Shop(params string[] upgradeIds)
        {
            var s = new Shop { Id = "s1", CityId = "fulda", IsOpen = true, Reputation = 3.0 };
            s.UpgradeIds.AddRange(upgradeIds);
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            return s;
        }

        // ── Catalog ──────────────────────────────────────────────────────────

        [Fact]
        public void CatalogHasShopAndGlobalUpgrades()
        {
            Assert.NotEmpty(UpgradeCatalog.ShopUpgrades);
            Assert.NotEmpty(UpgradeCatalog.GlobalUpgrades);
            Assert.Equal(
                UpgradeCatalog.ShopUpgrades.Count + UpgradeCatalog.GlobalUpgrades.Count,
                UpgradeCatalog.All.Count);
        }

        [Fact]
        public void ByIdFindsKnownUnknown()
        {
            Assert.NotNull(UpgradeCatalog.ById("wifi"));
            Assert.Null(UpgradeCatalog.ById("nonexistent"));
        }

        [Fact]
        public void DailyCostIsMonthlyDiv30()
        {
            var wifi = UpgradeCatalog.ById("wifi"); // monthly 90
            Assert.Equal(3.0, wifi.DailyCost, precision: 5);
        }

        [Fact]
        public void LieferdienstIsDeliveryAndGlobal()
        {
            var d = UpgradeCatalog.ById("lieferdienst");
            Assert.True(d.IsDelivery);
            Assert.True(d.IsGlobal);
        }

        // ── EffectiveUpgradeIds ──────────────────────────────────────────────

        [Fact]
        public void EffectiveMergesShopAndGlobalDeduped()
        {
            var state = State();
            state.GlobalUpgradeIds.Add("loyalty_app");
            state.GlobalUpgradeIds.Add("wifi");  // auch im Shop → dedupe
            var shop = Shop("wifi", "klima");
            var ids = UpgradeService.EffectiveUpgradeIds(shop, state);
            Assert.Contains("loyalty_app", ids);
            Assert.Contains("klima", ids);
            Assert.Equal(1, ids.Count(i => i == "wifi"));
        }

        // ── Boosts ───────────────────────────────────────────────────────────

        [Fact]
        public void CustomerBoostSumsUpgrades()
        {
            var shop = Shop("wifi", "klima"); // 0.06 + 0.08
            Assert.Equal(0.14, UpgradeService.CustomerBoost(shop, State()), precision: 5);
        }

        [Fact]
        public void AvgOrderBoostSumsUpgrades()
        {
            var shop = Shop("tv_sport", "kartenzahlung"); // 0.08 + 0.06
            Assert.Equal(0.14, UpgradeService.AvgOrderBoost(shop, State()), precision: 5);
        }

        [Fact]
        public void ReputationPerDaySums()
        {
            var shop = Shop("premium_reinigung"); // 0.015
            Assert.Equal(0.015, UpgradeService.ReputationPerDay(shop, State()), precision: 5);
        }

        [Fact]
        public void BrandPerDayFromGlobalUpgrade()
        {
            var state = State();
            state.GlobalUpgradeIds.Add("social_media_team"); // brand 0.06
            var shop = Shop();
            Assert.Equal(0.06, UpgradeService.BrandPerDay(shop, state), precision: 5);
        }

        // ── Daily cost ───────────────────────────────────────────────────────

        [Fact]
        public void ShopUpgradeDailyCostIgnoresGlobals()
        {
            var shop = Shop("wifi"); // 90/30 = 3
            shop.UpgradeIds.Add("lieferdienst"); // global → ignoriert hier
            Assert.Equal(3.0, UpgradeService.ShopUpgradeDailyCost(shop), precision: 5);
        }

        [Fact]
        public void GlobalUpgradeDailyCostSums()
        {
            var state = State();
            state.GlobalUpgradeIds.Add("lieferdienst");   // 500/30
            state.GlobalUpgradeIds.Add("loyalty_app");    // 350/30
            Assert.Equal((500 + 350) / 30.0, UpgradeService.GlobalUpgradeDailyCost(state), precision: 5);
        }

        // ── Delivery commission ──────────────────────────────────────────────

        [Fact]
        public void NoDeliveryNoCommission()
        {
            var shop = Shop("wifi");
            Assert.Equal(0, UpgradeService.DeliveryCommissionCost(shop, 1000, State()));
            Assert.False(UpgradeService.HasDeliveryChannel(shop, State()));
        }

        [Fact]
        public void LieferdienstChargesCommission()
        {
            var state = State();
            state.GlobalUpgradeIds.Add("lieferdienst"); // frac 0.18, rate 0.28
            var shop = Shop();
            // 1000 × 0.18 × 0.28 = 50.4
            Assert.Equal(50.4, UpgradeService.DeliveryCommissionCost(shop, 1000, state), precision: 4);
            Assert.True(UpgradeService.HasDeliveryChannel(shop, state));
        }

        [Fact]
        public void OwnDeliveryAppLowersCommissionTo8Percent()
        {
            var state = State();
            state.GlobalUpgradeIds.Add("lieferdienst");
            state.GlobalUpgradeIds.Add("eigen_lieferdienst"); // rate → 0.08
            var shop = Shop();
            // 1000 × 0.18 × 0.08 = 14.4
            Assert.Equal(14.4, UpgradeService.DeliveryCommissionCost(shop, 1000, state), precision: 4);
        }

        // ── Integration in GameEngineCore ────────────────────────────────────

        [Fact]
        public void UpgradesRaiseCustomersInShopStats()
        {
            var stateNo = State();
            var shopNo = Shop();
            shopNo.FootTraffic = 3000; shopNo.Reputation = 3.5;
            shopNo.Employees.Add(new Employee { Id = "e", TypeId = "kassierer",
                Speed = 8, Friendliness = 6, Reliability = 6, Experience = 5, SalaryPerDay = 65 });
            stateNo.Shops.Add(shopNo);

            var stateYes = stateNo.Clone();
            stateYes.Shops[0].UpgradeIds.Add("wifi");
            stateYes.Shops[0].UpgradeIds.Add("klima");

            double rno = 0, ryes = 0;
            for (var d = 1; d <= 30; d++)
            {
                rno += GameEngineCore.CalculateShopStats(stateNo.Shops[0], d, stateNo).ActualCustomers;
                ryes += GameEngineCore.CalculateShopStats(stateYes.Shops[0], d, stateYes).ActualCustomers;
            }
            Assert.True(ryes >= rno, $"no={rno} yes={ryes}");
        }

        [Fact]
        public void DeliveryCommissionShowsUpInCostBreakdown()
        {
            var state = State();
            var shop = Shop();
            shop.FootTraffic = 4000;
            shop.Employees.Add(new Employee { Id = "e", TypeId = "doener_meister",
                Speed = 7, Friendliness = 6, Reliability = 6, Experience = 7, SalaryPerDay = 80 });
            state.Shops.Add(shop);

            var brNo = GameEngineCore.CalculateDailyCostsBreakdown(shop, 1, state);
            Assert.Equal(0, brNo.DeliveryCommission);

            state.GlobalUpgradeIds.Add("lieferdienst");
            var brYes = GameEngineCore.CalculateDailyCostsBreakdown(shop, 1, state);
            Assert.True(brYes.DeliveryCommission > 0);
        }

        [Fact]
        public void ShopUpgradeDailyCostInBreakdown()
        {
            var state = State();
            var shop = Shop("wifi"); // daily 3.0
            shop.FootTraffic = 4000;
            state.Shops.Add(shop);
            var br = GameEngineCore.CalculateDailyCostsBreakdown(shop, 1, state);
            // pressure normal = 1.0 → upgrades = 3.0
            Assert.Equal(3.0, br.Upgrades, precision: 5);
        }

        [Fact]
        public void UpgradeReputationRaisesUpdate()
        {
            var state = State();
            var shop = Shop(); shop.Reputation = 3.0;
            var baseRep = GameEngineCore.UpdateReputation(shop, state);
            shop.UpgradeIds.Add("premium_reinigung"); // rep 0.015
            var withUpgrade = GameEngineCore.UpdateReputation(shop, state);
            Assert.True(withUpgrade > baseRep, $"base={baseRep} with={withUpgrade}");
        }

        [Fact]
        public void UpgradeBrandRaisesAwareness()
        {
            var state = State();
            var shops = new List<Shop> { Shop() };
            var baseBrand = DayProcessing.UpdateBrand(state, 1000, 100, shops);

            state.GlobalUpgradeIds.Add("loyalty_app"); // brand 0.05
            var withUpgrade = DayProcessing.UpdateBrand(state, 1000, 100, shops);
            Assert.True(withUpgrade.BrandAwareness > baseBrand.BrandAwareness);
        }
    }
}
