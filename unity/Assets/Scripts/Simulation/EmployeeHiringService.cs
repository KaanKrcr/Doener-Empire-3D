using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct EmployeeHiringResult
    {
        public readonly bool Success;
        public readonly string ErrorMessage;
        public readonly double Cost;
        public readonly string EmployeeId;

        private EmployeeHiringResult(bool success, string errorMessage, double cost, string employeeId)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Cost = cost;
            EmployeeId = employeeId;
        }

        public static EmployeeHiringResult Hired(double cost, string employeeId) =>
            new(true, null, cost, employeeId);

        public static EmployeeHiringResult Failed(string message) =>
            new(false, message, 0, null);
    }

    public sealed class EmployeeHiringService
    {
        private const double HiringFeeMultiplier = 1.25;

        public EmployeeHiringResult HireEmployee(GameState state, string shopId, string employeeId)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(shopId))
            {
                return EmployeeHiringResult.Failed("Filiale nicht gefunden.");
            }

            Shop shop = state.Shops.FirstOrDefault(candidate => candidate.Id == shopId);
            if (shop == null)
            {
                return EmployeeHiringResult.Failed("Filiale nicht gefunden.");
            }

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return EmployeeHiringResult.Failed("Bewerber nicht gefunden.");
            }

            if (shop.Employees.Any(employee => employee.Id == employeeId))
            {
                return EmployeeHiringResult.Failed("Mitarbeiter ist bereits in dieser Filiale.");
            }

            Employee candidate = state.EmployeePool.FirstOrDefault(employee => employee.Id == employeeId);
            if (candidate == null)
            {
                return EmployeeHiringResult.Failed("Bewerber nicht gefunden.");
            }

            if (!GameCatalog.EmployeeTypes.Any(type => type.Id == candidate.TypeId))
            {
                return EmployeeHiringResult.Failed("Bewerberrolle ist nicht verfuegbar.");
            }

            if (shop.Employees.Count >= EmployeeCapFor(shop))
            {
                return EmployeeHiringResult.Failed("Personal-Cap dieser Filiale ist erreicht.");
            }

            double cost = Math.Round(candidate.SalaryPerDay * HiringFeeMultiplier, 2);
            if (state.Cash < cost)
            {
                return EmployeeHiringResult.Failed("Nicht genug Kapital fuer diese Einstellung.");
            }

            state.EmployeePool.Remove(candidate);
            shop.Employees.Add(candidate);
            state.Cash = Math.Round(state.Cash - cost, 2);
            return EmployeeHiringResult.Hired(cost, candidate.Id);
        }

        private static int EmployeeCapFor(Shop shop)
        {
            CityData city = GameData.AllCities.FirstOrDefault(candidate => candidate.Id == shop.CityId);
            CityTier cityTier = city?.Tier ?? CityTier.Klein;
            return ShopSizing.EmployeeCap(cityTier, shop.SizeTier);
        }
    }
}
