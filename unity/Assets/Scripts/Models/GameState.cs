// Döner Empire 3D — zentraler Spielzustand (MVP-Umfang)
// Port aus lib/models/game_state.dart (GameState).
//
// MVP-fokussiert: enthält die Felder, die der Vertical Slice (City Map,
// Standortauswahl, Verwaltung, Tagessimulation, Bericht) und die bereits
// portierten Systeme brauchen. Endgame-Felder, deren Typen noch nicht portiert
// sind, sind unten als TODO gelistet und folgen später.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;

namespace DoenerEmpire.Models
{
    public sealed class GameState
    {
        // ── Identität / Kern ─────────────────────────────────────────────────
        public string CompanyName;
        public string FounderName;
        public double Cash;
        public int CurrentDay = 1;
        public int CurrentHour = 0;            // 0..14 Tages-Tick

        // ── Filialen & Welt ──────────────────────────────────────────────────
        public List<Shop> Shops = new();
        public List<string> UnlockedCityIds = new();
        public List<Competitor> Competitors = new();
        public List<Loan> Loans = new();

        // ── Wirtschaft / Verlauf ─────────────────────────────────────────────
        public double TotalRevenue = 0;
        public double TotalProfit = 0;
        public List<DailyRecord> History = new();
        public int CustomersServedTotal = 0;

        // ── Meta / Marke ─────────────────────────────────────────────────────
        public GameDifficulty Difficulty = GameDifficulty.Normal;
        public BrandStats Brand = new();
        public List<string> AchievementIds = new();
        public List<Employee> EmployeePool = new();
        public int LastEmployeePoolDay = 0;
        public List<string> ManagerEmployeeIds = new();
        public List<string> GlobalUpgradeIds = new();
        public string ActiveThemeId = "klassik";
        public int PrestigePoints = 0;

        // ── Tutorial / Events ────────────────────────────────────────────────
        public bool TutorialDone = false;
        public bool TutorialEnabled = false;
        public int TutorialStep = 0;
        public List<string> SeenEventIds = new();

        // ── Missionen (Status; aus Template initialisiert) ──────────────────
        public List<Mission> Missions = new();

        // ── Story-Kampagne (M4c-Port) ───────────────────────────────────────
        public List<string> CompletedChapterIds = new();

        // ── HR-System (M4e-Port) ────────────────────────────────────────────
        public HrManager HrManager;                    // null = kein Manager
        public HrStrategy HrStrategy = HrStrategy.Balanced;
        public List<HrManager> HrCandidates = new();   // verfügbare Manager-Kandidaten

        // ── Börse/Aktien (M6-Foundation) ────────────────────────────────────
        public StockState Stocks = new();

        // ── Produktionsanlagen (M7b) ────────────────────────────────────────
        public List<ProductionFacility> Facilities = new();

        // ── Combos & Quality (M5b-Foundation) ───────────────────────────────
        public List<string> ActiveComboIds = new();
        /// <summary>productId → IngredientQuality (Dart-Name: budget/standard/premium).</summary>
        public Dictionary<string, string> ProductQuality = new();

        // ── Marketing-Kampagnen (M6) ────────────────────────────────────────
        /// <summary>cityId → laufende stadtweite Kampagnen.</summary>
        public Dictionary<string, List<ActiveCampaign>> ActiveCityCampaigns = new();
        public List<ActiveCampaign> ActiveGlobalCampaigns = new();

        // ── Preis-Overrides (konzern- / stadtweit) ──────────────────────────
        /// <summary>productId → globaler Preis-Override (für neue Filialen).</summary>
        public Dictionary<string, double> GlobalPrices = new();
        /// <summary>cityId → (productId → Preis-Override).</summary>
        public Dictionary<string, Dictionary<string, double>> CityPrices = new();

        // TODO(Port): Endgame-Felder, sobald ihre Typen portiert sind —
        //   missions (Mission), stocks (StockState), facilities (ProductionFacility),
        //   hrManager (HrManager), hrStrategy (HrStrategy), hrCandidates,
        //   globalPrices/cityPrices, activeCity/GlobalCampaigns, completedChapterIds,
        //   activeComboIds, productQuality, supplyContract*.

        // ── Abgeleitete Werte ────────────────────────────────────────────────
        public int ShopCount => Shops.Count;

        public int EmployeeCount => Shops.Sum(s => s.Employees.Count);

        public double ActiveLoansTotal =>
            Loans.Where(l => !l.IsPaidOff).Sum(l => l.RemainingDebt);

        public IReadOnlyList<Competitor> CompetitorsIn(string cityId) =>
            Competitors.Where(c => c.CityId == cityId).ToList();

        public bool HasShopIn(string cityId) => Shops.Any(s => s.CityId == cityId);

        public DifficultyModifiers Modifiers => DifficultyData.Get(Difficulty);

