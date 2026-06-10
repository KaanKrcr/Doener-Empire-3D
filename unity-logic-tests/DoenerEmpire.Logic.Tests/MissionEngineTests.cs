// Tests für MissionEngine — spiegeln das Verhalten der Dart-Engine
// (lib/services/mission_engine.dart).

using System.Collections.Generic;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class MissionEngineTests
    {
        private static GameState NewState(GameDifficulty diff = GameDifficulty.Normal)
            => GameState.Initial("Test GmbH", "Tester", 1000, diff,
                tutorialEnabled: false);

        // ──────────────────────────────────────────────────────────────────────
        // ActiveMission / DoneCount
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void ActiveMissionReturnsFirstNotDone()
        {
            var ms = MissionTemplates.Build();
            Assert.Equal("open_first_shop", MissionEngine.ActiveMission(ms).Id);
            ms[0].IsDone = true;
            Assert.Equal("first_1000", MissionEngine.ActiveMission(ms).Id);
        }

        [Fact]
        public void ActiveMissionReturnsNullWhenAllDone()
        {
            var ms = MissionTemplates.Build();
            foreach (var m in ms) m.IsDone = true;
            Assert.Null(MissionEngine.ActiveMission(ms));
        }

        [Fact]
        public void DoneCountReflectsState()
        {
            var ms = MissionTemplates.Build();
            ms[0].IsDone = true;
            ms[2].IsDone = true;
            Assert.Equal(2, MissionEngine.DoneCount(ms));
        }

        // ──────────────────────────────────────────────────────────────────────
        // ActiveProgress
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void ProgressIs1WhenNoActiveMission()
        {
            var state = NewState();
            var ms = new List<Mission>();
            Assert.Equal(1.0, MissionEngine.ActiveProgress(state, ms));
        }

        [Fact]
        public void ProgressIs0AtStartForOpenFirstShop()
        {
            var state = NewState();
            var ms = MissionTemplates.Build();
            Assert.Equal(0.0, MissionEngine.ActiveProgress(state, ms));
        }

        [Fact]
        public void ProgressIs1AfterOpeningFirstShop()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });
            var ms = MissionTemplates.Build();
            Assert.Equal(1.0, MissionEngine.ActiveProgress(state, ms));
        }

        // ──────────────────────────────────────────────────────────────────────
        // CheckAndApply
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void CheckAndApplyCompletesAndPaysReward()
        {
            var state = NewState();
            var startCash = state.Cash;
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });
            var ms = MissionTemplates.Build();

            var res = MissionEngine.CheckAndApply(state, ms);
            Assert.NotNull(res.JustCompleted);
            Assert.Equal("open_first_shop", res.JustCompleted.Id);
            Assert.True(ms[0].IsDone);
            Assert.Equal(startCash + 500, state.Cash);
        }

        [Fact]
        public void CheckAndApplyReturnsNullWhenIncomplete()
        {
            var state = NewState();
            var ms = MissionTemplates.Build();
            var res = MissionEngine.CheckAndApply(state, ms);
            Assert.Null(res.JustCompleted);
            Assert.False(ms[0].IsDone);
        }

        [Fact]
        public void CheckAndApplyOnlyOneCompletionPerCall()
        {
            var state = NewState();
            // Genug für mehrere Missionen
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });
            state.TotalRevenue = 5000;
            var ms = MissionTemplates.Build();

            var r1 = MissionEngine.CheckAndApply(state, ms);
            Assert.Equal("open_first_shop", r1.JustCompleted.Id);

            var r2 = MissionEngine.CheckAndApply(state, ms);
            Assert.Equal("first_1000", r2.JustCompleted.Id);
        }

        // ──────────────────────────────────────────────────────────────────────
        // CurrentValueFor — pro MissionType
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void TotalRevenueScalesByProgressSpeed()
        {
            var hard = NewState(GameDifficulty.Hard);
            hard.TotalRevenue = 1000;
            var probe = new Mission { Id = "p", Type = MissionType.TotalRevenue, Target = 1000 };
            var p = MissionEngine.ActiveProgress(hard, new List<Mission> { probe });
            // Hard: progressSpeed=0.8 → 1000*0.8/1000 = 0.8
            Assert.Equal(0.8, p, precision: 5);
        }

        [Fact]
        public void HireEmployeesCountsAcrossShops()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda",
                Employees = new List<Employee> { new(), new() } });
            state.Shops.Add(new Shop { Id = "s2", CityId = "fulda",
                Employees = new List<Employee> { new() } });
            var probe = new Mission { Id = "p", Type = MissionType.HireEmployees, Target = 3 };
            Assert.Equal(3.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void BuyEquipmentCountsAllInstalled()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda",
                Equipment = new List<ShopEquipment>
                {
                    new() { EquipmentId = "spiess_klein" },
                    new() { EquipmentId = "kasse_basic" },
                } });
            var probe = new Mission { Id = "p", Type = MissionType.BuyEquipment, Target = 2 };
            Assert.Equal(2.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void UnlockProductDetectsEquipmentWithUnlock()
        {
            var state = NewState();
            // spiess_klein hat KEIN UnlocksProductId
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda",
                Equipment = new List<ShopEquipment>
                {
                    new() { EquipmentId = "spiess_klein" },
                } });
            var probe = new Mission { Id = "p", Type = MissionType.UnlockProduct, Target = 1 };
            Assert.Equal(0.0, MissionEngine.CurrentValueFor(probe, state));

            // fritteuse_standard schaltet pommes/doenerbox frei
            state.Shops[0].Equipment.Add(new ShopEquipment { EquipmentId = "fritteuse_standard" });
            Assert.Equal(1.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void ShopCountMetropoleOnlyCountsMetropoleShops()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "a", CityId = "fulda" });     // klein
            state.Shops.Add(new Shop { Id = "b", CityId = "berlin" });    // metropole
            state.Shops.Add(new Shop { Id = "c", CityId = "hamburg" });   // metropole

            var generic = new Mission { Id = "x", Type = MissionType.ShopCount, Target = 1 };
            Assert.Equal(3.0, MissionEngine.CurrentValueFor(generic, state));

            var metropole = new Mission { Id = "metropole", Type = MissionType.ShopCount, Target = 1 };
            Assert.Equal(2.0, MissionEngine.CurrentValueFor(metropole, state));
        }

        [Fact]
        public void UnlockCityIgnoresStartingThree()
        {
            var state = NewState();
            // Default Initial: fulda, bayreuth, goettingen = 3 → 0
            var probe = new Mission { Id = "p", Type = MissionType.UnlockCity, Target = 1 };
            Assert.Equal(0.0, MissionEngine.CurrentValueFor(probe, state));
            state.UnlockedCityIds.Add("augsburg");
            Assert.Equal(1.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void ReputationLevelTakesMaxAcrossShops()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "a", CityId = "fulda", Reputation = 3.2 });
            state.Shops.Add(new Shop { Id = "b", CityId = "fulda", Reputation = 4.1 });
            state.Shops.Add(new Shop { Id = "c", CityId = "fulda", Reputation = 2.9 });
            var probe = new Mission { Id = "p", Type = MissionType.ReputationLevel, Target = 4.0 };
            Assert.Equal(4.1, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void ReputationLevelIsZeroWithoutShops()
        {
            var state = NewState();
            var probe = new Mission { Id = "p", Type = MissionType.ReputationLevel, Target = 4.0 };
            Assert.Equal(0.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void BrandAwarenessReadFromBrand()
        {
            var state = NewState();
            state.Brand.BrandAwareness = 42.0;
            var probe = new Mission { Id = "p", Type = MissionType.BrandAwareness, Target = 40 };
            Assert.Equal(42.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void AcquiredShopsCountsWasAcquired()
        {
            var state = NewState();
            state.Shops.Add(new Shop { Id = "a", CityId = "fulda", WasAcquired = false });
            state.Shops.Add(new Shop { Id = "b", CityId = "fulda", WasAcquired = true });
            state.Shops.Add(new Shop { Id = "c", CityId = "fulda", WasAcquired = true });
            var probe = new Mission { Id = "p", Type = MissionType.AcquiredShops, Target = 2 };
            Assert.Equal(2.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void CompanyPublicReflectsStocksIsPublic()
        {
            var state = NewState();
            var probe = new Mission { Id = "p", Type = MissionType.CompanyPublic, Target = 1 };
            Assert.Equal(0.0, MissionEngine.CurrentValueFor(probe, state));

            state.Stocks.IsPublic = true;
            Assert.Equal(1.0, MissionEngine.CurrentValueFor(probe, state));
        }

        [Fact]
        public void StocksClonesAndPreservesFields()
        {
            var state = NewState();
            state.Stocks.IsPublic = true;
            state.Stocks.SharePrice = 12.5;
            state.Stocks.TotalShares = 1000;
            state.Stocks.PlayerShares = 600;
            var clone = state.Clone();
            Assert.NotSame(state.Stocks, clone.Stocks);
            Assert.True(clone.Stocks.IsPublic);
            Assert.Equal(12.5, clone.Stocks.SharePrice);
            Assert.Equal(0.60, clone.Stocks.PlayerShareRatio, precision: 5);
            Assert.True(clone.Stocks.HasControl);
        }

        [Fact]
        public void DaysSurvivedTracksCurrentDay()
        {
            var state = NewState();
            state.CurrentDay = 27;
            var probe = new Mission { Id = "p", Type = MissionType.DaysSurvived, Target = 30 };
            Assert.Equal(27.0, MissionEngine.CurrentValueFor(probe, state));
        }
    }
}
