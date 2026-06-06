using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class ProductPricingServiceTests
    {
        [Fact]
        public void SetProductPriceChangesExistingProduct()
        {
            GameState state = StateWithShop();

            ProductPriceChangeResult result = new ProductPricingService()
                .SetProductPrice(state, "shop_1", "doener_fladen", 8.499);

            Assert.True(result.Success);
            Assert.Equal(8.5, state.Shops[0].Menu[0].Price);
            Assert.Equal(5000, state.Cash);
        }

        [Fact]
        public void SetProductPriceWithInvalidShopLeavesStateUnchanged()
        {
            GameState state = StateWithShop();

            ProductPriceChangeResult result = new ProductPricingService()
                .SetProductPrice(state, "missing", "doener_fladen", 8.5);

            Assert.False(result.Success);
            Assert.Equal(6.5, state.Shops[0].Menu[0].Price);
            Assert.Equal(5000, state.Cash);
        }

        [Fact]
        public void SetProductPriceWithInvalidProductLeavesStateUnchanged()
        {
            GameState state = StateWithShop();

            ProductPriceChangeResult result = new ProductPricingService()
                .SetProductPrice(state, "shop_1", "missing", 8.5);

            Assert.False(result.Success);
            Assert.Equal(6.5, state.Shops[0].Menu[0].Price);
            Assert.Equal(5000, state.Cash);
        }

        [Theory]
        [InlineData(0.99)]
        [InlineData(25.01)]
        public void SetProductPriceWithOutOfRangePriceLeavesStateUnchanged(double price)
        {
            GameState state = StateWithShop();

            ProductPriceChangeResult result = new ProductPricingService()
                .SetProductPrice(state, "shop_1", "doener_fladen", price);

            Assert.False(result.Success);
            Assert.Equal(6.5, state.Shops[0].Menu[0].Price);
            Assert.Equal(5000, state.Cash);
        }

        private static GameState StateWithShop()
        {
            GameState state = GameState.Initial("Test", "Kaan", 5000);
            state.Cash = 5000;
            state.Shops.Add(new Shop
            {
                Id = "shop_1",
                CityId = "fulda",
                Menu =
                {
                    new ShopProduct { ProductId = "doener_fladen", Price = 6.5, IsActive = true },
                },
            });
            return state;
        }
    }
}
