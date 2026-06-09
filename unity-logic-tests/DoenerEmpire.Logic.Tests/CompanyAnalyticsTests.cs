// Tests für CompanyAnalytics — Ranking, Marktanteil, Gesundheits-Score.
// Spiegelt lib/services/game_engine.dart (shopsByProfit, playerMarketShareIn, healthScore).

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class CompanyAnalyticsTests
    {
        private static GameState State() => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop Shop(string id, int footTraffic, double rep = 3.5, int employees = 1)
        {
            var s = new Shop
            {
                Id = id, CityId = "fulda", IsOpen = true, Reputation = rep,
                FootTraffic = footTraffic, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            for (var i = 0; i < employees; i++)
                s.Employees.Add(new Employee { Id = $"{id}_e{i}", TypeId = "kassierer",
                    Speed = 7, Friendliness = 6, Reliability = 6, Experience = 5, SalaryPerDay = 65 });
            return s;
        }

        // ── ShopsByProfit ────────────────────────────────────────────────────

        [Fact]
        public void ShopsByProfitSortedDescending()
        {
            var s = State();
            s.Shops.Add(Shop("low", footTraffic: 1500));
            s.Shops.Add(Shop("high", footTraffic: 9000));
            var ranked = CompanyAnalytics.ShopsByProfit(s);
            Assert.Equal(2, ranked.Count);
            Assert.True(ranked[0].Profit >= ranked[1].Profit);
            Assert.Equal("high", ranked[0].Shop.Id);
        }

        [Fact]
        public void ShopsByProfitEmptyWithoutShops()
        {
            Assert.Empty(CompanyAnalytics.ShopsByProfit(State()));
        }

        // ── PlayerMarketShareIn ──────────────────────────────────────────────

        [Fact]
        public void MarketShareZeroWithoutOwnShop()
        {
            var s = State();
            s.Competitors.Add(new Competitor { Id = "c", CityId = "fulda", MarketShare = 0.4 });
            Assert.Equal(0, CompanyAnalytics.PlayerMarketShareIn(s, "fulda"));
        }

        [Fact]
        public void MarketShareIsRemainderAfterCompetitors()
        {
            var s = State();
            s.Shops.Add(Shop("own", footTraffic: 4000));
            s.Competitors.Add(new Competitor { Id = "c1", CityId = "fulda", MarketShare = 0.3 });
            s.Competitors.Add(new Competitor { Id = "c2", CityId = "fulda", MarketShare = 0.2 });
            Assert.Equal(0.5, CompanyAnalytics.PlayerMarketShareIn(s, "fulda"), precision: 5);
        }

        [Fact]
        public void MarketShareClampedToZeroWhenOversaturated()
        {
            var s = State();
            s.Shops.Add(Shop("own", footTraffic: 4000));
            s.Competitors.Add(new Competitor { Id = "c1", CityId = "fulda", MarketShare = 0.8 });
            s.Competitors.Add(new Competitor { Id = "c2", CityId = "fulda", MarketShare = 0.7 });
            Assert.Equal(0.0, CompanyAnalytics.PlayerMarketShareIn(s, "fulda"));
        }

        // ── Health ───────────────────────────────────────────────────────────

        [Fact]
        public void HealthNeutralWhenNoShops()
        {
            var h = CompanyAnalytics.Health(State());
            Assert.Equal(50, h.Score);
            Assert.Equal("Neu gegründet", h.Label);
        }

        [Fact]
        public void HealthInRange0To100()
        {
            var s = State();
            s.Shops.Add(Shop("a", footTraffic: 5000, rep: 4.5, employees: 2));
            var h = CompanyAnalytics.Health(s);
            Assert.InRange(h.Score, 0.0, 100.0);
            Assert.False(string.IsNullOrEmpty(h.Label));
        }

        [Fact]
        public void HealthyCompanyScoresHigherThanStruggling()
        {
            var healthy = State();
            healthy.Cash = 50000;
            healthy.Shops.Add(Shop("a", footTraffic: 8000, rep: 4.8, employees: 2));

            var struggling = State();
            struggling.Cash = 200;
            struggling.Shops.Add(Shop("b", footTraffic: 800, rep: 1.5, employees: 0));
            struggling.Loans.Add(new Loan { Id = "l", Amount = 20000, InterestRate = 0.1,
                DurationDays = 100, DayTaken = 0 });

            Assert.True(CompanyAnalytics.Health(healthy).Score > CompanyAnalytics.Health(struggling).Score);
        }

        [Fact]
        public void DebtLowersHealthScore()
        {
            var noDebt = State();
            noDebt.Cash = 10000;
            noDebt.Shops.Add(Shop("a", footTraffic: 4000, rep: 4.0, employees: 1));

            var withDebt = State();
            withDebt.Cash = 10000;
            withDebt.Shops.Add(Shop("a", footTraffic: 4000, rep: 4.0, employees: 1));
            withDebt.Loans.Add(new Loan { Id = "l", Amount = 30000, InterestRate = 0.08,
                DurationDays = 200, DayTaken = 0 });

            Assert.True(CompanyAnalytics.Health(noDebt).Score > CompanyAnalytics.Health(withDebt).Score);
        }
    }
}
