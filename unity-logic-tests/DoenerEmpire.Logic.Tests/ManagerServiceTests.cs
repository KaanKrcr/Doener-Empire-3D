// Tests für ManagerService — Manager-Zuweisung.
// Spiegelt lib/services/corporate_engine.dart (assignManager/unassignManager).

using System.Collections.Generic;
using Xunit;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ManagerServiceTests
    {
        private static GameState State()
            => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        [Fact]
        public void AssignAddsManagerId()
        {
            var s = State();
            ManagerService.AssignManager(s, "e1");
            Assert.Contains("e1", s.ManagerEmployeeIds);
        }

        [Fact]
        public void AssignIsIdempotent()
        {
            var s = State();
            ManagerService.AssignManager(s, "e1");
            ManagerService.AssignManager(s, "e1");
            Assert.Single(s.ManagerEmployeeIds);
        }

        [Fact]
        public void UnassignRemovesManagerId()
        {
            var s = State();
            ManagerService.AssignManager(s, "e1");
            ManagerService.UnassignManager(s, "e1");
            Assert.DoesNotContain("e1", s.ManagerEmployeeIds);
        }

        [Fact]
        public void UnassignUnknownIsSafe()
        {
            var s = State();
            ManagerService.UnassignManager(s, "ghost");
            Assert.Empty(s.ManagerEmployeeIds);
        }

        [Fact]
        public void ShopHasActiveManagerDetectsAssignedEmployee()
        {
            var s = State();
            var shop = new Shop { Id = "s1", CityId = "fulda" };
            shop.Employees.Add(new Employee { Id = "e1", TypeId = "kassierer" });
            s.Shops.Add(shop);

            Assert.False(ManagerService.ShopHasActiveManager(shop, s));
            ManagerService.AssignManager(s, "e1");
            Assert.True(ManagerService.ShopHasActiveManager(shop, s));
        }

        [Fact]
        public void ShopWithoutAssignedEmployeeHasNoManager()
        {
            var s = State();
            var shop = new Shop { Id = "s1", CityId = "fulda" };
            shop.Employees.Add(new Employee { Id = "e1", TypeId = "kassierer" });
            s.Shops.Add(shop);
            // Manager-ID gehört zu anderem Mitarbeiter
            ManagerService.AssignManager(s, "other");
            Assert.False(ManagerService.ShopHasActiveManager(shop, s));
        }
    }
}
