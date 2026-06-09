// Tests für ComboService + ComboData + IngredientQuality
// Spiegeln lib/services/game_engine.dart (Combo-Logik) + quality_model.dart.

using System.Collections.Generic;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ComboServiceTests
    {
        private static Shop ShopWithProducts(params string[] productIds)
        {
            var shop = new Shop { Id = "s1", CityId = "fulda", IsOpen = true };
            foreach (var id in productIds)
                shop.Menu.Add(new ShopProduct { ProductId = id, Price = 6.0, IsActive = true });
            return shop;
        }

        private static GameState StateWithCombos(params string[] comboIds)
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.ActiveComboIds.AddRange(comboIds);
            return s;
        }

        [Fact]
        public void ShopSupportsComboRequiresAllProducts()
        {
            var combo = ComboData.ById("mittagsmenu"); // doener_fladen + pommes + cola
            var partial = ShopWithProducts("doener_fladen", "cola");
            var full = ShopWithProducts("doener_fladen", "pommes", "cola");
            Assert.False(ComboService.ShopSupportsCombo(partial, combo));
            Assert.True(ComboService.ShopSupportsCombo(full, combo));
        }

        [Fact]
        public void InactiveProductDoesNotCountForCombo()
        {
            var combo = ComboData.ById("studenten_deal"); // doener_duerum + ayran
            var shop = ShopWithProducts("doener_duerum", "ayran");
            shop.Menu[1].IsActive = false;
            Assert.False(ComboService.ShopSupportsCombo(shop, combo));
        }

        [Fact]
        public void EffectiveCombosOnlyCountsActiveAndSupported()
        {
            var shop = ShopWithProducts("doener_duerum", "ayran");
            // studenten_deal wird unterstützt, mittagsmenu nicht
            var state = StateWithCombos("studenten_deal", "mittagsmenu");
            var boost = ComboService.CustomerBoost(shop, state);
            Assert.Equal(0.08, boost, precision: 5); // nur studenten_deal
        }

        [Fact]
        public void NoStateMeansNoBoost()
        {
            var shop = ShopWithProducts("doener_duerum", "ayran");
            Assert.Equal(0.0, ComboService.CustomerBoost(shop, null));
            Assert.Equal(0.0, ComboService.AvgOrderBoost(shop, null));
            Assert.Equal(0.0, ComboService.ReputationPerDay(shop, null));
        }

        [Fact]
        public void MultipleCombosStack()
        {
            // Shop führt alle Produkte für mittagsmenu UND doppel_doener
            var shop = ShopWithProducts("doener_fladen", "doener_duerum", "pommes", "cola");
            var state = StateWithCombos("mittagsmenu", "doppel_doener");
            // mittagsmenu 0.06 + doppel_doener 0.07
            Assert.Equal(0.13, ComboService.CustomerBoost(shop, state), precision: 5);
            // mittagsmenu 0.10 + doppel_doener 0.12
            Assert.Equal(0.22, ComboService.AvgOrderBoost(shop, state), precision: 5);
        }

        [Fact]
        public void ComboBoostShowsUpInShopStats()
        {
            var shopNoCombo = ShopWithProducts("doener_fladen", "pommes", "cola");
            shopNoCombo.FootTraffic = 3000; shopNoCombo.Reputation = 3.5;
            shopNoCombo.Employees.Add(new Employee
            {
                Id = "e", TypeId = "kassierer", Speed = 8, Friendliness = 6,
                Reliability = 6, Experience = 5, SalaryPerDay = 65,
            });

            var stateNo = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            stateNo.Shops.Add(shopNoCombo);

            var stateYes = stateNo.Clone();
            stateYes.ActiveComboIds.Add("mittagsmenu");

            // averaging über Tage gegen DailyVariation
            double rno = 0, ryes = 0;
            for (var d = 1; d <= 30; d++)
            {
                rno += GameEngineCore.CalculateShopStats(stateNo.Shops[0], d, stateNo).ActualCustomers;
                ryes += GameEngineCore.CalculateShopStats(stateYes.Shops[0], d, stateYes).ActualCustomers;
            }
            Assert.True(ryes >= rno, $"no={rno} yes={ryes}");
        }

        [Fact]
        public void ComboByIdUnknownReturnsNull()
        {
            Assert.Null(ComboData.ById("nonexistent"));
        }

        [Fact]
        public void AllCombosHaveValidShape()
        {
            foreach (var c in ComboData.All)
            {
                Assert.False(string.IsNullOrEmpty(c.Id));
                Assert.False(string.IsNullOrEmpty(c.Name));
                Assert.NotEmpty(c.ProductIds);
                Assert.True(c.DailyCost > 0);
            }
        }

        // ── IngredientQuality ────────────────────────────────────────────────

        [Theory]
        [InlineData(IngredientQuality.Budget, 0.78)]
        [InlineData(IngredientQuality.Standard, 1.0)]
        [InlineData(IngredientQuality.Premium, 1.35)]
        public void IngredientMultMatchesDart(IngredientQuality q, double expected)
        {
            Assert.Equal(expected, IngredientQualityInfo.IngredientMult(q));
        }

        [Fact]
        public void PremiumHelpsReputationBudgetHurts()
        {
            Assert.True(IngredientQualityInfo.ReputationPerDay(IngredientQuality.Premium) > 0);
            Assert.True(IngredientQualityInfo.ReputationPerDay(IngredientQuality.Budget) < 0);
            Assert.Equal(0.0, IngredientQualityInfo.ReputationPerDay(IngredientQuality.Standard));
        }

        [Fact]
        public void QualityDartRoundTrip()
        {
            foreach (IngredientQuality q in System.Enum.GetValues(typeof(IngredientQuality)))
                Assert.Equal(q, IngredientQualityInfo.FromDart(IngredientQualityInfo.ToDart(q)));
            Assert.Equal(IngredientQuality.Standard, IngredientQualityInfo.FromDart("unknown"));
        }
    }
}
