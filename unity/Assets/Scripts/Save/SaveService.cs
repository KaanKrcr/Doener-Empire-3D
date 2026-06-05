using System.Collections.Generic;
using System.Text.Json;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Save
{
    public sealed class SaveService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            IncludeFields = true,
            WriteIndented = false,
        };

        public string Serialize(GameState state)
        {
            return JsonSerializer.Serialize(ToDto(state), JsonOptions);
        }

        public GameState Deserialize(string json)
        {
            GameStateDto dto = JsonSerializer.Deserialize<GameStateDto>(json, JsonOptions) ?? new GameStateDto();
            return FromDto(dto);
        }

        private static GameStateDto ToDto(GameState state)
        {
            state ??= new GameState();

            return new GameStateDto
            {
                companyName = state.CompanyName,
                founderName = state.FounderName,
                cash = state.Cash,
                currentDay = state.CurrentDay,
                currentHour = state.CurrentHour,
                shops = MapList(state.Shops, ToDto),
                unlockedCityIds = new List<string>(state.UnlockedCityIds ?? new List<string>()),
                competitors = MapList(state.Competitors, ToDto),
                loans = MapList(state.Loans, ToDto),
                totalRevenue = state.TotalRevenue,
                totalProfit = state.TotalProfit,
                customersServedTotal = state.CustomersServedTotal,
                difficulty = EnumNames.ToDart(state.Difficulty),
                brand = ToDto(state.Brand),
                achievementIds = new List<string>(state.AchievementIds ?? new List<string>()),
                employeePool = MapList(state.EmployeePool, ToDto),
                lastEmployeePoolDay = state.LastEmployeePoolDay,
                managerEmployeeIds = new List<string>(state.ManagerEmployeeIds ?? new List<string>()),
                globalUpgradeIds = new List<string>(state.GlobalUpgradeIds ?? new List<string>()),
                activeThemeId = state.ActiveThemeId,
                prestigePoints = state.PrestigePoints,
                tutorialDone = state.TutorialDone,
                tutorialEnabled = state.TutorialEnabled,
                tutorialStep = state.TutorialStep,
                seenEventIds = new List<string>(state.SeenEventIds ?? new List<string>()),
            };
        }

        private static GameState FromDto(GameStateDto dto)
        {
            return new GameState
            {
                CompanyName = dto.companyName,
                FounderName = dto.founderName,
                Cash = dto.cash,
                CurrentDay = dto.currentDay,
                CurrentHour = dto.currentHour,
                Shops = MapList(dto.shops, FromDto),
                UnlockedCityIds = new List<string>(dto.unlockedCityIds ?? new List<string>()),
                Competitors = MapList(dto.competitors, FromDto),
                Loans = MapList(dto.loans, FromDto),
                TotalRevenue = dto.totalRevenue,
                TotalProfit = dto.totalProfit,
                CustomersServedTotal = dto.customersServedTotal,
                Difficulty = EnumNames.DifficultyFromDart(dto.difficulty),
                Brand = FromDto(dto.brand),
                AchievementIds = new List<string>(dto.achievementIds ?? new List<string>()),
                EmployeePool = MapList(dto.employeePool, FromDto),
                LastEmployeePoolDay = dto.lastEmployeePoolDay,
                ManagerEmployeeIds = new List<string>(dto.managerEmployeeIds ?? new List<string>()),
                GlobalUpgradeIds = new List<string>(dto.globalUpgradeIds ?? new List<string>()),
                ActiveThemeId = dto.activeThemeId ?? "klassik",
                PrestigePoints = dto.prestigePoints,
                TutorialDone = dto.tutorialDone,
                TutorialEnabled = dto.tutorialEnabled,
                TutorialStep = dto.tutorialStep,
                SeenEventIds = new List<string>(dto.seenEventIds ?? new List<string>()),
            };
        }

        private static ShopDto ToDto(Shop shop)
        {
            return new ShopDto
            {
                id = shop.Id,
                name = shop.Name,
                customName = shop.CustomName,
                cityId = shop.CityId,
                locationName = shop.LocationName,
                footTraffic = shop.FootTraffic,
                weeklyRent = shop.WeeklyRent,
                isOpen = shop.IsOpen,
                menu = MapList(shop.Menu, ToDto),
                equipment = MapList(shop.Equipment, ToDto),
                employees = MapList(shop.Employees, ToDto),
                reputation = shop.Reputation,
                dayOpened = shop.DayOpened,
                activeCampaigns = MapList(shop.ActiveCampaigns, ToDto),
                personality = EnumNames.ToDart(shop.Personality),
                upgradeIds = new List<string>(shop.UpgradeIds ?? new List<string>()),
                autoHire = shop.AutoHire,
                originalCompetitorName = shop.OriginalCompetitorName,
                wasAcquired = shop.WasAcquired,
                morale = shop.Morale,
                regulars = shop.Regulars,
                sizeTier = EnumNames.ToDart(shop.SizeTier),
            };
        }

        private static Shop FromDto(ShopDto dto)
        {
            return new Shop
            {
                Id = dto.id,
                Name = dto.name,
                CustomName = dto.customName,
                CityId = dto.cityId,
                LocationName = dto.locationName,
                FootTraffic = dto.footTraffic,
                WeeklyRent = dto.weeklyRent,
                IsOpen = dto.isOpen,
                Menu = MapList(dto.menu, FromDto),
                Equipment = MapList(dto.equipment, FromDto),
                Employees = MapList(dto.employees, FromDto),
                Reputation = dto.reputation,
                DayOpened = dto.dayOpened,
                ActiveCampaigns = MapList(dto.activeCampaigns, FromDto),
                Personality = EnumNames.LocationFromDart(dto.personality),
                UpgradeIds = new List<string>(dto.upgradeIds ?? new List<string>()),
                AutoHire = dto.autoHire,
                OriginalCompetitorName = dto.originalCompetitorName,
                WasAcquired = dto.wasAcquired,
                Morale = dto.morale,
                Regulars = dto.regulars,
                SizeTier = ShopSizing.FromDartName(dto.sizeTier),
            };
        }

        private static ShopProductDto ToDto(ShopProduct product)
        {
            return new ShopProductDto
            {
                productId = product.ProductId,
                price = product.Price,
                isActive = product.IsActive,
            };
        }

        private static ShopProduct FromDto(ShopProductDto dto)
        {
            return new ShopProduct
            {
                ProductId = dto.productId,
                Price = dto.price,
                IsActive = dto.isActive,
            };
        }

        private static ShopEquipmentDto ToDto(ShopEquipment equipment)
        {
            return new ShopEquipmentDto { equipmentId = equipment.EquipmentId };
        }

        private static ShopEquipment FromDto(ShopEquipmentDto dto)
        {
            return new ShopEquipment { EquipmentId = dto.equipmentId };
        }

        private static EmployeeDto ToDto(Employee employee)
        {
            return new EmployeeDto
            {
                id = employee.Id,
                typeId = employee.TypeId,
                name = employee.Name,
                speed = employee.Speed,
                friendliness = employee.Friendliness,
                reliability = employee.Reliability,
                experience = employee.Experience,
                salaryPerDay = employee.SalaryPerDay,
                traits = MapList(employee.Traits, EmployeeEnumNames.ToDart),
                daysEmployed = employee.DaysEmployed,
                origin = EmployeeEnumNames.ToDart(employee.Origin),
                growthPotential = employee.GrowthPotential,
                shift = EmployeeEnumNames.ToDart(employee.Shift),
            };
        }

        private static Employee FromDto(EmployeeDto dto)
        {
            return new Employee
            {
                Id = dto.id,
                TypeId = dto.typeId,
                Name = dto.name,
                Speed = dto.speed,
                Friendliness = dto.friendliness,
                Reliability = dto.reliability,
                Experience = dto.experience,
                SalaryPerDay = dto.salaryPerDay,
                Traits = MapList(dto.traits, raw => EmployeeEnumNames.TraitFromDart(raw) ?? PersonalityTrait.Modest),
                DaysEmployed = dto.daysEmployed,
                Origin = EmployeeEnumNames.OriginFromDart(dto.origin),
                GrowthPotential = dto.growthPotential,
                Shift = EmployeeEnumNames.ShiftFromDart(dto.shift),
            };
        }

        private static CompetitorDto ToDto(Competitor competitor)
        {
            return new CompetitorDto
            {
                id = competitor.Id,
                name = competitor.Name,
                cityId = competitor.CityId,
                personality = EnumNames.ToDart(competitor.Personality),
                shopCount = competitor.ShopCount,
                reputation = competitor.Reputation,
                priceLevel = competitor.PriceLevel,
                marketShare = competitor.MarketShare,
                daysSinceLastAction = competitor.DaysSinceLastAction,
            };
        }

        private static Competitor FromDto(CompetitorDto dto)
        {
            return new Competitor
            {
                Id = dto.id,
                Name = dto.name,
                CityId = dto.cityId,
                Personality = EnumNames.CompetitorFromDart(dto.personality),
                ShopCount = dto.shopCount,
                Reputation = dto.reputation,
                PriceLevel = dto.priceLevel,
                MarketShare = dto.marketShare,
                DaysSinceLastAction = dto.daysSinceLastAction,
            };
        }

        private static LoanDto ToDto(Loan loan)
        {
            return new LoanDto
            {
                id = loan.Id,
                amount = loan.Amount,
                interestRate = loan.InterestRate,
                durationDays = loan.DurationDays,
                dayTaken = loan.DayTaken,
                amountPaid = loan.AmountPaid,
            };
        }

        private static Loan FromDto(LoanDto dto)
        {
            return new Loan
            {
                Id = dto.id,
                Amount = dto.amount,
                InterestRate = dto.interestRate,
                DurationDays = dto.durationDays,
                DayTaken = dto.dayTaken,
                AmountPaid = dto.amountPaid,
            };
        }

        private static BrandStatsDto ToDto(BrandStats brand)
        {
            brand ??= new BrandStats();
            return new BrandStatsDto
            {
                brandAwareness = brand.BrandAwareness,
                cityReputation = new Dictionary<string, double>(brand.CityReputation ?? new Dictionary<string, double>()),
            };
        }

        private static BrandStats FromDto(BrandStatsDto dto)
        {
            dto ??= new BrandStatsDto();
            return new BrandStats
            {
                BrandAwareness = dto.brandAwareness,
                CityReputation = new Dictionary<string, double>(dto.cityReputation ?? new Dictionary<string, double>()),
            };
        }

        private static ActiveCampaignDto ToDto(ActiveCampaign campaign)
        {
            return new ActiveCampaignDto
            {
                campaignId = campaign.CampaignId,
                startDay = campaign.StartDay,
                endDay = campaign.EndDay,
            };
        }

        private static ActiveCampaign FromDto(ActiveCampaignDto dto)
        {
            return new ActiveCampaign
            {
                CampaignId = dto.campaignId,
                StartDay = dto.startDay,
                EndDay = dto.endDay,
            };
        }

        private static List<TOut> MapList<TIn, TOut>(IEnumerable<TIn> source, System.Func<TIn, TOut> map)
        {
            List<TOut> result = new();
            if (source == null)
            {
                return result;
            }

            foreach (TIn item in source)
            {
                result.Add(map(item));
            }

            return result;
        }

        private sealed class GameStateDto
        {
            public string companyName;
            public string founderName;
            public double cash;
            public int currentDay = 1;
            public int currentHour;
            public List<ShopDto> shops = new();
            public List<string> unlockedCityIds = new();
            public List<CompetitorDto> competitors = new();
            public List<LoanDto> loans = new();
            public double totalRevenue;
            public double totalProfit;
            public int customersServedTotal;
            public string difficulty = "normal";
            public BrandStatsDto brand = new();
            public List<string> achievementIds = new();
            public List<EmployeeDto> employeePool = new();
            public int lastEmployeePoolDay;
            public List<string> managerEmployeeIds = new();
            public List<string> globalUpgradeIds = new();
            public string activeThemeId = "klassik";
            public int prestigePoints;
            public bool tutorialDone;
            public bool tutorialEnabled;
            public int tutorialStep;
            public List<string> seenEventIds = new();
        }

        private sealed class ShopDto
        {
            public string id;
            public string name;
            public string customName;
            public string cityId;
            public string locationName;
            public int footTraffic;
            public double weeklyRent;
            public bool isOpen = true;
            public List<ShopProductDto> menu = new();
            public List<ShopEquipmentDto> equipment = new();
            public List<EmployeeDto> employees = new();
            public double reputation = 3.0;
            public int dayOpened;
            public List<ActiveCampaignDto> activeCampaigns = new();
            public string personality = "touristic";
            public List<string> upgradeIds = new();
            public bool autoHire;
            public string originalCompetitorName;
            public bool wasAcquired;
            public double morale = 0.75;
            public double regulars;
            public string sizeTier = "klein";
        }

        private sealed class ShopProductDto
        {
            public string productId;
            public double price;
            public bool isActive = true;
        }

        private sealed class ShopEquipmentDto
        {
            public string equipmentId;
        }

        private sealed class EmployeeDto
        {
            public string id;
            public string typeId;
            public string name;
            public int speed;
            public int friendliness;
            public int reliability;
            public int experience;
            public double salaryPerDay;
            public List<string> traits = new();
            public int daysEmployed;
            public string origin = "regular";
            public double growthPotential;
            public string shift = "ganztags";
        }

        private sealed class CompetitorDto
        {
            public string id;
            public string name;
            public string cityId;
            public string personality = "balanced";
            public int shopCount = 1;
            public double reputation = 3.0;
            public double priceLevel = 1.0;
            public double marketShare = 0.15;
            public int daysSinceLastAction;
        }

        private sealed class LoanDto
        {
            public string id;
            public double amount;
            public double interestRate;
            public int durationDays;
            public int dayTaken;
            public double amountPaid;
        }

        private sealed class BrandStatsDto
        {
            public double brandAwareness = 5.0;
            public Dictionary<string, double> cityReputation = new();
        }

        private sealed class ActiveCampaignDto
        {
            public string campaignId;
            public int startDay;
            public int endDay;
        }
    }
}
