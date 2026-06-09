using System.IO;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Save;

namespace DoenerEmpire.Logic.Tests
{
    /// <summary>
    /// Verifiziert, dass der C# SaveService eine echte Flutter-Save-Struktur
    /// laden kann, ohne zu crashen, und dass die MVP-Kernfelder korrekt
    /// gemappt werden (Risiko R6 aus dem 2026-06-06 Review).
    ///
    /// Die Fixture liegt unter Fixtures/flutter_save_mvp.json und repräsentiert
    /// einen typischen Spielstand: 1 Shop mit Menü/Equipment/Mitarbeiter,
    /// 1 Konkurrent, 1 Kredit, Brand, Cities, Upgrades, Difficulty=normal.
    ///
    /// Die Fixture enthält bewusst auch Felder, die das C# DTO (noch) nicht
    /// kennt — history, missions, stocks, facilities, hrStrategy, hrCandidates,
    /// globalPrices, cityPrices, activeCityCampaigns, activeGlobalCampaigns,
    /// completedChapterIds, activeComboIds, productQuality. Diese sollen
    /// IGNORIERT werden (kein Crash, kein Datenverlust für MVP-Felder).
    /// </summary>
    public class FlutterSaveCompatibilityTests
    {
        private static string FixturePath =>
            Path.Combine(Path.GetDirectoryName(typeof(FlutterSaveCompatibilityTests).Assembly.Location)!,
                         "Fixtures", "flutter_save_mvp.json");

        private static string LoadFixture() => File.ReadAllText(FixturePath);

        [Fact]
        public void FlutterFixture_LoadsWithoutCrash()
        {
            string json = LoadFixture();
            var service = new SaveService();

            GameState state = service.Deserialize(json);

            Assert.NotNull(state);
        }

        [Fact]
        public void FlutterFixture_TopLevelScalarsAreMappedCorrectly()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Equal("Döner Empire", state.CompanyName);
            Assert.Equal("Kaan", state.FounderName);
            Assert.Equal(8742.5, state.Cash);
            Assert.Equal(18, state.CurrentDay);
            Assert.Equal(0, state.CurrentHour);
            Assert.Equal(42150.0, state.TotalRevenue);
            Assert.Equal(7890.0, state.TotalProfit);
            Assert.Equal(4128, state.CustomersServedTotal);
            Assert.True(state.TutorialDone);
            Assert.False(state.TutorialEnabled);
            Assert.Equal(99, state.TutorialStep);
            Assert.Equal(GameDifficulty.Normal, state.Difficulty);
            Assert.Equal("klassik", state.ActiveThemeId);
        }

