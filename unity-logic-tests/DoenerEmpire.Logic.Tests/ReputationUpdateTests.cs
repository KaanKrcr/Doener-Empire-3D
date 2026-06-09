// Tests für GameEngineCore.UpdateReputation
// Spiegelt lib/services/game_engine.dart (_updateReputation).

using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ReputationUpdateTests
    {
        private static GameState State(GameDifficulty d = GameDifficulty.Normal)
            => GameState.Initial("X", "Y", 15000, d, tutorialEnabled: false);

        private static Shop MakeShop(double price, double reputation = 3.0,
            int employeeCount = 0)
        {
            var shop = new Shop
            {
                Id = "s1", CityId = "fulda", IsOpen = true,
                Reputation = reputation, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = price });
            for (var i = 0; i < employeeCount; i++)
                shop.Employees.Add(new Employee
                {
                    Id = $"e{i}", TypeId = "kassierer", Speed = 6, Friendliness = 8,
                    Reliability = 6, Experience = 5, SalaryPerDay = 65,
                });
            return shop;
        }

        [Fact]
        public void EmptyMenuKeepsReputation()
        {
            var shop = new Shop { Id = "s", CityId = "fulda", Reputation = 3.3 };
            Assert.Equal(3.3, GameEngineCore.UpdateReputation(shop, State()));
        }

        [Fact]
        public void CheapPriceRaisesReputation()
        {
            // basePrice doener_fladen = 6.50; ratio 0.8 → +0.05
            var shop = MakeShop(price: 5.20, reputation: 3.0);
            var newRep = GameEngineCore.UpdateReputation(shop, State());
            Assert.True(newRep > 3.0, $"newRep={newRep}");
        }

        [Fact]
        public void ExpensivePriceLowersReputation()
        {
            // ratio > 1.6 → -0.25
            var shop = MakeShop(price: 12.0, reputation: 3.0);
            var newRep = GameEngineCore.UpdateReputation(shop, State());
            Assert.True(newRep < 3.0, $"newRep={newRep}");
        }

        [Fact]
        public void HardDifficultyPunishesOverpricingMore()
        {
            var normal = GameEngineCore.UpdateReputation(MakeShop(12.0), State(GameDifficulty.Normal));
            var hard = GameEngineCore.UpdateReputation(MakeShop(12.0), State(GameDifficulty.Hard));
            Assert.True(hard < normal, $"hard={hard} normal={normal}");
        }

        [Fact]
        public void FriendlyStaffHelpsReputation()
        {
            var noStaff = GameEngineCore.UpdateReputation(MakeShop(6.50, 3.0, 0), State());
            var withStaff = GameEngineCore.UpdateReputation(MakeShop(6.50, 3.0, 2), State());
            Assert.True(withStaff > noStaff, $"no={noStaff} with={withStaff}");
        }

        [Fact]
        public void ReputationClampedToValidRange()
        {
            var high = MakeShop(5.0, reputation: 5.0);
            var low = MakeShop(12.0, reputation: 0.5);
            Assert.InRange(GameEngineCore.UpdateReputation(high, State()), 0.5, 5.0);
            Assert.InRange(GameEngineCore.UpdateReputation(low, State()), 0.5, 5.0);
        }

        [Fact]
        public void PremiumQualityBoostsReputation()
        {
            var state = State();
            var shop = MakeShop(6.50, reputation: 3.0, employeeCount: 1);
            var baseRep = GameEngineCore.UpdateReputation(shop, state);

            state.ProductQuality["doener_fladen"] = "premium";
            var premiumRep = GameEngineCore.UpdateReputation(shop, state);
            Assert.True(premiumRep > baseRep, $"base={baseRep} premium={premiumRep}");
        }

        [Fact]
        public void ActiveComboBoostsReputation()
        {
            var state = State();
            // Shop führt doener_duerum + ayran → studenten_deal greift
            var shop = new Shop
            {
                Id = "s1", CityId = "fulda", IsOpen = true, Reputation = 3.0,
                Personality = LocationPersonality.University,
            };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_duerum", Price = 6.0 });
            shop.Menu.Add(new ShopProduct { ProductId = "ayran", Price = 2.0 });

            var noCombo = GameEngineCore.UpdateReputation(shop, state);
            state.ActiveComboIds.Add("studenten_deal");
            var withCombo = GameEngineCore.UpdateReputation(shop, state);
            Assert.True(withCombo > noCombo, $"no={noCombo} with={withCombo}");
        }
    }
}
