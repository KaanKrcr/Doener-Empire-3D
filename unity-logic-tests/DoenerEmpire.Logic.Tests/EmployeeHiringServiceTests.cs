using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class EmployeeHiringServiceTests
    {
        [Fact]
        public void HireEmployeeDeductsHiringCostAddsEmployeeAndRemovesCandidate()
        {
            GameState state = StateWithShop(1000);
            state.EmployeePool.Add(Candidate("emp_1", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "shop_1", "emp_1");

            Assert.True(result.Success);
            Assert.Equal(100, result.Cost);
            Assert.Equal("emp_1", result.EmployeeId);
            Assert.Equal(900, state.Cash);
            Assert.Single(state.Shops[0].Employees);
            Assert.Equal("emp_1", state.Shops[0].Employees[0].Id);
            Assert.Empty(state.EmployeePool);
        }

        [Fact]
        public void HireEmployeeWithInvalidShopLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);
            state.EmployeePool.Add(Candidate("emp_1", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "missing", "emp_1");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Empty(state.Shops[0].Employees);
            Assert.Single(state.EmployeePool);
        }

        [Fact]
        public void HireEmployeeWithInvalidEmployeeIdLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);
            state.EmployeePool.Add(Candidate("emp_1", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "shop_1", "missing");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Empty(state.Shops[0].Employees);
            Assert.Single(state.EmployeePool);
        }

        [Fact]
        public void HireDuplicateEmployeeLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);
            state.Shops[0].Employees.Add(Candidate("emp_1", salary: 80));
            state.EmployeePool.Add(Candidate("emp_1", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "shop_1", "emp_1");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Single(state.Shops[0].Employees);
            Assert.Single(state.EmployeePool);
        }

        [Fact]
        public void HireAtEmployeeCapLeavesStateUnchanged()
        {
            GameState state = StateWithShop(1000);
            state.Shops[0].Employees.Add(Candidate("emp_1", salary: 80));
            state.Shops[0].Employees.Add(Candidate("emp_2", salary: 80));
            state.Shops[0].Employees.Add(Candidate("emp_3", salary: 80));
            state.EmployeePool.Add(Candidate("emp_4", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "shop_1", "emp_4");

            Assert.False(result.Success);
            Assert.Equal(1000, state.Cash);
            Assert.Equal(3, state.Shops[0].Employees.Count);
            Assert.Single(state.EmployeePool);
        }

        [Fact]
        public void HireEmployeeWithInsufficientCashLeavesStateUnchanged()
        {
            GameState state = StateWithShop(99);
            state.EmployeePool.Add(Candidate("emp_1", salary: 80));

            EmployeeHiringResult result = new EmployeeHiringService()
                .HireEmployee(state, "shop_1", "emp_1");

            Assert.False(result.Success);
            Assert.Equal(99, state.Cash);
            Assert.Empty(state.Shops[0].Employees);
            Assert.Single(state.EmployeePool);
        }

        private static GameState StateWithShop(double cash)
        {
            GameState state = GameState.Initial("Test", "Kaan", cash);
            state.Cash = cash;
            state.Shops.Add(new Shop
            {
                Id = "shop_1",
                CityId = "fulda",
                SizeTier = ShopSizeTier.Klein,
            });
            return state;
        }

        private static Employee Candidate(string id, double salary) => new()
        {
            Id = id,
            TypeId = "kassierer",
            Name = id,
            Speed = 5,
            Friendliness = 5,
            Reliability = 5,
            Experience = 5,
            SalaryPerDay = salary,
        };
    }
}