        [Fact]
        public void FlutterFixture_UnlockedCitiesAreLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Equal(2, state.UnlockedCityIds.Count);
            Assert.Contains("fulda", state.UnlockedCityIds);
            Assert.Contains("kassel", state.UnlockedCityIds);
        }

        [Fact]
        public void FlutterFixture_GlobalUpgradesAndAchievementsAreLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Contains("lieferdienst", state.GlobalUpgradeIds);
            Assert.Contains("first_shop", state.AchievementIds);
            Assert.Contains("first_employee", state.AchievementIds);
            Assert.Contains("intro", state.SeenEventIds);
            Assert.Contains("first_competitor", state.SeenEventIds);
        }

        [Fact]
        public void FlutterFixture_BrandStatsAreLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Equal(14.5, state.Brand.BrandAwareness);
            Assert.Equal(62.0, state.Brand.CityReputation["fulda"]);
            Assert.Equal(22.0, state.Brand.CityReputation["kassel"]);
        }

        [Fact]
        public void FlutterFixture_ShopWithMenuEquipmentEmployeesIsLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Single(state.Shops);
            Shop shop = state.Shops[0];

            Assert.Equal("shop_fulda_hauptstrasse", shop.Id);
            Assert.Equal("Döner Empire", shop.Name);
            Assert.Equal("Hauptstrasse 12", shop.CustomName);
            Assert.Equal("fulda", shop.CityId);
            Assert.Equal("Hauptstrasse", shop.LocationName);
            Assert.Equal(1842, shop.FootTraffic);
            Assert.Equal(8750.0, shop.WeeklyRent);
            Assert.True(shop.IsOpen);
            Assert.Equal(4.2, shop.Reputation);
            Assert.Equal(1, shop.DayOpened);
            Assert.Equal(LocationPersonality.Business, shop.Personality);
            Assert.Contains("led_sign", shop.UpgradeIds);
            Assert.False(shop.AutoHire);
            Assert.False(shop.WasAcquired);

            Assert.Equal(4, shop.Menu.Count);
            Assert.Equal("doener_fladen", shop.Menu[0].ProductId);
            Assert.Equal(6.5, shop.Menu[0].Price);
            Assert.True(shop.Menu[0].IsActive);
            Assert.False(shop.Menu[3].IsActive); // pommes ist deaktiviert

            Assert.Equal(2, shop.Equipment.Count);
            Assert.Equal("spiess_klein", shop.Equipment[0].EquipmentId);

            Assert.Single(shop.Employees);
            Employee emp = shop.Employees[0];
            Assert.Equal("emp_1", emp.Id);
            Assert.Equal("Ali", emp.Name);
            Assert.Equal(7, emp.Speed);
            Assert.Equal(95.0, emp.SalaryPerDay);
            Assert.Equal(CandidateOrigin.HiddenGem, emp.Origin);
            Assert.Equal(2, emp.Traits.Count);
            Assert.Contains(PersonalityTrait.Charmer, emp.Traits);
            Assert.Contains(PersonalityTrait.Workaholic, emp.Traits);

            Assert.Single(shop.ActiveCampaigns);
            Assert.Equal("flyer", shop.ActiveCampaigns[0].CampaignId);
            Assert.Equal(14, shop.ActiveCampaigns[0].StartDay);
        }

        [Fact]
        public void FlutterFixture_ShopFlutterMissingFieldsGetDefaults()
        {
            // Flutter Shop.toJson() schreibt KEIN morale, regulars, sizeTier.
            // C# muss sinnvolle Defaults vergeben statt zu crashen.
            GameState state = new SaveService().Deserialize(LoadFixture());
            Shop shop = state.Shops[0];

            // ShopDto-Defaults: morale=0.75, regulars=0, sizeTier="klein" → Klein
            Assert.Equal(0.75, shop.Morale);
            Assert.Equal(0.0, shop.Regulars);
            Assert.Equal(ShopSizeTier.Klein, shop.SizeTier);
        }

        [Fact]
        public void FlutterFixture_EmployeeFlutterMissingShiftGetsDefault()
        {
            // Flutter Employee.toJson() schreibt KEIN shift.
            // C# muss Default ("ganztags" → Ganztags) vergeben.
            GameState state = new SaveService().Deserialize(LoadFixture());
            Employee emp = state.Shops[0].Employees[0];

            Assert.Equal(EmployeeShift.Ganztags, emp.Shift);
        }

        [Fact]
        public void FlutterFixture_CompetitorIsLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Single(state.Competitors);
            Competitor c = state.Competitors[0];
            Assert.Equal("comp_1", c.Id);
            Assert.Equal("Mehmets Döner", c.Name);
            Assert.Equal("fulda", c.CityId);
            Assert.Equal(CompetitorPersonality.CheapMass, c.Personality);
            Assert.Equal(2, c.ShopCount);
            Assert.Equal(3.6, c.Reputation);
            Assert.Equal(0.85, c.PriceLevel);
            Assert.Equal(0.28, c.MarketShare);
            Assert.Equal(4, c.DaysSinceLastAction);
        }

        [Fact]
        public void FlutterFixture_LoanIsLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Single(state.Loans);
            Loan l = state.Loans[0];
            Assert.Equal("loan_1", l.Id);
            Assert.Equal(5000.0, l.Amount);
            Assert.Equal(0.08, l.InterestRate);
            Assert.Equal(30, l.DurationDays);
            Assert.Equal(5, l.DayTaken);
            Assert.Equal(1200.0, l.AmountPaid);
        }

        [Fact]
        public void FlutterFixture_EmployeePoolIsLoaded()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            Assert.Single(state.EmployeePool);
            Assert.Equal("Lisa", state.EmployeePool[0].Name);
            Assert.Equal(CandidateOrigin.Regular, state.EmployeePool[0].Origin);
            Assert.Equal(17, state.LastEmployeePoolDay);
        }

        /// <summary>
        /// Stand 2026-06-09: Die zuvor offenen Felder werden jetzt vom C#-DTO
        /// geladen (SaveService erweitert). Dieser Test verifiziert, dass die
        /// Fixture-Werte korrekt ankommen bzw. sichere Defaults greifen.
        /// </summary>
        [Fact]
        public void FlutterFixture_ExtendedFieldsNowLoad_2026_06_09()
        {
            GameState state = new SaveService().Deserialize(LoadFixture());

            // History wird jetzt geladen (Fixture: 1 Eintrag, Tag 17).
            Assert.NotEmpty(state.History);
            Assert.Equal(17, state.History[0].Day);
            Assert.Equal(1820.0, state.History[0].Revenue);
            Assert.Equal(248, state.History[0].Customers);

            // Leere/null-Felder der Fixture → sichere Defaults.
            Assert.NotNull(state.Stocks);
            Assert.False(state.Stocks.IsPublic);
            Assert.Empty(state.Facilities);
            Assert.Null(state.HrManager);
            Assert.Equal(HrStrategy.Balanced, state.HrStrategy);
            Assert.Empty(state.HrCandidates);
            Assert.Empty(state.CompletedChapterIds);
            Assert.Empty(state.ActiveComboIds);
            Assert.Empty(state.ProductQuality);
            Assert.Empty(state.ActiveCityCampaigns);
            Assert.Empty(state.ActiveGlobalCampaigns);

            // Verbleibende, noch nicht in C#-GameState modellierte Felder
            // (missions, globalPrices, cityPrices) werden weiterhin ignoriert
            // — kein Crash bis hierher = OK.
        }
    }
}
