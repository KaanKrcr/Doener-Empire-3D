using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct ShopCampaignResult
    {
        public readonly bool Success;
        public readonly string ErrorMessage;
        public readonly double Cost;
        public readonly string CampaignId;

        private ShopCampaignResult(bool success, string errorMessage, double cost, string campaignId)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Cost = cost;
            CampaignId = campaignId;
        }

        public static ShopCampaignResult Started(double cost, string campaignId) =>
            new(true, null, cost, campaignId);

        public static ShopCampaignResult Failed(string message) =>
            new(false, message, 0, null);
    }

    public sealed class ShopCampaignService
    {
        public ShopCampaignResult StartShopCampaign(GameState state, string shopId, string campaignId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(shopId))
            {
                return ShopCampaignResult.Failed("Filiale nicht gefunden.");
            }

            Shop shop = state.Shops.FirstOrDefault(candidate => candidate.Id == shopId);
            if (shop == null)
            {
                return ShopCampaignResult.Failed("Filiale nicht gefunden.");
            }

            if (string.IsNullOrWhiteSpace(campaignId))
            {
                return ShopCampaignResult.Failed("Kampagne nicht gefunden.");
            }

            MarketingCampaign campaign = MarketingCatalog.ShopCampaigns.FirstOrDefault(candidate => candidate.Id == campaignId);
            if (campaign == null)
            {
                return ShopCampaignResult.Failed("Kampagne nicht gefunden.");
            }

            if (campaign.Scope != MarketingScope.Shop)
            {
                return ShopCampaignResult.Failed("Nur Filial-Kampagnen sind hier verfuegbar.");
            }

            if (shop.ActiveCampaigns.Any(active =>
                    active.CampaignId == campaign.Id && active.IsActive(state.CurrentDay)))
            {
                return ShopCampaignResult.Failed("Diese Kampagne laeuft bereits.");
            }

            if (state.Cash < campaign.Cost)
            {
                return ShopCampaignResult.Failed("Nicht genug Kapital fuer diese Kampagne.");
            }

            state.Cash = Math.Round(state.Cash - campaign.Cost, 2);
            shop.ActiveCampaigns.Add(new ActiveCampaign
            {
                CampaignId = campaign.Id,
                StartDay = state.CurrentDay,
                EndDay = state.CurrentDay + campaign.DurationDays,
            });

            return ShopCampaignResult.Started(campaign.Cost, campaign.Id);
        }
    }
}
