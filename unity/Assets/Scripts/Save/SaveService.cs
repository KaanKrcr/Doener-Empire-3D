using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Save
{
    public static class SaveService
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
        };

        public static string ToJson(GameState state)
        {
            return JsonSerializer.Serialize(ToDto(state), Options);
        }

        public static GameState FromJson(string json)
        {
            var dto = JsonSerializer.Deserialize<GameStateDto>(json, Options) ?? new GameStateDto();
            return FromDto(dto);
        }

        private static GameStateDto ToDto(GameState state) => new()
        {
            companyName = state.CompanyName,
            founderName = state.FounderName,
            cash = state.Cash,
            currentDay = state.CurrentDay,
            currentHour = state.CurrentHour,
            shops = state.Shops.Select(ToDto).ToList(),
            unlockedCityIds = new List<string>(state.UnlockedCityIds),
            competitors = state.Competitors.Select(ToDto).ToList(),
            loans = state.Loans.Select(ToDto).ToList(),
            totalRevenue = state.TotalRevenue,
            totalProfit = state.TotalProfit,
            customersServedTotal = state.CustomersServedTotal,
            difficulty = EnumNames.ToDart(state.Difficulty),
            brand = ToDto(state.Brand),
            achievementIds = new List<string>(state.AchievementIds),
            employeePool = state.EmployeePool.Select(ToDto).ToList(),
            lastEmployeePoolDay = state.LastEmployeePoolDay,
            managerEmployeeIds = new List<string>(state.ManagerEmployeeIds),
            globalUpgradeIds = new List<string>(state.GlobalUpgradeIds),
            activeThemeId = state.ActiveThemeId,
            prestigePoints = state.PrestigePoints,
            tutorialDone = state.TutorialDone,
            tutorialEnabled = state.TutorialEnabled,
            tutorialStep = state.TutorialStep,
            seenEventIds = new List<string>(state.SeenEventIds),
        };

        private static GameState FromDto(GameStateDto dto) => new()
        {
            CompanyName = dto.companyName,
            FounderName = dto.founderName,
            Cash = dto.cash,
            CurrentDay = dto.currentDay,
            CurrentHour = dto.currentHour,
            Shops = (dto.shops ?? new()).Select(FromDto).ToList(),
            UnlockedCityIds = dto.unlockedCityIds ?? new(),
            Competitors = (dto.competitors ?? new()).Select(FromDto).ToList(),
            Loans = (dto.loans ?? new()).Select(FromDto).ToList(),
            TotalRevenue = dto.totalRevenue,
            TotalProfit = dto.totalProfit,
            CustomersServedTotal = dto.customersServedTotal,
            Difficulty = EnumNames.DifficultyFromDart(dto.difficulty),
            Brand = FromDto(dto.brand),
            AchievementIds = dto.achievementIds ?? new(),
            EmployeePool = (dto.employeePool ?? new()).Select(FromDto).ToList(),
            LastEmployeePoolDay = dto.lastEmployeePoolDay,
            ManagerEmployeeIds = dto.managerEmployeeIds ?? new(),
            GlobalUpgradeIds = dto.globalUpgradeIds ?? new(),
            ActiveThemeId = dto.activeThemeId ?? "klassik",
            PrestigePoints = dto.prestigePoints,
            TutorialDone = dto.tutorialDone,
            TutorialEnabled = dto.tutorialEnabled,
            TutorialStep = dto.tutorialStep,
            SeenEventIds = dto.seenEventIds ?? new(),
        };

        private static ShopDto ToDto(Shop shop) => new()
        {
            id = shop.Id,
            name = shop.Name,
            customName = shop.CustomName,
            cityId = shop.CityId,
            locationName = shop.LocationName,
            footTraffic = shop.FootTraffic,
            weeklyRent = shop.WeeklyRent,
            isOpen = shop.IsOpen,
            menu = shop.Menu.Select(ToDto).ToList(),
            equipment = shop.Equipment.Select(ToDto).ToList(),
            employees = shop.Employees.Select(ToDto).ToList(),
            reputation = shop.Reputation,
            dayOpened = shop.DayOpened,
            activeCampaigns = shop.ActiveCampaigns.Select(ToDto).ToList(),
            personality = EnumNames.ToDart(shop.Personality),
            upgradeIds = new List<string>(shop.UpgradeIds),
            autoHire = shop.AutoHire,
            originalCompetitorName = shop.OriginalCompetitorName,
            wasAcquired = shop.WasAcquired,
            morale = shop.Morale,
            regulars = shop.Regulars,
            sizeTier = EnumNames.ToDart(shop.SizeTier),
        };

        private static Shop FromDto(ShopDto dto) => new()
        {
            Id = dto.id,
            Name = dto.name,
            CustomName = dto.customName,
            CityId = dto.cityId,
            LocationName = dto.locationName,
            FootTraffic = dto.footTraffic,
            WeeklyRent = dto.weeklyRent,
            IsOpen = dto.isOpen,
            Menu = (dto.menu ?? new()).Select(FromDto).ToList(),
            Equipment = (dto.equipment ?? new()).Select(FromDto).ToList(),
            Employees = (dto.employees ?? new()).Select(FromDto).ToList(),
            Reputation = dto.reputation,
            DayOpened = dto.dayOpened,
            ActiveCampaigns = (dto.activeCampaigns ?? new()).Select(FromDto).ToList(),
            Personality = EnumNames.LocationFromDart(dto.personality),
            UpgradeIds = dto.upgradeIds ?? new(),
            AutoHire = dto.autoHire,
            OriginalCompetitorName = dto.originalCompetitorName,
            WasAcquired = dto.wasAcquired,
            Morale = dto.morale,
            Regulars = dto.regulars,
            SizeTier = EnumNames.ShopSizeFromDart(dto.sizeTier),
        };

        private static ShopProductDto ToDto(ShopProduct product) => new()
        {
            productId = product.ProductId,
            price = product.Price,
            isActive = product.IsActive,
        };

        private static ShopProduct FromDto(ShopProductDto dto) => new()
        {
            ProductId = dto.productId,
            Price = dto.price,
            IsActive = dto.isActive,
        };

        private static ShopEquipmentDto ToDto(ShopEquipment equipment) => new()
        {
            equipmentId = equipment.EquipmentId,
        };

        private static ShopEquipment FromDto(ShopEquipmentDto dto) => new()
        {
            EquipmentId = dto.equipmentId,
        };

        private static EmployeeDto ToDto(Employee employee) => new()
        {
            id = employee.Id,
            typeId = employee.TypeId,
            name = employee.Name,
            speed = employee.Speed,
            friendliness = employee.Friendliness,
            reliability = employee.Reliability,
            experience = employee.Experience,
            salaryPerDay = employee.SalaryPerDay,
            traits = employee.Traits.Select(EmployeeEnumNames.ToDart).ToList(),
            daysEmployed = employee.DaysEmployed,
            origin = EmployeeEnumNames.ToDart(employee.Origin),
            growthPotential = employee.GrowthPotential,
            shift = EmployeeEnumNames.ToDart(employee.Shift),
        };

        private static Employee FromDto(EmployeeDto dto) => new()
        {
            Id = dto.id,
            TypeId = dto.typeId,
            Name = dto.name,
            Speed = dto.speed,
            Friendliness = dto.friendliness,
            Reliability = dto.reliability,
            Experience = dto.experience,
            SalaryPerDay = dto.salaryPerDay,
            Traits = (dto.traits ?? new())
                .Select(EmployeeEnumNames.TraitFromDart)
                .Where(t => t.HasValue)
                .Select(t => t.Value)
                .ToList(),
            DaysEmployed = dto.daysEmployed,
            Origin = EmployeeEnumNames.OriginFromDart(dto.origin),
            GrowthPotential = dto.growthPotential,
            Shift = EmployeeEnumNames.ShiftFromDart(dto.shift),
        };

        private static CompetitorDto ToDto(Competitor competitor) => new()
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

        private static Competitor FromDto(CompetitorDto dto) => new()
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

        private static LoanDto ToDto(Loan loan) => new()
        {
            id = loan.Id,
            amount = loan.Amount,
            interestRate = loan.InterestRate,
            durationDays = loan.DurationDays,
            dayTaken = loan.DayTaken,
            amountPaid = loan.AmountPaid,
        };

        private static Loan FromDto(LoanDto dto) => new()
        {
            Id = dto.id,
            Amount = dto.amount,
            InterestRate = dto.interestRate,
            DurationDays = dto.durationDays,
            DayTaken = dto.dayTaken,
            AmountPaid = dto.amountPaid,
        };

        private static ActiveCampaignDto ToDto(ActiveCampaign campaign) => new()
        {
            campaignId = campaign.CampaignId,
            startDay = campaign.StartDay,
            endDay = campaign.EndDay,
        };

        private static ActiveCampaign FromDto(ActiveCampaignDto dto) => new()
        {
            CampaignId = dto.campaignId,
            StartDay = dto.startDay,
            EndDay = dto.endDay,
        };

        private static BrandStatsDto ToDto(BrandStats brand) => new()
        {
            brandAwareness = brand.BrandAwareness,
            cityReputation = new Dictionary<string, double>(brand.CityReputation),
        };

        private static BrandStats FromDto(BrandStatsDto dto) => new()
        {
            BrandAwareness = dto?.brandAwareness ?? 5.0,
            CityReputation = dto?.cityReputation ?? new Dictionary<string, double>(),
        };

        private sealed class GameStateDto
        {
            public string companyName { get; set; }
            public string founderName { get; set; }
            public double cash { get; set; }
            public int currentDay { get; set; } = 1;
            public int currentHour { get; set; }
            public List<ShopDto> shops { get; set; } = new();
            public List<string> unlockedCityIds { get; set; } = new();
            public List<CompetitorDto> competitors { get; set; } = new();
            public List<LoanDto> loans { get; set; } = new();
            public double totalRevenue { get; set; }
            public double totalProfit { get; set; }
            public int customersServedTotal { get; set; }
            public string difficulty { get; set; } = "normal";
            public BrandStatsDto brand { get; set; } = new();
            public List<string> achievementIds { get; set; } = new();
            public List<EmployeeDto> employeePool { get; set; } = new();
            public int lastEmployeePoolDay { get; set; }
            public List<string> managerEmployeeIds { get; set; } = new();
            public List<string> globalUpgradeIds { get; set; } = new();
            public string activeThemeId { get; set; } = "klassik";
            public int prestigePoints { get; set; }
            public bool tutorialDone { get; set; }
            public bool tutorialEnabled { get; set; }
            public int tutorialStep { get; set; }
            public List<string> seenEventIds { get; set; } = new();
        }

        private sealed class ShopDto
        {
            public string id { get; set; }
            public string name { get; set; }
            public string customName { get; set; }
            public string cityId { get; set; }
            public string locationName { get; set; }
            public int footTraffic { get; set; }
            public double weeklyRent { get; set; }
            public bool isOpen { get; set; } = true;
            public List<ShopProductDto> menu { get; set; } = new();
            public List<ShopEquipmentDto> equipment { get; set; } = new();
            public List<EmployeeDto> employees { get; set; } = new();
            public double reputation { get; set; } = 3.0;
            public int dayOpened { get; set; }
            public List<ActiveCampaignDto> activeCampaigns { get; set; } = new();
            public string personality { get; set; } = "touristic";
            public List<string> upgradeIds { get; set; } = new();
            public bool autoHire { get; set; }
            public string originalCompetitorName { get; set; }
            public bool wasAcquired { get; set; }
            public double morale { get; set; } = 0.75;
            public double regulars { get; set; }
            public string sizeTier { get; set; } = "klein";
        }

        private sealed class ShopProductDto
        {
            public string productId { get; set; }
            public double price { get; set; }
            public bool isActive { get; set; } = true;
        }

        private sealed class ShopEquipmentDto
        {
            public string equipmentId { get; set; }
        }

        private sealed class EmployeeDto
        {
            public string id { get; set; }
            public string typeId { get; set; }
            public string name { get; set; }
            public int speed { get; set; }
            public int friendliness { get; set; }
            public int reliability { get; set; }
            public int experience { get; set; }
            public double salaryPerDay { get; set; }
            public List<string> traits { get; set; } = new();
            public int daysEmployed { get; set; }
            public string origin { get; set; } = "regular";
            public double growthPotential { get; set; }
            public string shift { get; set; } = "ganztags";
        }

        private sealed class CompetitorDto
        {
            public string id { get; set; }
            public string name { get; set; }
            public string cityId { get; set; }
            public string personality { get; set; } = "balanced";
            public int shopCount { get; set; } = 1;
            public double reputation { get; set; } = 3.0;
            public double priceLevel { get; set; } = 1.0;
            public double marketShare { get; set; } = 0.15;
            public int daysSinceLastAction { get; set; }
        }

        private sealed class LoanDto
        {
            public string id { get; set; }
            public double amount { get; set; }
            public double interestRate { get; set; }
            public int durationDays { get; set; }
            public int dayTaken { get; set; }
            public double amountPaid { get; set; }
        }

        private sealed class ActiveCampaignDto
        {
            public string campaignId { get; set; }
            public int startDay { get; set; }
            public int endDay { get; set; }
        }

        private sealed class BrandStatsDto
        {
            public double brandAwareness { get; set; } = 5.0;
            public Dictionary<string, double> cityReputation { get; set; } = new();
        }
    }
}
