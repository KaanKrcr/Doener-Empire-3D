using System.Text.Json;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Save;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class SaveServiceTests
    {
        [Fact]
        public void RoundtripPreservesCurrentMvpGameState()
        {
            var state = GameState.Initial("Sultan Doener GmbH", "Kaan", 42420, GameDifficulty.Hard);
            state.CurrentDay = 12;
            state.CurrentHour = 9;
            state.UnlockedCityIds.Add("berlin");
            state.Brand.BrandAwareness = 18.5;
            state.Brand.CityReputation["fulda"] = 42.0;
            state.Shops.Add(new Shop
            {
                Id = "shop_fulda_1",
                Name = "Sultan Doener",
                CustomName = "Mitte",
                CityId = "fulda",
                LocationName = "Marktplatz",
                FootTraffic = 5100,
                WeeklyRent = 1470,
                DayOpened = 2,
                Reputation = 4.2,
                Personality = LocationPersonality.Business,
                SizeTier = ShopSizeTier.Gross,
                UpgradeIds = { "wifi" },
                Menu =
                {
                    new ShopProduct { ProductId = "doener_fladen", Price = 6.9, IsActive = true },
                    new ShopProduct { ProductId = "doener_box", Price = 7.5, IsActive = false },
                },
                Equipment = { new ShopEquipment { EquipmentId = "spiess_standard" } },
                Employees =
                {
                    new Employee
                    {
                        Id = "emp_1",
                        TypeId = "service",
                        Name = "Ali",
                        Speed = 8,
                        Friendliness = 7,
                        Reliability = 9,
                        Experience = 6,
                        SalaryPerDay = 96.5,
                        Traits = { PersonalityTrait.Charmer, PersonalityTrait.Loyal },
                        DaysEmployed = 5,
                        Origin = CandidateOrigin.ExCompetitor,
                        GrowthPotential = 0.6,
                        Shift = EmployeeShift.Abend,
                    },
                },
            });
            state.Competitors.Add(new Competitor
            {
                Id = "comp_1",
                Name = "King Doener",
                CityId = "fulda",
                Personality = CompetitorPersonality.CheapMass,
                ShopCount = 3,
                Reputation = 3.7,
                PriceLevel = 0.86,
                MarketShare = 0.23,
                DaysSinceLastAction = 4,
            });
            state.Loans.Add(new Loan
            {
                Id = "loan_1",
                Amount = 10000,
                InterestRate = 0.06,
                DurationDays = 120,
                DayTaken = 3,
                AmountPaid = 800,
            });

            var json = SaveService.ToJson(state);
            var loaded = SaveService.FromJson(json);

            Assert.Equal(42420, loaded.Cash);
            Assert.Equal(12, loaded.CurrentDay);
            Assert.Equal(9, loaded.CurrentHour);
            Assert.Equal(GameDifficulty.Hard, loaded.Difficulty);
            Assert.Contains("berlin", loaded.UnlockedCityIds);
            Assert.Equal(18.5, loaded.Brand.BrandAwareness);
            Assert.Equal(42.0, loaded.Brand.CityReputation["fulda"]);
            Assert.Single(loaded.Shops);
            Assert.Equal(ShopSizeTier.Gross, loaded.Shops[0].SizeTier);
            Assert.Equal(LocationPersonality.Business, loaded.Shops[0].Personality);
            Assert.Equal(2, loaded.Shops[0].Menu.Count);
            Assert.False(loaded.Shops[0].Menu[1].IsActive);
            Assert.Single(loaded.Shops[0].Equipment);
            Assert.Single(loaded.Shops[0].Employees);
            Assert.Equal(CandidateOrigin.ExCompetitor, loaded.Shops[0].Employees[0].Origin);
            Assert.Equal(EmployeeShift.Abend, loaded.Shops[0].Employees[0].Shift);
            Assert.Contains(PersonalityTrait.Charmer, loaded.Shops[0].Employees[0].Traits);
            Assert.Single(loaded.Competitors);
            Assert.Equal(CompetitorPersonality.CheapMass, loaded.Competitors[0].Personality);
            Assert.Single(loaded.Loans);
            Assert.Equal(800, loaded.Loans[0].AmountPaid);
        }

        [Fact]
        public void WritesDartCompatibleEnumStrings()
        {
            var state = GameState.Initial("Sultan Doener GmbH", "Kaan", 1000, GameDifficulty.Impossible);
            state.Shops.Add(new Shop
            {
                Id = "s1",
                Name = "Sultan",
                CityId = "fulda",
                LocationName = "Mitte",
                Personality = LocationPersonality.Nightlife,
                SizeTier = ShopSizeTier.Flagship,
                Employees =
                {
                    new Employee
                    {
                        Id = "e1",
                        Name = "Ayse",
                        Origin = CandidateOrigin.HiddenGem,
                        Shift = EmployeeShift.Frueh,
                        Traits = { PersonalityTrait.Workaholic },
                    },
                },
            });
            state.Competitors.Add(new Competitor
            {
                Id = "c1",
                Name = "Cheap",
                CityId = "fulda",
                Personality = CompetitorPersonality.CheapMass,
            });

            using var doc = JsonDocument.Parse(SaveService.ToJson(state));
            var root = doc.RootElement;
            Assert.Equal("impossible", root.GetProperty("difficulty").GetString());
            Assert.Equal("nightlife", root.GetProperty("shops")[0].GetProperty("personality").GetString());
            Assert.Equal("flagship", root.GetProperty("shops")[0].GetProperty("sizeTier").GetString());
            Assert.Equal("hiddenGem", root.GetProperty("shops")[0].GetProperty("employees")[0].GetProperty("origin").GetString());
            Assert.Equal("frueh", root.GetProperty("shops")[0].GetProperty("employees")[0].GetProperty("shift").GetString());
            Assert.Equal("workaholic", root.GetProperty("shops")[0].GetProperty("employees")[0].GetProperty("traits")[0].GetString());
            Assert.Equal("cheapMass", root.GetProperty("competitors")[0].GetProperty("personality").GetString());
        }
    }
}
