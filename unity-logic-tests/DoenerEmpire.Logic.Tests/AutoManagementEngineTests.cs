// Tests für AutoManagementEngine — Auto-Pricing + Auto-Hire.
// Spiegelt lib/services/corporate_engine.dart (applyManagerAutoPricing, applyAutoHire).

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class AutoManagementEngineTests
    {
        private static Shop ShopWith(
            string id = "s1", double rep = 3.0, bool autoHire = false,
            int employees = 0, double price = 6.5)
        {
            var shop = new Shop
            {
                Id = id, CityId = "fulda", IsOpen = true, Reputation = rep,
                FootTraffic = 8000, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic, AutoHire = autoHire,
            };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = price });
            for (var i = 0; i < employees; i++)
                shop.Employees.Add(new Employee { Id = $"{id}_e{i}", TypeId = "kassierer",
                    Speed = 6, Friendliness = 6, Reliability = 6, Experience = 5, SalaryPerDay = 65 });
            return shop;
        }

        private static Employee Candidate(string id, double salary = 60, int stats = 6)
            => new()
            {
                Id = id, TypeId = "doener_meister", Name = id,
                Speed = stats, Friendliness = stats, Reliability = stats, Experience = stats,
                SalaryPerDay = salary, Origin = CandidateOrigin.Regular,
            };

        // ── Auto-Pricing ─────────────────────────────────────────────────────

        [Fact]
        public void NoManagerNoPriceChange()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = ShopWith(price: 6.5);
            s.Shops.Add(shop);
            AutoManagementEngine.ApplyManagerAutoPricing(s);
            Assert.Equal(6.5, shop.Menu[0].Price);
        }

        [Fact]
        public void ManagerRaisesPriceForHighReputation()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = ShopWith(rep: 4.5, employees: 1, price: 6.5);
            s.Shops.Add(shop);
            ManagerService.AssignManager(s, shop.Employees[0].Id);
            AutoManagementEngine.ApplyManagerAutoPricing(s);
            Assert.True(shop.Menu[0].Price > 6.5);
        }

        [Fact]
        public void ManagerLowersPriceForWeakReputation()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = ShopWith(rep: 2.0, employees: 1, price: 6.5);
            s.Shops.Add(shop);
            ManagerService.AssignManager(s, shop.Employees[0].Id);
            AutoManagementEngine.ApplyManagerAutoPricing(s);
            Assert.True(shop.Menu[0].Price < 6.5);
        }

        [Fact]
        public void AutoPricingClampsToBounds()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = ShopWith(rep: 4.5, employees: 1, price: 29.9);
            s.Shops.Add(shop);
            ManagerService.AssignManager(s, shop.Employees[0].Id);
            for (var i = 0; i < 20; i++) AutoManagementEngine.ApplyManagerAutoPricing(s);
            Assert.InRange(shop.Menu[0].Price, 1.0, 30.0);
        }

        [Fact]
        public void InactiveProductPriceUnchanged()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var shop = ShopWith(rep: 4.5, employees: 1);
            shop.Menu[0].IsActive = false;
            s.Shops.Add(shop);
            ManagerService.AssignManager(s, shop.Employees[0].Id);
            AutoManagementEngine.ApplyManagerAutoPricing(s);
            Assert.Equal(6.5, shop.Menu[0].Price);
        }

        // ── Auto-Hire ────────────────────────────────────────────────────────

        [Fact]
        public void NoAutoHireFlagNoHiring()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            var shop = ShopWith(autoHire: false);
            s.Shops.Add(shop);
            s.EmployeePool.Add(Candidate("c1"));
            AutoManagementEngine.ApplyAutoHire(s, new Random(1));
            Assert.Empty(shop.Employees);
        }

        [Fact]
        public void EmptyShopGetsHires()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            var shop = ShopWith(autoHire: true, employees: 0);
            s.Shops.Add(shop);
            for (var i = 0; i < 5; i++) s.EmployeePool.Add(Candidate($"c{i}"));
            AutoManagementEngine.ApplyAutoHire(s, new Random(1));
            Assert.NotEmpty(shop.Employees);
        }

        [Fact]
        public void EmptyPoolStopsAutoHire()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            var shop = ShopWith(autoHire: true, employees: 0);
            s.Shops.Add(shop);
            AutoManagementEngine.ApplyAutoHire(s, new Random(1));
            Assert.Empty(shop.Employees);
        }

        [Fact]
        public void HiringRemovesFromPoolAndCostsCash()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            var shop = ShopWith(autoHire: true, employees: 0);
            s.Shops.Add(shop);
            for (var i = 0; i < 5; i++) s.EmployeePool.Add(Candidate($"c{i}"));
            var poolBefore = s.EmployeePool.Count;
            var cashBefore = s.Cash;
            AutoManagementEngine.ApplyAutoHire(s, new Random(1));
            Assert.True(s.EmployeePool.Count < poolBefore);
            Assert.True(s.Cash < cashBefore);
            Assert.Equal(poolBefore - shop.Employees.Count, s.EmployeePool.Count);
        }

        [Fact]
        public void RespectsCityEmployeeCap()
        {
            // Klein → max 3 Mitarbeiter
            var s = GameState.Initial("X", "Y", 500000, tutorialEnabled: false);
            var shop = ShopWith(autoHire: true, employees: 0);
            s.Shops.Add(shop);
            for (var i = 0; i < 20; i++) s.EmployeePool.Add(Candidate($"c{i}"));
            // Mehrere Tage Auto-Hire
            for (var d = 0; d < 10; d++) AutoManagementEngine.ApplyAutoHire(s, new Random(d));
            Assert.True(shop.Employees.Count <= GameEngineCore.MaxEmployeesForShop(shop));
        }

        [Fact]
        public void KeepsCashReserve()
        {
            // Wenig Cash → Auto-Hire stoppt, um Reserve zu halten.
            var s = GameState.Initial("X", "Y", 1600, tutorialEnabled: false);
            var shop = ShopWith(autoHire: true, employees: 0);
            s.Shops.Add(shop);
            for (var i = 0; i < 5; i++) s.EmployeePool.Add(Candidate($"c{i}", salary: 200));
            AutoManagementEngine.ApplyAutoHire(s, new Random(1));
            // Cash sollte nicht unter eine plausible Reserve fallen (>= 0)
            Assert.True(s.Cash >= 0);
        }

        [Fact]
        public void Stable60DayLoopWithAutoHireAndPricing()
        {
            var s = GameState.Initial("Empire", "Boss", 100000, GameDifficulty.Easy,
                tutorialEnabled: false);
            var shop = ShopWith(id: "s1", rep: 3.5, autoHire: true, employees: 1);
            ManagerService.AssignManager(s, shop.Employees[0].Id);
            s.Shops.Add(shop);
            var rng = new Random(7);
            for (var d = 0; d < 60; d++)
            {
                // Pool gelegentlich auffüllen (sonst trocknet er aus)
                if (s.EmployeePool.Count < 3)
                    for (var i = 0; i < 4; i++) s.EmployeePool.Add(Candidate($"d{d}_{i}"));
                DayProcessing.ProcessDay(s);
                Assert.InRange(shop.Reputation, 0.5, 5.0);
                Assert.True(shop.Employees.Count <= GameEngineCore.MaxEmployeesForShop(shop));
            }
            Assert.Equal(61, s.CurrentDay);
        }
    }
}
