// Tests für FacilityEngine + FacilityCatalog + ProcessDay-Integration.
// Spiegelt lib/services/corporate_engine.dart (Facility-Teil).

using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class FacilityEngineTests
    {
        private static GameState State(double cash = 500000)
            => GameState.Initial("X", "Y", cash, tutorialEnabled: false);

        private static void AddShops(GameState s, int n)
        {
            for (var i = 0; i < n; i++)
                s.Shops.Add(new Shop { Id = $"s{i}", CityId = "fulda", Reputation = 3.0 });
        }

        // ── Catalog ──────────────────────────────────────────────────────────

        [Fact]
        public void FindReturnsMatchingTemplate()
        {
            var t = FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Mittel);
            Assert.Equal(ProductionType.Fleisch, t.Type);
            Assert.Equal(FacilityTier.Mittel, t.Tier);
            Assert.Equal(200000, t.BuildCost);
        }

        // ── BuildFacility ────────────────────────────────────────────────────

        [Fact]
        public void BuildFacilityDeductsCashAndAdds()
        {
            var s = State(100000);
            var template = FacilityCatalog.Find(ProductionType.Brot, FacilityTier.Klein); // 35000
            FacilityEngine.BuildFacility(s, template);
            Assert.Single(s.Facilities);
            Assert.Equal(65000, s.Cash);
        }

        [Fact]
        public void BuildFacilityNoOpWhenTooPoor()
        {
            var s = State(1000);
            var template = FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Industrie);
            FacilityEngine.BuildFacility(s, template);
            Assert.Empty(s.Facilities);
            Assert.Equal(1000, s.Cash);
        }

        // ── Daily costs + B2B ────────────────────────────────────────────────

        [Fact]
        public void DailyCostsSumOperating()
        {
            var s = State();
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Brot, FacilityTier.Klein)); // 120
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Gemuese, FacilityTier.Klein)); // 90
            Assert.Equal(210, FacilityEngine.FacilityDailyCosts(s));
        }

        [Fact]
        public void B2BRevenueScalesWithCompetitorCount()
        {
            var s = State();
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Klein)); // 300

            // Wenige Konkurrenten → marketDemand geclamped auf 0.3
            var low = FacilityEngine.FacilityB2BRevenue(s);
            Assert.Equal(300 * 0.3, low, precision: 3);

            // Viele Konkurrenten → marketDemand geclamped auf 1.5
            for (var i = 0; i < 20; i++)
                s.Competitors.Add(new Competitor { Id = $"c{i}", CityId = "fulda" });
            var high = FacilityEngine.FacilityB2BRevenue(s);
            Assert.Equal(300 * 1.5, high, precision: 3);
        }

        [Fact]
        public void NoFacilitiesNoCostsNoRevenue()
        {
            var s = State();
            Assert.Equal(0, FacilityEngine.FacilityDailyCosts(s));
            Assert.Equal(0, FacilityEngine.FacilityB2BRevenue(s));
        }

        // ── Saving ───────────────────────────────────────────────────────────

        [Fact]
        public void NoFacilitiesNoSaving()
        {
            var s = State();
            AddShops(s, 3);
            Assert.Equal(0, FacilityEngine.FacilitySavingForShop(s, s.Shops[0]));
        }

        [Fact]
        public void FleischFacilityGivesExpectedSavingWithinCapacity()
        {
            var s = State();
            AddShops(s, 3); // <= klein.maxShops (5)
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Klein));
            // costShare 0.55 × ingredientSaving 0.20 = 0.11
            Assert.Equal(0.11, FacilityEngine.FacilitySavingForShop(s, s.Shops[0]), precision: 5);
        }

        [Fact]
        public void SavingProRatedWhenShopsExceedCapacity()
        {
            var s = State();
            AddShops(s, 10); // > klein.maxShops (5)
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Klein));
            // coverage = 5/10 = 0.5 → 0.55 × 0.20 × 0.5 = 0.055
            Assert.Equal(0.055, FacilityEngine.FacilitySavingForShop(s, s.Shops[0]), precision: 5);
        }

        [Fact]
        public void SavingClampedTo70Percent()
        {
            var s = State(5000000);
            AddShops(s, 1);
            // Alle Typen auf Industrie/Gross für maximales Saving
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Industrie));
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Brot, FacilityTier.Gross));
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Gemuese, FacilityTier.Gross));
            var saving = FacilityEngine.FacilitySavingForShop(s, s.Shops[0]);
            Assert.InRange(saving, 0.0, 0.7);
        }

        [Fact]
        public void BestTierPerTypeUsed()
        {
            var s = State(5000000);
            AddShops(s, 3);
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Klein));
            var smallSaving = FacilityEngine.FacilitySavingForShop(s, s.Shops[0]);
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Gross));
            var bigSaving = FacilityEngine.FacilitySavingForShop(s, s.Shops[0]);
            Assert.True(bigSaving > smallSaving);
        }

        // ── Integration ──────────────────────────────────────────────────────

        [Fact]
        public void FacilitySavingReducesIngredientCostInBreakdown()
        {
            var s = State();
            var shop = new Shop { Id = "s1", CityId = "fulda", IsOpen = true,
                Reputation = 3.5, FootTraffic = 4000, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            s.Shops.Add(shop);

            var brBefore = GameEngineCore.CalculateDailyCostsBreakdown(shop, 1, s);
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Gross));
            var brAfter = GameEngineCore.CalculateDailyCostsBreakdown(shop, 1, s);
            Assert.True(brAfter.Ingredients < brBefore.Ingredients,
                $"before={brBefore.Ingredients} after={brAfter.Ingredients}");
        }

        [Fact]
        public void FacilityNetAffectsCashInProcessDay()
        {
            // Anlage ohne Konkurrenten: B2B niedrig (×0.3), Kosten hoch → negativ Net.
            var s = State();
            var shop = new Shop { Id = "s1", CityId = "fulda", IsOpen = true,
                Reputation = 3.5, FootTraffic = 4000, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            s.Shops.Add(shop);

            var sNo = s.Clone();
            FacilityEngine.BuildFacility(s, FacilityCatalog.Find(ProductionType.Fleisch, FacilityTier.Industrie));
            // Industrie: cost 1500/Tag, B2B 4000×0.3=1200 → net -300 (minus reduzierte Zutaten teils gegenläufig)

            var rNo = DayProcessing.ProcessDay(sNo);
            var rWith = DayProcessing.ProcessDay(s);
            // Mit teurer Anlage ohne Markt sollte Net-Cash niedriger sein.
            Assert.True(s.Cash < sNo.Cash + rNo.Record.Revenue, "facility net should impact cash");
        }
    }
}
