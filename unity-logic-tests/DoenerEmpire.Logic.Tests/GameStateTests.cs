using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class GameStateTests
    {
        [Fact]
        public void InitialDefaults()
        {
            var g = GameState.Initial("Sultan Döner GmbH", "Mustafa", 15000);
            Assert.Equal(15000, g.Cash);
            Assert.Equal(1, g.CurrentDay);
            Assert.Equal(0, g.CurrentHour);
            Assert.Equal(GameDifficulty.Normal, g.Difficulty);
            Assert.True(g.TutorialEnabled);
            Assert.Equal("klassik", g.ActiveThemeId);
            Assert.Equal(5.0, g.Brand.BrandAwareness);
            Assert.Equal(0, g.ShopCount);
        }

        [Fact]
        public void ThreeFreeStartingCitiesUnlocked()
        {
            var g = GameState.Initial("X", "Y", 15000);
            Assert.Equal(3, g.UnlockedCityIds.Count);
            Assert.Contains("fulda", g.UnlockedCityIds);
            Assert.Contains("bayreuth", g.UnlockedCityIds);
            Assert.Contains("goettingen", g.UnlockedCityIds);
        }

        private static Shop ShopWith(string city, int employees)
        {
            var s = new Shop { Id = "s_" + city, CityId = city, Name = "Kette" };
            for (var i = 0; i < employees; i++)
                s.Employees.Add(new Employee { Id = $"{city}_e{i}", Name = "E" });
            return s;
        }

        [Fact]
        public void ShopAndEmployeeCounts()
        {
            var g = GameState.Initial("X", "Y", 15000);
            g.Shops.Add(ShopWith("fulda", 2));
            g.Shops.Add(ShopWith("bayreuth", 3));
            Assert.Equal(2, g.ShopCount);
            Assert.Equal(5, g.EmployeeCount);
        }

        [Fact]
        public void CompetitorsInAndHasShopIn()
        {
            var g = GameState.Initial("X", "Y", 15000);
            g.Shops.Add(ShopWith("fulda", 1));
            g.Competitors.Add(new Competitor { Id = "c1", CityId = "fulda", Name = "King Döner" });
            g.Competitors.Add(new Competitor { Id = "c2", CityId = "berlin", Name = "Best Döner" });

            Assert.Single(g.CompetitorsIn("fulda"));
            Assert.Empty(g.CompetitorsIn("koeln"));
            Assert.True(g.HasShopIn("fulda"));
            Assert.False(g.HasShopIn("berlin"));
        }

        [Fact]
        public void ActiveLoansTotalSumsUnpaidDebt()
        {
            var g = GameState.Initial("X", "Y", 15000);
            var l1 = new Loan { Id = "l1", Amount = 10000, InterestRate = 0.05, DurationDays = 100, DayTaken = 1 };
            var l2 = new Loan { Id = "l2", Amount = 5000, InterestRate = 0.05, DurationDays = 100, DayTaken = 1 };
            l2.AmountPaid = l2.TotalRepayment; // abbezahlt → zählt nicht
            g.Loans.Add(l1);
            g.Loans.Add(l2);
            Assert.Equal(l1.RemainingDebt, g.ActiveLoansTotal, 2);
        }

        [Fact]
        public void ModifiersReflectDifficulty()
        {
            var g = GameState.Initial("X", "Y", 15000, GameDifficulty.Impossible);
            Assert.Equal(1.90, g.Modifiers.CompetitorAggressivenessMultiplier);
        }

        [Fact]
        public void CloneIsDeepAndIndependent()
        {
            var g = GameState.Initial("X", "Y", 15000);
            g.Shops.Add(ShopWith("fulda", 1));
            g.Shops[0].Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });

            var c = g.Clone();
            c.Cash = 999;
            c.Shops[0].Menu[0].Price = 9.0;
            c.UnlockedCityIds.Add("berlin");

            Assert.Equal(15000, g.Cash);                 // Original unverändert
            Assert.Equal(6.5, g.Shops[0].Menu[0].Price);
            Assert.Equal(3, g.UnlockedCityIds.Count);
            Assert.NotSame(g.Shops, c.Shops);
            Assert.NotSame(g.Brand, c.Brand);
        }
    }
}
