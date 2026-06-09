// Tests für DayProcessing — City-Unlocks, Combo-Kosten, Brand-Update.
// Spiegelt lib/services/game_engine.dart (_checkCityUnlocks, activeComboDailyCost, _updateBrand).

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class DayProcessingTests
    {
        private static GameState State()
            => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        // ── CheckCityUnlocks ─────────────────────────────────────────────────

        [Fact]
        public void NoUnlocksBelowThreshold()
        {
            var start = new List<string> { "fulda", "bayreuth", "goettingen" };
            var result = DayProcessing.CheckCityUnlocks(start, 1000);
            Assert.Equal(start.Count, result.Count);
        }

        [Fact]
        public void UnlocksMittelstadtAt30k()
        {
            var start = new List<string> { "fulda", "bayreuth", "goettingen" };
            var result = DayProcessing.CheckCityUnlocks(start, 30000);
            // augsburg + muenster haben UnlockCost 30000
            Assert.Contains("augsburg", result);
            Assert.Contains("muenster", result);
            Assert.DoesNotContain("frankfurt", result); // 150k
        }

        [Fact]
        public void UnlocksAccumulateWithRevenue()
        {
            var start = new List<string> { "fulda", "bayreuth", "goettingen" };
            var result = DayProcessing.CheckCityUnlocks(start, 1000000);
            // Alle kostenpflichtigen Städte freigeschaltet
            var paidCities = GameData.AllCities.Where(c => c.UnlockCost > 0).Select(c => c.Id);
            foreach (var id in paidCities) Assert.Contains(id, result);
        }

        [Fact]
        public void FreeCitiesNotAddedAutomatically()
        {
            // Leere Liste + viel Umsatz → Startstädte (cost 0) werden NICHT zugefügt
            var result = DayProcessing.CheckCityUnlocks(new List<string>(), 1000000);
            Assert.DoesNotContain("fulda", result);
            Assert.DoesNotContain("bayreuth", result);
        }

        [Fact]
        public void DoesNotDuplicateExisting()
        {
            var start = new List<string> { "fulda", "augsburg" };
            var result = DayProcessing.CheckCityUnlocks(start, 30000);
            Assert.Equal(1, result.Count(c => c == "augsburg"));
        }

        // ── ActiveComboDailyCost ─────────────────────────────────────────────

        [Fact]
        public void NoCombosNoCost()
        {
            Assert.Equal(0, DayProcessing.ActiveComboDailyCost(State()));
        }

        [Fact]
        public void ComboCostSums()
        {
            var state = State();
            state.ActiveComboIds.Add("mittagsmenu");  // 45
            state.ActiveComboIds.Add("studenten_deal"); // 35
            Assert.Equal(80, DayProcessing.ActiveComboDailyCost(state));
        }

        [Fact]
        public void UnknownComboIgnored()
        {
            var state = State();
            state.ActiveComboIds.Add("does_not_exist");
            Assert.Equal(0, DayProcessing.ActiveComboDailyCost(state));
        }

        // ── UpdateBrand ──────────────────────────────────────────────────────

        [Fact]
        public void AwarenessGrowsWithActivity()
        {
            var state = State();
            state.Brand.BrandAwareness = 5.0;
            var shops = new List<Shop>
            {
                new() { Id = "s1", CityId = "fulda", Reputation = 3.0 },
            };
            var brand = DayProcessing.UpdateBrand(state, dailyRevenue: 5000, dailyCustomers: 500, shops);
            Assert.True(brand.BrandAwareness > 5.0);
        }

        [Fact]
        public void AwarenessClampedTo100()
        {
            var state = State();
            state.Brand.BrandAwareness = 99.9;
            var shops = new List<Shop> { new() { Id = "s1", CityId = "fulda", Reputation = 5.0 } };
            var brand = DayProcessing.UpdateBrand(state, 1000000, 100000, shops);
            Assert.InRange(brand.BrandAwareness, 0.0, 100.0);
        }

        [Fact]
        public void PlateauSlowsGrowthAbove30()
        {
            var state = State();
            // Bei Awareness 50 wirkt das Plateau (über 30 nur 40% des Overshoots)
            state.Brand.BrandAwareness = 50.0;
            var shops = new List<Shop> { new() { Id = "s1", CityId = "fulda", Reputation = 3.0 } };
            var brand = DayProcessing.UpdateBrand(state, 1000, 100, shops);
            // 50 → overshoot 20 → 30 + 20*0.4 = 38 (+ kleiner activity-Zuwachs)
            Assert.InRange(brand.BrandAwareness, 37.9, 39.0);
        }

        [Fact]
        public void CityReputationGrowsPerShop()
        {
            var state = State();
            var shops = new List<Shop>
            {
                new() { Id = "s1", CityId = "fulda", Reputation = 4.0 },
                new() { Id = "s2", CityId = "fulda", Reputation = 4.0 },
            };
            var brand = DayProcessing.UpdateBrand(state, 5000, 500, shops);
            Assert.True(brand.InCity("fulda") > 0);
        }

        [Fact]
        public void HigherShopReputationGrowsCityRepFaster()
        {
            var state = State();
            var low = new List<Shop> { new() { Id = "a", CityId = "fulda", Reputation = 1.0 } };
            var high = new List<Shop> { new() { Id = "a", CityId = "fulda", Reputation = 5.0 } };
            var brandLow = DayProcessing.UpdateBrand(state, 5000, 500, low);
            var brandHigh = DayProcessing.UpdateBrand(state, 5000, 500, high);
            Assert.True(brandHigh.InCity("fulda") > brandLow.InCity("fulda"));
        }

        [Fact]
        public void UpdateBrandReturnsNewInstance()
        {
            var state = State();
            var shops = new List<Shop> { new() { Id = "s1", CityId = "fulda", Reputation = 3.0 } };
            var brand = DayProcessing.UpdateBrand(state, 5000, 500, shops);
            Assert.NotSame(state.Brand, brand);
        }
    }
}
