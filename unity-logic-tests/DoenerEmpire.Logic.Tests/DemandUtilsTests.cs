// Tests für DemandUtils — spiegeln Season / DailySpecial / Preis-Nachfrage
// aus lib/services/game_engine.dart.

using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class DemandUtilsTests
    {
        // ── Season ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(1, Season.Fruehling)]
        [InlineData(30, Season.Fruehling)]
        [InlineData(31, Season.Sommer)]
        [InlineData(60, Season.Sommer)]
        [InlineData(61, Season.Herbst)]
        [InlineData(91, Season.Winter)]
        [InlineData(121, Season.Fruehling)]   // Rotation nach 4*30 = 120
        public void SeasonRotatesEvery30Days(int day, Season expected)
        {
            Assert.Equal(expected, DemandUtils.SeasonForDay(day));
        }

        [Fact]
        public void SeasonLabelAndEmojiNotEmpty()
        {
            foreach (Season s in System.Enum.GetValues(typeof(Season)))
            {
                Assert.False(string.IsNullOrWhiteSpace(SeasonInfo.Label(s)));
                Assert.False(string.IsNullOrWhiteSpace(SeasonInfo.Emoji(s)));
            }
        }

        // ── SeasonCategoryMultiplier ─────────────────────────────────────────

        [Fact]
        public void SommerBoostsDrinksAndDampensDoener()
        {
            Assert.Equal(1.25, DemandUtils.SeasonCategoryMultiplier(
                Season.Sommer, ProductCategory.Getraenk));
            Assert.Equal(0.95, DemandUtils.SeasonCategoryMultiplier(
                Season.Sommer, ProductCategory.Doener));
            Assert.Equal(1.0, DemandUtils.SeasonCategoryMultiplier(
                Season.Sommer, ProductCategory.Beilage));
        }

        [Fact]
        public void WinterBoostsDoenerAndBoxesDampensDrinks()
        {
            Assert.Equal(1.12, DemandUtils.SeasonCategoryMultiplier(
                Season.Winter, ProductCategory.Doener));
            Assert.Equal(1.12, DemandUtils.SeasonCategoryMultiplier(
                Season.Winter, ProductCategory.Box));
            Assert.Equal(0.85, DemandUtils.SeasonCategoryMultiplier(
                Season.Winter, ProductCategory.Getraenk));
        }

        [Fact]
        public void FruehlingBoostsSides()
        {
            Assert.Equal(1.08, DemandUtils.SeasonCategoryMultiplier(
                Season.Fruehling, ProductCategory.Beilage));
            Assert.Equal(1.0, DemandUtils.SeasonCategoryMultiplier(
                Season.Fruehling, ProductCategory.Doener));
        }

        [Fact]
        public void HerbstBoostsBoxesAndDrinks()
        {
            Assert.Equal(1.08, DemandUtils.SeasonCategoryMultiplier(
                Season.Herbst, ProductCategory.Box));
            Assert.Equal(1.05, DemandUtils.SeasonCategoryMultiplier(
                Season.Herbst, ProductCategory.Getraenk));
        }

        // ── DailySpecial ─────────────────────────────────────────────────────

        [Fact]
        public void DailySpecialRotatesThroughCatalog()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            for (var d = 0; d < GameData.AllProducts.Count; d++)
                seen.Add(DemandUtils.DailySpecialProductId(d));
            Assert.Equal(GameData.AllProducts.Count, seen.Count);
        }

        [Fact]
        public void DailySpecialIsDeterministic()
        {
            Assert.Equal(
                DemandUtils.DailySpecialProductId(42),
                DemandUtils.DailySpecialProductId(42));
        }

        [Fact]
        public void DailySpecialBoostMatchesDart()
        {
            Assert.Equal(1.6, DemandUtils.DailySpecialBoost);
        }

        // ── PriceDemandFactor ───────────────────────────────────────────────

        [Fact]
        public void PriceZeroOrNegativeMeansNoDemand()
        {
            Assert.Equal(0.0, DemandUtils.PriceDemandFactor(0, 6.50));
            Assert.Equal(0.0, DemandUtils.PriceDemandFactor(-1, 6.50));
        }

        [Fact]
        public void PriceAtBaseReturnsAround1()
        {
            var f = DemandUtils.PriceDemandFactor(6.50, 6.50);
            Assert.InRange(f, 0.95, 1.05);
        }

        [Fact]
        public void DiscountIncreasesDemand()
        {
            var atBase = DemandUtils.PriceDemandFactor(6.50, 6.50);
            var discounted = DemandUtils.PriceDemandFactor(5.50, 6.50);
            Assert.True(discounted > atBase);
            Assert.InRange(discounted, 0.6, 1.25);
        }

        [Fact]
        public void OverpricingDropsDemand()
        {
            var atBase = DemandUtils.PriceDemandFactor(6.50, 6.50);
            var expensive = DemandUtils.PriceDemandFactor(9.00, 6.50);
            Assert.True(expensive < atBase);
            Assert.InRange(expensive, 0.0, 1.0);
        }

        [Fact]
        public void HardIsMorePriceSensitiveThanEasy()
        {
            // Bei Aufpreis: härtere Difficulty drückt Nachfrage stärker
            var easy = DemandUtils.PriceDemandFactor(9.0, 6.50, GameDifficulty.Easy);
            var hard = DemandUtils.PriceDemandFactor(9.0, 6.50, GameDifficulty.Hard);
            Assert.True(easy > hard, $"easy={easy} hard={hard}");
        }
    }
}
