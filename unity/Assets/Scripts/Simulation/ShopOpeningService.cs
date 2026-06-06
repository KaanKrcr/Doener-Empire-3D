using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct ShopOpeningRequest
    {
        public readonly string HotspotId;
        public readonly string CompanyName;
        public readonly string CityId;
        public readonly string LocationName;
        public readonly int FootTraffic;
        public readonly double WeeklyRent;
        public readonly double Deposit;
        public readonly LocationPersonality Personality;

        public ShopOpeningRequest(
            string hotspotId,
            string companyName,
            string cityId,
            string locationName,
            int footTraffic,
            double weeklyRent,
            double deposit,
            LocationPersonality personality)
        {
            HotspotId = hotspotId;
            CompanyName = companyName;
            CityId = cityId;
            LocationName = locationName;
            FootTraffic = footTraffic;
            WeeklyRent = weeklyRent;
            Deposit = deposit;
            Personality = personality;
        }
    }

    public readonly struct ShopOpeningResult
    {
        public readonly bool Success;
        public readonly Shop Shop;
        public readonly double Cost;
        public readonly string ErrorMessage;

        private ShopOpeningResult(bool success, Shop shop, double cost, string errorMessage)
        {
            Success = success;
            Shop = shop;
            Cost = cost;
            ErrorMessage = errorMessage;
        }

        public static ShopOpeningResult Opened(Shop shop, double cost) => new(true, shop, cost, null);

        public static ShopOpeningResult Failed(string message) => new(false, null, 0, message);
    }

    public sealed class ShopOpeningService
    {
        public ShopOpeningResult OpenShop(GameState state, ShopOpeningRequest request)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(request.HotspotId) || string.IsNullOrWhiteSpace(request.CityId))
            {
                return ShopOpeningResult.Failed("Standortdaten sind unvollstaendig.");
            }

            if (state.Shops.Any(shop => shop.Id == request.HotspotId))
            {
                return ShopOpeningResult.Failed("Dieser Standort gehoert dir bereits.");
            }

            double cost = request.Deposit + request.WeeklyRent;
            if (state.Cash < cost)
            {
                return ShopOpeningResult.Failed("Nicht genug Kapital fuer Kaution und erste Miete.");
            }

            Shop shop = CreateDefaultShop(state, request);
            state.Cash -= cost;
            state.Shops.Add(shop);
            return ShopOpeningResult.Opened(shop, cost);
        }

        private static Shop CreateDefaultShop(GameState state, ShopOpeningRequest request)
        {
            return new Shop
            {
                Id = request.HotspotId,
                Name = string.IsNullOrWhiteSpace(request.CompanyName) ? state.CompanyName : request.CompanyName,
                CustomName = request.LocationName,
                CityId = request.CityId,
                LocationName = request.LocationName,
                FootTraffic = request.FootTraffic,
                WeeklyRent = request.WeeklyRent,
                Reputation = 3.2,
                DayOpened = state.CurrentDay,
                Personality = request.Personality,
                SizeTier = ShopSizeTier.Klein,
                Morale = 0.75,
                Regulars = 0.0,
                Menu = GameData.AllProducts
                    .Where(product => product.IsDefault)
                    .Select(product => new ShopProduct
                    {
                        ProductId = product.Id,
                        Price = product.BasePrice,
                        IsActive = true,
                    })
                    .ToList(),
                Equipment =
                {
                    new ShopEquipment { EquipmentId = "spiess_klein" },
                    new ShopEquipment { EquipmentId = "kasse_basic" },
                },
            };
        }
    }
}
