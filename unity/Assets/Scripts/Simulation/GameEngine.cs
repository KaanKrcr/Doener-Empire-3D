using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class GameEngine
    {
        public DaySimulationResult SimulateDay(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var shopResults = state.Shops
                .Select(shop => SimulateShopDay(shop, state))
                .ToList();

            var revenue = shopResults.Sum(r => r.Revenue);
            var rentCosts = shopResults.Sum(r => r.RentCosts);
            var salaryCosts = shopResults.Sum(r => r.SalaryCosts);
            var ingredientCosts = shopResults.Sum(r => r.IngredientCosts);
            var costs = rentCosts + salaryCosts + ingredientCosts;
            var customers = shopResults.Sum(r => r.Customers);
            var profit = revenue - costs;

            var record = new DailyRecord
            {
                Day = state.CurrentDay,
                Revenue = revenue,
                Costs = costs,
                Customers = customers,
                RentCosts = rentCosts,
                SalaryCosts = salaryCosts,
                IngredientCosts = ingredientCosts,
                DeliveryCommissionCosts = 0,
                LoanPayments = 0,
                Investments = 0,
            };

            state.Cash = Math.Round(state.Cash + profit, 2);
            state.TotalRevenue = Math.Round(state.TotalRevenue + revenue, 2);
            state.TotalProfit = Math.Round(state.TotalProfit + profit, 2);
            state.CustomersServedTotal += customers;
            state.History.Add(record);
            state.CurrentDay += 1;
            state.CurrentHour = 0;

            return new DaySimulationResult(
                day: record.Day,
                revenue: record.Revenue,
                costs: record.Costs,
                profit: record.Profit,
                customers: record.Customers,
                shopResults: shopResults);
        }

        public ShopDaySimulationResult SimulateShopDay(Shop shop, GameState state)
        {
            if (shop == null) throw new ArgumentNullException(nameof(shop));
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (!shop.IsOpen)
            {
                return new ShopDaySimulationResult(
                    shopId: shop.Id,
                    revenue: 0,
                    rentCosts: 0,
                    salaryCosts: 0,
                    ingredientCosts: 0,
                    customers: 0);
            }

            var activeProducts = shop.Menu
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    ShopProduct = p,
                    Data = GameData.AllProducts.FirstOrDefault(d => d.Id == p.ProductId),
                })
                .Where(p => p.Data != null)
                .ToList();

            if (activeProducts.Count == 0)
            {
                return new ShopDaySimulationResult(
                    shopId: shop.Id,
                    revenue: 0,
                    rentCosts: Math.Round(shop.DailyRent, 2),
                    salaryCosts: Math.Round(SalaryCosts(shop, state), 2),
                    ingredientCosts: 0,
                    customers: 0);
            }

            var modifiers = state.Modifiers;
            var equipment = EquipmentDataFor(shop).ToList();
            var avgPrice = activeProducts.Average(p => p.ShopProduct.Price);
            var avgBasePrice = activeProducts.Average(p => p.Data.BasePrice);
            var priceRatio = avgBasePrice <= 0 ? 1.0 : avgPrice / avgBasePrice;
            var priceDemand = Clamp(1.0 - ((priceRatio - 1.0) * 0.55 * modifiers.CustomerPriceSensitivityMultiplier), 0.55, 1.25);
            var reputationDemand = Clamp(0.70 + (shop.Reputation / 5.0) * 0.45, 0.70, 1.15);
            var moraleDemand = Clamp(0.75 + shop.Morale * 0.30, 0.80, 1.10);
            var regularsDemand = 1.0 + Clamp(shop.Regulars, 0.0, 0.5) * 0.25;
            var brandDemand = state.Brand?.CustomerMultiplier(shop.CityId) ?? 1.0;
            var difficultyDemand = modifiers.ProgressSpeedMultiplier / modifiers.EconomicPressureMultiplier;
            var sizeDemand = ShopSizing.ConfigFor(shop.SizeTier).CapacityMultiplier;

            var rawDemand = shop.FootTraffic / 100.0
                * priceDemand
                * reputationDemand
                * moraleDemand
                * regularsDemand
                * brandDemand
                * difficultyDemand
                * sizeDemand;

            var staffCapacity = Math.Max(12.0, shop.Employees.Sum(e => 18.0 + (e.SpeedFactor * 22.0)));
            var equipmentCapacity = 35.0 + equipment.Sum(e => e.CapacityBonus);
            var capacity = Math.Max(1.0, Math.Min(rawDemand, Math.Min(staffCapacity, equipmentCapacity) * sizeDemand));
            var customers = Math.Max(0, (int)Math.Round(capacity, MidpointRounding.AwayFromZero));

            var revenue = Math.Round(customers * avgPrice, 2);
            var ingredientSaving = Clamp(equipment.Sum(e => e.IngredientSavingBonus), 0.0, 0.35);
            var ingredientCostPerCustomer = activeProducts.Average(p => p.Data.IngredientCostPerUnit) * (1.0 - ingredientSaving);
            var ingredientCosts = Math.Round(customers * ingredientCostPerCustomer, 2);
            var rentCosts = Math.Round(shop.DailyRent * ShopSizing.ConfigFor(shop.SizeTier).RentMultiplier, 2);
            var salaryCosts = Math.Round(SalaryCosts(shop, state), 2);

            return new ShopDaySimulationResult(
                shopId: shop.Id,
                revenue: revenue,
                rentCosts: rentCosts,
                salaryCosts: salaryCosts,
                ingredientCosts: ingredientCosts,
                customers: customers);
        }

        private static double SalaryCosts(Shop shop, GameState state)
            => shop.Employees.Sum(e => e.SalaryPerDay) * state.Modifiers.CandidateSalaryMultiplier;

        private static IEnumerable<EquipmentData> EquipmentDataFor(Shop shop)
            => shop.Equipment
                .Select(e => GameCatalog.AllEquipment.FirstOrDefault(d => d.Id == e.EquipmentId))
                .Where(e => e != null);

        private static double Clamp(double value, double min, double max)
            => value < min ? min : (value > max ? max : value);
    }

    public sealed class DaySimulationResult
    {
        public DaySimulationResult(
            int day,
            double revenue,
            double costs,
            double profit,
            int customers,
            IReadOnlyList<ShopDaySimulationResult> shopResults)
        {
            Day = day;
            Revenue = Math.Round(revenue, 2);
            Costs = Math.Round(costs, 2);
            Profit = Math.Round(profit, 2);
            Customers = customers;
            ShopResults = shopResults;
        }

        public int Day { get; }
        public double Revenue { get; }
        public double Costs { get; }
        public double Profit { get; }
        public int Customers { get; }
        public IReadOnlyList<ShopDaySimulationResult> ShopResults { get; }
    }

    public sealed class ShopDaySimulationResult
    {
        public ShopDaySimulationResult(
            string shopId,
            double revenue,
            double rentCosts,
            double salaryCosts,
            double ingredientCosts,
            int customers)
        {
            ShopId = shopId;
            Revenue = Math.Round(revenue, 2);
            RentCosts = Math.Round(rentCosts, 2);
            SalaryCosts = Math.Round(salaryCosts, 2);
            IngredientCosts = Math.Round(ingredientCosts, 2);
            Customers = customers;
        }

        public string ShopId { get; }
        public double Revenue { get; }
        public double RentCosts { get; }
        public double SalaryCosts { get; }
        public double IngredientCosts { get; }
        public double Costs => Math.Round(RentCosts + SalaryCosts + IngredientCosts, 2);
        public double Profit => Math.Round(Revenue - Costs, 2);
        public int Customers { get; }
    }
}
