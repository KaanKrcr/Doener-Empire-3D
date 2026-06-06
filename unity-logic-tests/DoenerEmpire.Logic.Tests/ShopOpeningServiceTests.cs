using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class ShopOpeningServiceTests
    {
        [Fact]
        public void OpenShopSubtractsDepositAndInitialRentAndCreatesDefaultShop()
        {
            var state = GameState.Initial("Doener Empire", "Kaan", 10000);

            ShopOpeningResult result = new ShopOpeningService().OpenShop(state, Request());

            Assert.True(result.Success);
            Assert.Equal(5240, state.Cash);
            Shop shop = Assert.Single(state.Shops);
            Assert.Same(shop, result.Shop);
            Assert.Equal("available_marktplatz", shop.Id);
            Assert.Equal("Doener Empire", shop.Name);
            Assert.Equal("Marktplatz", shop.CustomName);
            Assert.Equal("fulda", shop.CityId);
            Assert.Equal(5400, shop.FootTraffic);
            Assert.Equal(1560, shop.WeeklyRent);
            Assert.Equal(ShopSizeTier.Klein, shop.SizeTier);
            Assert.Equal(state.CurrentDay, shop.DayOpened);
            Assert.Equal(GameData.AllProducts.Count(product => product.IsDefault), shop.Menu.Count);
            Assert.Contains(shop.Menu, product => product.ProductId == "doener_fladen" && product.Price == 6.5 && product.IsActive);
            Assert.Contains(shop.Equipment, equipment => equipment.EquipmentId == "spiess_klein");
            Assert.Contains(shop.Equipment, equipment => equipment.EquipmentId == "kasse_basic");
        }

        [Fact]
        public void OpenShopWithInsufficientCashLeavesStateUnchanged()
        {
            var state = GameState.Initial("Doener Empire", "Kaan", 4759);

            ShopOpeningResult result = new ShopOpeningService().OpenShop(state, Request());

            Assert.False(result.Success);
            Assert.Equal("Nicht genug Kapital fuer Kaution und erste Miete.", result.ErrorMessage);
            Assert.Equal(4759, state.Cash);
            Assert.Empty(state.Shops);
        }

        [Fact]
        public void OpenShopDoesNotDuplicateOwnedLocation()
        {
            var state = GameState.Initial("Doener Empire", "Kaan", 10000);
            state.Shops.Add(new Shop { Id = "available_marktplatz", CityId = "fulda" });

            ShopOpeningResult result = new ShopOpeningService().OpenShop(state, Request());

            Assert.False(result.Success);
            Assert.Equal(10000, state.Cash);
            Assert.Single(state.Shops);
        }

        private static ShopOpeningRequest Request()
        {
            return new ShopOpeningRequest(
                "available_marktplatz",
                "Doener Empire",
                "fulda",
                "Marktplatz",
                5400,
                1560,
                3200,
                LocationPersonality.Touristic);
        }
    }
}
