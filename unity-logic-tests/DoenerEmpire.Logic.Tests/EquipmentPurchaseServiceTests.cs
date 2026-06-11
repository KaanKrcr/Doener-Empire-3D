using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class EquipmentPurchaseServiceTests
    {
        [Fact]
        public void BuyEquipmentDeductsCashAndAddsSingleEquipment()
        {
            GameState state = StateWithShop(5000);

            EquipmentPurchaseResult result = new EquipmentPurchaseService()
                .BuyEquipment(state, "shop_1", "fritteuse_standard");

            Assert.True(result.Success);
            Assert.Equal(700, result.Cost);
            Assert.Equal("fritteuse_standard", result.EquipmentId);
            Assert.Equal(4300, state.Cash);
            Assert.Single(state.Shops[0].Equipment);
            Assert.Equal("fritteuse_standard", state.Shops[0].Equipment[0].EquipmentId);
            Assert.Empty(state.Shops[0].Menu);
        }

        [Fact]
        public void BuyEquipmentWithInvalidShopLeavesStateUnchanged()
        {
            GameState state = StateWithShop(5000);

            EquipmentPurchaseResult result = new EquipmentPurchaseService()
                .BuyEquipment(state, "missing", "fritteuse_standard");

            Assert.False(result.Success);
            Assert.Equal(5000, state.Cash);
            Assert.Empty(state.Shops[0].Equipment);
        }

        [Fact]
        public void BuyEquipmentWithInvalidEquipmentLeavesStateUnchanged()
        {
            GameState state = StateWithShop(5000);

            EquipmentPurchaseResult result = new EquipmentPurchaseService()
                .BuyEquipment(state, "shop_1", "missing");

            Assert.False(result.Success);
            Assert.Equal(5000, state.Cash);
            Assert.Empty(state.Shops[0].Equipment);
        }

        [Fact]
        public void BuyDuplicateEquipmentLeavesStateUnchanged()
        {
            GameState state = StateWithShop(5000);
            state.Shops[0].Equipment.Add(new ShopEquipment { EquipmentId = "fritteuse_standard" });

            EquipmentPurchaseResult result = new EquipmentPurchaseService()
                .BuyEquipment(state, "shop_1", "fritteuse_standard");

            Assert.False(result.Success);
            Assert.Equal(5000, state.Cash);
            Assert.Single(state.Shops[0].Equipment);
        }

        [Fact]
        public void BuyEquipmentWithInsufficientCashLeavesStateUnchanged()
        {
            GameState state = StateWithShop(699);

            EquipmentPurchaseResult result = new EquipmentPurchaseService()
                .BuyEquipment(state, "shop_1", "fritteuse_standard");

            Assert.False(result.Success);
            Assert.Equal(699, state.Cash);
            Assert.Empty(state.Shops[0].Equipment);
        }

        private static GameState StateWithShop(double cash)
        {
            GameState state = GameState.Initial("Test", "Kaan", cash);
            state.Cash = cash;
            state.Shops.Add(new Shop
            {
                Id = "shop_1",
                CityId = "fulda",
            });
            return state;
        }
    }
}
