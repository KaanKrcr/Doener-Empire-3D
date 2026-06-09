// Tests für GameEngineCore — Kern-Tagessimulation (calculateShopStats etc.)
// Spiegelt das Verhalten von lib/services/game_engine.dart.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class GameEngineCoreTests
    {
        private static Shop MakeShop(
            string id = "s1",
            string cityId = "fulda",
            int footTraffic = 4500,
            double reputation = 3.0,
            LocationPersonality personality = LocationPersonality.Touristic,
            bool addMenu = true,
            bool addEquipment = false,
            int employeeCount = 0)
        {
            var shop = new Shop
            {
                Id = id, Name = "Test", CityId = cityId,
                LocationName = "Marktplatz",
                FootTraffic = footTraffic, WeeklyRent = 1200,
                IsOpen = true, Reputation = reputation, DayOpened = 1,
                Personality = personality,
            };
            if (addMenu)
            {
                shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.50 });
                shop.Menu.Add(new ShopProduct { ProductId = "ayran", Price = 2.00 });
            }
            if (addEquipment)
            {
                shop.Equipment.Add(new ShopEquipment { EquipmentId = "spiess_standard" });
                shop.Equipment.Add(new ShopEquipment { EquipmentId = "kasse_basic" });
            }
            for (var i = 0; i < employeeCount; i++)
            {
                shop.Employees.Add(new Employee
                {
                    Id = $"e{i}", TypeId = "doener_meister", Name = $"E{i}",
                    Speed = 6, Friendliness = 6, Reliability = 6, Experience = 6,
                    SalaryPerDay = 80,
                });
            }
            return shop;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Edge cases: closed shop, empty menu
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void ClosedShopProducesZero()
        {
            var shop = MakeShop();
            shop.IsOpen = false;
            var stats = GameEngineCore.CalculateShopStats(shop, day: 1);
            Assert.Equal(0, stats.ActualRevenue);
            Assert.Equal(0, stats.ActualCustomers);
        }

        [Fact]
        public void EmptyMenuProducesZero()
        {
            var shop = MakeShop(addMenu: false);
            var stats = GameEngineCore.CalculateShopStats(shop, day: 1);
            Assert.Equal(0, stats.ActualRevenue);
        }

        [Fact]
        public void OnlyInactiveMenuProducesZero()
        {
            var shop = MakeShop();
            foreach (var p in shop.Menu) p.IsActive = false;
            var stats = GameEngineCore.CalculateShopStats(shop, day: 1);
            Assert.Equal(0, stats.ActualRevenue);
        }

        // ──────────────────────────────────────────────────────────────────────
        // ReputationFactor
        // ──────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0, 0.4)]
        [InlineData(5.0, 1.4)]
        public void ReputationFactorAtBounds(double rep, double expected)
        {
            Assert.Equal(expected, GameEngineCore.ReputationFactor(rep), precision: 5);
        }

        [Fact]
        public void ReputationFactorClamps()
        {
            Assert.Equal(0.4, GameEngineCore.ReputationFactor(-5));
            Assert.Equal(1.4, GameEngineCore.ReputationFactor(100));
        }

        // ──────────────────────────────────────────────────────────────────────
        // EquipmentQualityScore
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void NoEquipmentBaseline()
        {
            var shop = MakeShop();
            Assert.Equal(0.5, GameEngineCore.EquipmentQualityScore(shop));
        }

        [Fact]
        public void EquipmentAddsQuality()
        {
            var noEq = GameEngineCore.EquipmentQualityScore(MakeShop());
            var withEq = GameEngineCore.EquipmentQualityScore(MakeShop(addEquipment: true));
            Assert.True(withEq > noEq);
            Assert.InRange(withEq, 0.5, 2.5);
        }

        // ──────────────────────────────────────────────────────────────────────
        // StaffQualityScore
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void NoStaffMinimum()
        {
            Assert.Equal(0.55, GameEngineCore.StaffQualityScore(MakeShop()));
        }

        [Fact]
        public void MoreStaffRaisesScore()
        {
            var none = GameEngineCore.StaffQualityScore(MakeShop());
            var two = GameEngineCore.StaffQualityScore(MakeShop(employeeCount: 2));
            Assert.True(two > none);
            Assert.InRange(two, 0.55, 2.4);
        }

        // ──────────────────────────────────────────────────────────────────────
        // CapacityLimit
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void CapacityBaselineIs20()
        {
            Assert.Equal(20, GameEngineCore.CapacityLimit(MakeShop()));
        }

        [Fact]
        public void CapacityGrowsWithStaffAndEquipment()
        {
            var bare = GameEngineCore.CapacityLimit(MakeShop());
            var equipped = GameEngineCore.CapacityLimit(MakeShop(addEquipment: true, employeeCount: 2));
            Assert.True(equipped > bare * 3);
        }

        // ──────────────────────────────────────────────────────────────────────
        // DailyVariation determinism
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void DailyVariationIsDeterministicPerShopAndDay()
        {
            var shop = MakeShop();
            var v1 = GameEngineCore.DailyVariation(shop, 5);
            var v2 = GameEngineCore.DailyVariation(shop, 5);
            Assert.Equal(v1, v2);
        }

        [Fact]
        public void DailyVariationDiffersAcrossDays()
        {
            var shop = MakeShop();
            var v1 = GameEngineCore.DailyVariation(shop, 5);
            var v2 = GameEngineCore.DailyVariation(shop, 6);
            // Sehr unwahrscheinlich gleich
            Assert.NotEqual(v1, v2);
        }

        [Fact]
        public void DailyVariationStaysInBand()
        {
            var shop = MakeShop();
            for (var d = 0; d < 100; d++)
            {
                var v = GameEngineCore.DailyVariation(shop, d);
                Assert.InRange(v, 0.6, 1.4);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // CalculateShopStats — sanity
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void OpenShopProducesPositiveCustomers()
        {
            var shop = MakeShop(employeeCount: 1, addEquipment: true);
            var stats = GameEngineCore.CalculateShopStats(shop, day: 1);
            Assert.True(stats.ActualCustomers > 0);
            Assert.True(stats.ActualRevenue > 0);
            Assert.True(stats.AvgOrderValue > 0);
        }

        [Fact]
        public void ActualNeverExceedsCapacity()
        {
            // Riesige Stadt + viel Traffic, aber keine Mitarbeiter/Equipment
            var shop = MakeShop(footTraffic: 100000);
            var stats = GameEngineCore.CalculateShopStats(shop, day: 1);
            Assert.True(stats.ActualCustomers <= stats.Capacity + 1, // round-Toleranz
                $"actual={stats.ActualCustomers} cap={stats.Capacity}");
        }

        [Fact]
        public void HigherReputationGivesMoreCustomers()
        {
            // Reduziertes Setup: kein Capacity-Cap-Bias durch hohe Mitarbeiterzahl.
            // Ohne Personal greift Cap=20 sofort, also kleinerer Traffic.
            var lowRep = MakeShop(footTraffic: 1500, reputation: 1.0,
                employeeCount: 2, addEquipment: true);
            var highRep = MakeShop(footTraffic: 1500, reputation: 5.0,
                employeeCount: 2, addEquipment: true);
            double low = 0, high = 0;
            for (var d = 1; d <= 30; d++)
            {
                low += GameEngineCore.CalculateShopStats(lowRep, d).ActualCustomers;
                high += GameEngineCore.CalculateShopStats(highRep, d).ActualCustomers;
            }
            // Reputationsfaktor: low=0.6, high=1.4 → ~2.3x. Aber rep-faktor wirkt
            // nur auf base, der Rest skaliert. Brand-Multiplier 1.0 ohne State.
            // Erwarte deutlich mehr Kunden mit besserer Rep.
            Assert.True(high > low * 1.5, $"low={low} high={high}");
        }

        // ──────────────────────────────────────────────────────────────────────
        // HourlyCustomerCurve
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void HourlyCurveSumsToActualCustomers()
        {
            var shop = MakeShop(employeeCount: 2, addEquipment: true);
            var curve = GameEngineCore.HourlyCustomerCurve(shop, 1);
            var stats = GameEngineCore.CalculateShopStats(shop, 1);
            var sum = curve.Sum();
            // Skalierung ist exakt → erlauben kleine Floating-Toleranz
            Assert.Equal(stats.ActualCustomers, sum, precision: 4);
        }

        [Fact]
        public void HourlyCurveZeroWhenClosed()
        {
            var shop = MakeShop();
            shop.IsOpen = false;
            var curve = GameEngineCore.HourlyCustomerCurve(shop, 1);
            Assert.Equal(14, curve.Length);
            Assert.All(curve, v => Assert.Equal(0, v));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("fulda", 3)]      // Klein
        [InlineData("augsburg", 5)]   // Mittel
        [InlineData("frankfurt", 7)]  // Gross
        [InlineData("berlin", 10)]    // Metropole
        public void MaxEmployeesScalesWithTier(string cityId, int expected)
        {
            var shop = MakeShop(cityId: cityId);
            Assert.Equal(expected, GameEngineCore.MaxEmployeesForShop(shop));
        }

        [Fact]
        public void IsCapacityLimitedTrueWhenNoStaffWithBigTraffic()
        {
            var shop = MakeShop(footTraffic: 100000);
            Assert.True(GameEngineCore.IsCapacityLimited(shop));
            var rec = GameEngineCore.RecommendedExtraEmployees(shop);
            Assert.True(rec > 0);
        }

        [Fact]
        public void TotalCustomersTodaySumsAcrossShops()
        {
            var state = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            state.Shops.Add(MakeShop("s1", employeeCount: 1, addEquipment: true));
            state.Shops.Add(MakeShop("s2", employeeCount: 1, addEquipment: true));
            var total = GameEngineCore.TotalCustomersToday(state);
            var sum = state.Shops.Sum(s =>
                GameEngineCore.CalculateDailyCustomers(s, state.CurrentDay, state));
            Assert.Equal(sum, total);
            Assert.True(total > 0);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Global Spieß Upgrade
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void GlobalSpiessUpgradeRaisesCapacityAndQuality()
        {
            var state = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = MakeShop(employeeCount: 1);
            state.Shops.Add(shop);

            var capNo = GameEngineCore.CapacityLimit(shop, state);
            var qNo = GameEngineCore.EquipmentQualityScore(shop, state);

            state.GlobalUpgradeIds.Add(GameEngineCore.GlobalSpiessStandardId);

            var capYes = GameEngineCore.CapacityLimit(shop, state);
            var qYes = GameEngineCore.EquipmentQualityScore(shop, state);

            Assert.Equal(capNo + 100, capYes);
            Assert.True(qYes > qNo);
        }
    }
}
