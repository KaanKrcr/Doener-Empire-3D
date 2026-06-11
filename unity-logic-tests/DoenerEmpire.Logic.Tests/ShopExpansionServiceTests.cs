using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class ShopExpansionServiceTests
    {
        [Fact]
        public void ExpandToNextTierDeductsCashAndUpdatesTier()
        {
            GameState state = StateWithShop(10000, ShopSizeTier.Klein, "fulda");

            ShopExpansionResult result = new ShopExpansionService().ExpandToNextTier(state, "shop_1");

            Assert.True(result.Success);
            Assert.Equal(8000, result.Cost);
            Assert.Equal(ShopSizeTier.Mittel, result.NewTier);
            Assert.Equal(2000, state.Cash);
            Assert.Equal(ShopSizeTier.Mittel, state.Shops[0].SizeTier);
            Assert.Equal(5, ShopSizing.EmployeeCap(CityTier.Klein, state.Shops[0].SizeTier));
            Assert.Equal(0.73, state.Shops[0].Morale, precision: 5);
        }

        [Fact]
        public void ExpandWithInsufficientCashLeavesStateUnchanged()
        {
            GameState state = StateWithShop(7999, ShopSizeTier.Klein, "fulda");

            ShopExpansionResult result = new ShopExpansionService().ExpandToNextTier(state, "shop_1");

            Assert.False(result.Success);
            Assert.Equal(7999, state.Cash);
            Assert.Equal(ShopSizeTier.Klein, state.Shops[0].SizeTier);
            Assert.Equal(0.75, state.Shops[0].Morale);
        }

        [Fact]
        public void ExpandAtCityCapLeavesStateUnchanged()
        {
            GameState state = StateWithShop(100000, ShopSizeTier.Mittel, "fulda");

            ShopExpansionResult result = new ShopExpansionService().ExpandToNextTier(state, "shop_1");

            Assert.False(result.Success);
            Assert.Equal(100000, state.Cash);
            Assert.Equal(ShopSizeTier.Mittel, state.Shops[0].SizeTier);
            Assert.Equal(0.75, state.Shops[0].Morale);
        }

        [Fact]
        public void ExpandAtMaxTierLeavesStateUnchanged()
        {
            GameState state = StateWithShop(100000, ShopSizeTier.Flagship, "berlin");

            ShopExpansionResult result = new ShopExpansionService().ExpandToNextTier(state, "shop_1");

            Assert.False(result.Success);
            Assert.Equal(100000, state.Cash);
            Assert.Equal(ShopSizeTier.Flagship, state.Shops[0].SizeTier);
        }

        private static GameState StateWithShop(double cash, ShopSizeTier tier, string cityId)
        {
            GameState state = GameState.Initial("Test", "Kaan", cash);
            state.Cash = cash;
            state.Shops.Add(new Shop
            {
                Id = "shop_1",
                CityId = cityId,
                SizeTier = tier,
                WeeklyRent = 1400,
                Morale = 0.75,
            });
            return state;
        }
    }
}
