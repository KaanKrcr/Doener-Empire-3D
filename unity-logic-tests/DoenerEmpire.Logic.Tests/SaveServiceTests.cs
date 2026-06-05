using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Save;

namespace DoenerEmpire.Logic.Tests
{
    public class SaveServiceTests
    {
        [Fact]
        public void RoundTripPreservesMvpState()
        {
            GameState state = GameState.Initial("Doener Empire", "Kaan", 23456, GameDifficulty.Hard, false, 3);
            state.CurrentDay = 12;
            state.CurrentHour = 7;
            state.UnlockedCityIds.Add("koeln");
            state.TotalRevenue = 99000;
            state.TotalProfit = 12345;
            state.CustomersServedTotal = 4567;
            state.Brand.BrandAwareness = 22;
            state.Brand.CityReputation["fulda"] = 64;
            state.AchievementIds.Add("first_shop");
            state.ManagerEmployeeIds.Add("emp_manager");
            state.GlobalUpgradeIds.Add("brand_logo");
            state.SeenEventIds.Add("intro");

            state.Shops.Add(new Shop
            {
                Id = "shop_fulda_hauptstrasse",
                Name = "Doener Empire",
                CustomName = "Hauptstrasse 12",
                CityId = "fulda",
                LocationName = "Hauptstrasse",
                FootTraffic = 4600,
                WeeklyRent = 1200,
                Reputation = 4.4,
                DayOpened = 2,
                Personality = LocationPersonality.Business,
                SizeTier = ShopSizeTier.Mittel,
                Morale = 0.82,
                Regulars = 0.12,
            });
            state.Shops[0].Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5, IsActive = true });
            state.Shops[0].Equipment.Add(new ShopEquipment { EquipmentId = "kasse_digital" });
            state.Shops[0].Employees.Add(new Employee
            {
                Id = "emp_1",
                TypeId = "doener_meister",
                Name = "Ali",
                Speed = 8,
                Friendliness = 7,
                Reliability = 9,
                Experience = 6,
                SalaryPerDay = 92.5,
                DaysEmployed = 5,
                Origin = CandidateOrigin.HiddenGem,
                GrowthPotential = 0.4,
                Shift = EmployeeShift.Abend,
                Traits = { PersonalityTrait.Charmer, PersonalityTrait.Loyal },
            });

            state.EmployeePool.Add(new Employee { Id = "pool_1", Name = "Lena", TypeId = "kassierer", Shift = EmployeeShift.Mittag });
            state.Competitors.Add(new Competitor
            {
                Id = "comp_1",
                Name = "King Doener",
                CityId = "fulda",
                Personality = CompetitorPersonality.CheapMass,
                ShopCount = 2,
                Reputation = 3.1,
                PriceLevel = 0.82,
                MarketShare = 0.21,
                DaysSinceLastAction = 4,
            });
            state.Loans.Add(new Loan
            {
                Id = "loan_1",
                Amount = 10000,
                InterestRate = 0.06,
                DurationDays = 180,
                DayTaken = 3,
                AmountPaid = 350,
            });

            SaveService service = new();
            string json = service.Serialize(state);
            GameState loaded = service.Deserialize(json);

            Assert.Equal(state.Cash, loaded.Cash);
            Assert.Equal(state.CurrentDay, loaded.CurrentDay);
            Assert.Equal(state.CurrentHour, loaded.CurrentHour);
            Assert.Equal(state.Difficulty, loaded.Difficulty);
            Assert.Contains("koeln", loaded.UnlockedCityIds);
            Assert.Single(loaded.Shops);
            Assert.Equal("doener_fladen", loaded.Shops[0].Menu[0].ProductId);
            Assert.Equal("kasse_digital", loaded.Shops[0].Equipment[0].EquipmentId);
            Assert.Equal(EmployeeShift.Abend, loaded.Shops[0].Employees[0].Shift);
            Assert.Equal(ShopSizeTier.Mittel, loaded.Shops[0].SizeTier);
            Assert.Single(loaded.Competitors);
            Assert.Equal(CompetitorPersonality.CheapMass, loaded.Competitors[0].Personality);
            Assert.Single(loaded.Loans);
            Assert.Equal(10000, loaded.Loans[0].Amount);
            Assert.Equal(64, loaded.Brand.CityReputation["fulda"]);
        }

        [Fact]
        public void EnumValuesAreDartCompatibleStrings()
        {
            GameState state = GameState.Initial("X", "Y", 15000, GameDifficulty.Impossible);
            state.Shops.Add(new Shop
            {
                Id = "shop_1",
                Name = "Doener Empire",
                CityId = "fulda",
                Personality = LocationPersonality.University,
                SizeTier = ShopSizeTier.Flagship,
            });
            state.Shops[0].Employees.Add(new Employee
            {
                Id = "emp_1",
                Name = "Ali",
                Origin = CandidateOrigin.TopTalent,
                Shift = EmployeeShift.Frueh,
                Traits = { PersonalityTrait.Workaholic },
            });
            state.Competitors.Add(new Competitor
            {
                Id = "comp_1",
                Name = "Premium Kebap",
                CityId = "fulda",
                Personality = CompetitorPersonality.CheapMass,
            });

            string json = new SaveService().Serialize(state);

            Assert.Contains("\"difficulty\":\"impossible\"", json);
            Assert.Contains("\"personality\":\"university\"", json);
            Assert.Contains("\"sizeTier\":\"flagship\"", json);
            Assert.Contains("\"origin\":\"topTalent\"", json);
            Assert.Contains("\"shift\":\"frueh\"", json);
            Assert.Contains("\"cheapMass\"", json);
            Assert.DoesNotContain("\"Difficulty\":", json);
            Assert.DoesNotContain("\"SizeTier\":", json);
        }
    }
}