        // ── Factory: neuer Spielstand ────────────────────────────────────────
        public static GameState Initial(
            string companyName,
            string founderName,
            double startCash,
            GameDifficulty difficulty = GameDifficulty.Normal,
            bool tutorialEnabled = true,
            int prestigePoints = 0)
        {
            return new GameState
            {
                CompanyName = companyName,
                FounderName = founderName,
                Cash = startCash,
                CurrentDay = 1,
                CurrentHour = 0,
                Shops = new List<Shop>(),
                UnlockedCityIds = new List<string> { "fulda", "bayreuth", "goettingen" },
                Loans = new List<Loan>(),
                TotalRevenue = 0,
                TotalProfit = 0,
                History = new List<DailyRecord>(),
                Competitors = new List<Competitor>(),
                Difficulty = difficulty,
                Brand = new BrandStats { BrandAwareness = 5.0 },
                AchievementIds = new List<string>(),
                EmployeePool = new List<Employee>(),
                LastEmployeePoolDay = 0,
                ManagerEmployeeIds = new List<string>(),
                GlobalUpgradeIds = new List<string>(),
                ActiveThemeId = "klassik",
                PrestigePoints = prestigePoints,
                TutorialDone = false,
                TutorialEnabled = tutorialEnabled,
                TutorialStep = 0,
                SeenEventIds = new List<string>(),
                Missions = MissionTemplates.Build(),
                CompletedChapterIds = new List<string>(),
                HrManager = null,
                HrStrategy = HrStrategy.Balanced,
                HrCandidates = new List<HrManager>(),
                Stocks = new StockState(),
                Facilities = new List<ProductionFacility>(),
                ActiveComboIds = new List<string>(),
                ProductQuality = new Dictionary<string, string>(),
                ActiveCityCampaigns = new Dictionary<string, List<ActiveCampaign>>(),
                ActiveGlobalCampaigns = new List<ActiveCampaign>(),
                GlobalPrices = new Dictionary<string, double>(),
                CityPrices = new Dictionary<string, Dictionary<string, double>>(),
            };
        }

        public GameState Clone() => new()
        {
            CompanyName = CompanyName, FounderName = FounderName,
            Cash = Cash, CurrentDay = CurrentDay, CurrentHour = CurrentHour,
            Shops = Shops.Select(s => s.Clone()).ToList(),
            UnlockedCityIds = new List<string>(UnlockedCityIds),
            Competitors = Competitors.Select(c => new Competitor
            {
                Id = c.Id, Name = c.Name, CityId = c.CityId, Personality = c.Personality,
                ShopCount = c.ShopCount, Reputation = c.Reputation, PriceLevel = c.PriceLevel,
                MarketShare = c.MarketShare, DaysSinceLastAction = c.DaysSinceLastAction,
            }).ToList(),
            Loans = Loans.Select(l => l.Clone()).ToList(),
            TotalRevenue = TotalRevenue, TotalProfit = TotalProfit,
            History = History.Select(h => new DailyRecord
            {
                Day = h.Day, Revenue = h.Revenue, Costs = h.Costs, Customers = h.Customers,
                RentCosts = h.RentCosts, SalaryCosts = h.SalaryCosts,
                IngredientCosts = h.IngredientCosts,
                DeliveryCommissionCosts = h.DeliveryCommissionCosts,
                LoanPayments = h.LoanPayments, Investments = h.Investments,
            }).ToList(),
            CustomersServedTotal = CustomersServedTotal,
            Difficulty = Difficulty,
            Brand = Brand.Clone(),
            AchievementIds = new List<string>(AchievementIds),
            EmployeePool = new List<Employee>(EmployeePool),
            LastEmployeePoolDay = LastEmployeePoolDay,
            ManagerEmployeeIds = new List<string>(ManagerEmployeeIds),
            GlobalUpgradeIds = new List<string>(GlobalUpgradeIds),
            ActiveThemeId = ActiveThemeId,
            PrestigePoints = PrestigePoints,
            TutorialDone = TutorialDone, TutorialEnabled = TutorialEnabled,
            TutorialStep = TutorialStep,
            SeenEventIds = new List<string>(SeenEventIds),
            Missions = Missions.Select(m => new Mission
            {
                Id = m.Id, Title = m.Title, Description = m.Description, Emoji = m.Emoji,
                CashReward = m.CashReward, Type = m.Type, Target = m.Target, IsDone = m.IsDone,
            }).ToList(),
            CompletedChapterIds = new List<string>(CompletedChapterIds),
            HrManager = HrManager?.Clone(),
            HrStrategy = HrStrategy,
            HrCandidates = HrCandidates.Select(h => h.Clone()).ToList(),
            Stocks = Stocks?.Clone() ?? new StockState(),
            Facilities = Facilities.Select(f => f.Clone()).ToList(),
            ActiveComboIds = new List<string>(ActiveComboIds),
            ProductQuality = new Dictionary<string, string>(ProductQuality),
            ActiveCityCampaigns = ActiveCityCampaigns.ToDictionary(
                kv => kv.Key, kv => kv.Value.Select(c => c.Clone()).ToList()),
            ActiveGlobalCampaigns = ActiveGlobalCampaigns.Select(c => c.Clone()).ToList(),
            GlobalPrices = new Dictionary<string, double>(GlobalPrices),
            CityPrices = CityPrices.ToDictionary(
                kv => kv.Key, kv => new Dictionary<string, double>(kv.Value)),
        };
    }
}
