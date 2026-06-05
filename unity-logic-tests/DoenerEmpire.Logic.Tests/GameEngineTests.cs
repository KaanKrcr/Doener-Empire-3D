using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class GameEngineTests
    {
        [Fact]
        public void SimulateDayCalculatesDeterministicTotalsAndAdvancesState()
        {
            var state = GameState.Initial("Sultan", "Kaan", 1000);
            state.Brand.BrandAwareness = 20;
            state.Brand.CityReputation["fulda"] = 30;
            state.Shops.Add(OpenShop("open", "fulda", footTraffic: 4200, rent: 700));

            var result = new GameEngine().SimulateDay(state);

            Assert.Equal(1, result.Day);
            Assert.Equal(234, result.Revenue);
            Assert.Equal(239.2, result.Costs);
            Assert.Equal(-5.2, result.Profit);
            Assert.Equal(36, result.Customers);
            Assert.Equal(994.8, state.Cash);
            Assert.Equal(2, state.CurrentDay);
            Assert.Equal(234, state.TotalRevenue);
            Assert.Equal(-5.2, state.TotalProfit);
            Assert.Equal(36, state.CustomersServedTotal);
            Assert.Single(state.History);
            Assert.Equal(result.Profit, state.History[0].Profit, 2);
        }

        [Fact]
        public void ClosedShopsGenerateNoRevenueOrCustomers()
        {
            var state = GameState.Initial("Sultan", "Kaan", 1000);
            var closed = OpenShop("closed", "fulda", footTraffic: 9000, rent: 700);
            closed.IsOpen = false;
            state.Shops.Add(closed);

            var result = new GameEngine().SimulateDay(state);

            var shopResult = Assert.Single(result.ShopResults);
            Assert.Equal(0, shopResult.Revenue);
            Assert.Equal(0, shopResult.Customers);
            Assert.Equal(0, result.Revenue);
            Assert.Equal(0, state.CustomersServedTotal);
            Assert.Equal(1000, state.Cash);
        }

        [Fact]
        public void DifficultyAndShopInputsVisiblyAffectTheResult()
        {
            var easy = GameState.Initial("Sultan", "Kaan", 1000, GameDifficulty.Easy);
            easy.Shops.Add(OpenShop("easy", "fulda", footTraffic: 2500, rent: 700));

            var hard = GameState.Initial("Sultan", "Kaan", 1000, GameDifficulty.Hard);
            hard.Shops.Add(OpenShop("hard", "fulda", footTraffic: 2500, rent: 700));

            var easyResult = new GameEngine().SimulateDay(easy);
            var hardResult = new GameEngine().SimulateDay(hard);

            Assert.True(easyResult.Customers > hardResult.Customers);
            Assert.True(easyResult.Revenue > hardResult.Revenue);
            Assert.True(hardResult.ShopResults.Single().SalaryCosts > easyResult.ShopResults.Single().SalaryCosts);
        }

        private static Shop OpenShop(string id, string cityId, int footTraffic, double rent)
        {
            return new Shop
            {
                Id = id,
                Name = "Sultan",
                CityId = cityId,
                LocationName = "Marktplatz",
                FootTraffic = footTraffic,
                WeeklyRent = rent,
                Reputation = 4,
                Morale = 0.8,
                Regulars = 0.1,
                SizeTier = ShopSizeTier.Klein,
                Menu =
                {
                    new ShopProduct { ProductId = "doener_fladen", Price = 6.5 },
                },
                Equipment =
                {
                    new ShopEquipment { EquipmentId = "spiess_klein" },
                    new ShopEquipment { EquipmentId = "kasse_basic" },
                },
                Employees =
                {
                    new Employee
                    {
                        Id = id + "_meister",
                        TypeId = "doener_meister",
                        Name = "Ali",
                        Speed = 8,
                        Friendliness = 7,
                        Reliability = 8,
                        Experience = 8,
                        SalaryPerDay = 60,
                    },
                },
            };
        }
    }
}
