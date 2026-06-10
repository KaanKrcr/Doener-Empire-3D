// Tests für GameEngineCore.CalculateHourlyRevenue/Costs (Live-Tick).
// Spiegelt lib/services/game_engine.dart (calculateHourlyRevenue/Costs).

using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class HourlyTickTests
    {
        private static GameState State() => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop Shop(string id = "s1")
        {
            var s = new Shop
            {
                Id = id, CityId = "fulda", IsOpen = true, Reputation = 3.5,
                FootTraffic = 5000, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            s.Employees.Add(new Employee { Id = id + "_e", TypeId = "doener_meister",
                Speed = 7, Friendliness = 7, Reliability = 7, Experience = 7, SalaryPerDay = 80 });
            return s;
        }

        [Fact]
        public void HourlyRevenueIsDailyDividedByOpenHours()
        {
            var s = State();
            s.Shops.Add(Shop());
            var daily = GameEngineCore.CalculateDailyRevenue(s.Shops[0], s.CurrentDay, s);
            var hourly = GameEngineCore.CalculateHourlyRevenue(s);
            Assert.Equal(daily / GameData.DailyOpenHours, hourly, precision: 5);
        }

        [Fact]
        public void HourlyRevenueSumsAcrossShops()
        {
            var one = State(); one.Shops.Add(Shop("a"));
            var two = State(); two.Shops.Add(Shop("a")); two.Shops.Add(Shop("b"));
            Assert.True(GameEngineCore.CalculateHourlyRevenue(two)
                      > GameEngineCore.CalculateHourlyRevenue(one));
        }

        [Fact]
        public void HourlyCostsIncludeShopAndGlobalAndCombo()
        {
            var s = State();
            s.Shops.Add(Shop());
            var baseHourly = GameEngineCore.CalculateHourlyCosts(s);

            // Globales Upgrade + Combo erhöhen die stündlichen Kosten.
            s.GlobalUpgradeIds.Add("lieferdienst"); // 500/Monat
            s.ActiveComboIds.Add("mittagsmenu");    // 45/Tag
            var withExtras = GameEngineCore.CalculateHourlyCosts(s);
            Assert.True(withExtras > baseHourly, $"base={baseHourly} extras={withExtras}");
        }

        [Fact]
        public void HourlyCostsArePositiveWithOpenShop()
        {
            var s = State();
            s.Shops.Add(Shop());
            Assert.True(GameEngineCore.CalculateHourlyCosts(s) > 0);
        }

        [Fact]
        public void NoShopsNoHourlyRevenueOrShopCosts()
        {
            var s = State();
            Assert.Equal(0, GameEngineCore.CalculateHourlyRevenue(s));
            // Ohne Shops + ohne globale Upgrades/Combos: 0 Kosten
            Assert.Equal(0, GameEngineCore.CalculateHourlyCosts(s));
        }
    }
}
