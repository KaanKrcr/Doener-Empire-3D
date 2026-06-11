using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class ShopCampaignServiceTests
    {
        [Fact]
        public void StartShopCampaignDeductsCatalogCostAndAddsSingleActiveCampaign()
        {
            GameState state = StateWithShop(1000);
            state.CurrentDay = 12;

            ShopCampaignResult result = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "flyer_local");

            Assert.True(result.Success);
            Assert.Equal(400, result.Cost);
            Assert.Equal("flyer_local", result.CampaignId);
            Assert.Equal(600, state.Cash);
            Assert.Single(state.Shops[0].ActiveCampaigns);
            Assert.Equal("flyer_local", state.Shops[0].ActiveCampaigns[0].CampaignId);
            Assert.Equal(12, state.Shops[0].ActiveCampaigns[0].StartDay);
            Assert.Equal(15, state.Shops[0].ActiveCampaigns[0].EndDay);
            Assert.Empty(state.ActiveCityCampaigns);
            Assert.Empty(state.ActiveGlobalCampaigns);
        }

        [Fact]
        public void StartShopCampaignWithInvalidShopLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);

            ShopCampaignResult result = new ShopCampaignService()
                .StartShopCampaign(state, "missing", "flyer_local");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Empty(state.Shops[0].ActiveCampaigns);
        }

        [Fact]
        public void StartShopCampaignWithInvalidCampaignIdLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);

            ShopCampaignResult result = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "missing");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Empty(state.Shops[0].ActiveCampaigns);
        }

        [Fact]
        public void StartShopCampaignRejectsCityOrGlobalCampaignIds()
        {
            GameState state = StateWithShop(10000);

            ShopCampaignResult cityResult = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "city_plakat");
            ShopCampaignResult globalResult = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "tv_werbung");

            Assert.False(cityResult.Success);
            Assert.False(globalResult.Success);
            Assert.Equal(10000, state.Cash);
            Assert.Empty(state.Shops[0].ActiveCampaigns);
            Assert.Empty(state.ActiveCityCampaigns);
            Assert.Empty(state.ActiveGlobalCampaigns);
        }

        [Fact]
        public void StartDuplicateActiveShopCampaignLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);
            state.CurrentDay = 3;
            state.Shops[0].ActiveCampaigns.Add(new ActiveCampaign
            {
                CampaignId = "flyer_local",
                StartDay = 1,
                EndDay = 4,
            });

            ShopCampaignResult result = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "flyer_local");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Single(state.Shops[0].ActiveCampaigns);
        }

        [Fact]
        public void StartShopCampaignWithInsufficientCashLeavesStateUnchanged()
        {
            GameState state = StateWithShop(399);

            ShopCampaignResult result = new ShopCampaignService()
                .StartShopCampaign(state, "shop_1", "flyer_local");

            Assert.False(result.Success);
            Assert.Equal(399, state.Cash);
            Assert.Empty(state.Shops[0].ActiveCampaigns);
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
