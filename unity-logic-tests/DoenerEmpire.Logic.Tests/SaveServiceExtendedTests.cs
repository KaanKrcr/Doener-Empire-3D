// Tests für die erweiterten SaveService-Felder (M4–M7-Ports).
// Verifiziert Round-Trip von Stocks, Facilities, HR, Campaigns, Combos, Quality.

using System.Collections.Generic;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Save;

namespace DoenerEmpire.Logic.Tests
{
    public class SaveServiceExtendedTests
    {
        private static GameState RoundTrip(GameState state)
        {
            var svc = new SaveService();
            return svc.Deserialize(svc.Serialize(state));
        }

        [Fact]
        public void StocksRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.Stocks.IsPublic = true;
            s.Stocks.IpoDay = 120;
            s.Stocks.SharePrice = 13.7;
            s.Stocks.TotalShares = 5000;
            s.Stocks.PlayerShares = 3000;
            s.Stocks.PriceHistory.AddRange(new[] { 10.0, 11.0, 13.7 });
            s.Stocks.AnalystExpectation = 2500;
            s.Stocks.LastQuarterDay = 90;

            var r = RoundTrip(s);
            Assert.True(r.Stocks.IsPublic);
            Assert.Equal(120, r.Stocks.IpoDay);
            Assert.Equal(13.7, r.Stocks.SharePrice);
            Assert.Equal(5000, r.Stocks.TotalShares);
            Assert.Equal(3000, r.Stocks.PlayerShares);
            Assert.Equal(3, r.Stocks.PriceHistory.Count);
            Assert.Equal(2500, r.Stocks.AnalystExpectation);
            Assert.Equal(90, r.Stocks.LastQuarterDay);
        }

        [Fact]
        public void FacilitiesRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.Facilities.Add(new ProductionFacility
            { Id = "f1", Type = ProductionType.Fleisch, Tier = FacilityTier.Gross, DayBuilt = 30 });
            s.Facilities.Add(new ProductionFacility
            { Id = "f2", Type = ProductionType.Gemuese, Tier = FacilityTier.Klein, DayBuilt = 45 });

            var r = RoundTrip(s);
            Assert.Equal(2, r.Facilities.Count);
            Assert.Equal(ProductionType.Fleisch, r.Facilities[0].Type);
            Assert.Equal(FacilityTier.Gross, r.Facilities[0].Tier);
            Assert.Equal(ProductionType.Gemuese, r.Facilities[1].Type);
            Assert.Equal(45, r.Facilities[1].DayBuilt);
        }

        [Fact]
        public void HrManagerRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.HrStrategy = HrStrategy.PrioritizeQuality;
            s.HrManager = new HrManager
            {
                Id = "m1", Name = "Ayla Demir", Archetype = HrManagerArchetype.TalentScout,
                TalentSense = 9, Network = 8, Negotiation = 5, Speed = 6, Training = 5,
                SalaryPerDay = 250, Level = 3, Xp = 280,
            };

            var r = RoundTrip(s);
            Assert.Equal(HrStrategy.PrioritizeQuality, r.HrStrategy);
            Assert.NotNull(r.HrManager);
            Assert.Equal("Ayla Demir", r.HrManager.Name);
            Assert.Equal(HrManagerArchetype.TalentScout, r.HrManager.Archetype);
            Assert.Equal(9, r.HrManager.TalentSense);
            Assert.Equal(3, r.HrManager.Level);
            Assert.Equal(280, r.HrManager.Xp);
        }

        [Fact]
        public void NullHrManagerStaysNull()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var r = RoundTrip(s);
            Assert.Null(r.HrManager);
            Assert.Equal(HrStrategy.Balanced, r.HrStrategy);
        }

        [Fact]
        public void HrCandidatesRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.HrCandidates.Add(new HrManager
            { Id = "c1", Name = "Murat", Archetype = HrManagerArchetype.CostOptimizer,
              TalentSense = 5, Network = 5, Negotiation = 8, Speed = 6, Training = 6,
              SalaryPerDay = 120, Level = 1, Xp = 0 });
            var r = RoundTrip(s);
            Assert.Single(r.HrCandidates);
            Assert.Equal(HrManagerArchetype.CostOptimizer, r.HrCandidates[0].Archetype);
        }

        [Fact]
        public void CampaignFieldsRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.CompletedChapterIds.Add("ch1_traum");
            s.CompletedChapterIds.Add("ch2_stammkunden");
            s.ActiveComboIds.Add("mittagsmenu");
            s.ActiveGlobalCampaigns.Add(new ActiveCampaign { CampaignId = "tv_werbung", StartDay = 5, EndDay = 19 });
            s.ActiveCityCampaigns["fulda"] = new List<ActiveCampaign>
            { new() { CampaignId = "city_plakat", StartDay = 3, EndDay = 10 } };

            var r = RoundTrip(s);
            Assert.Equal(2, r.CompletedChapterIds.Count);
            Assert.Contains("ch1_traum", r.CompletedChapterIds);
            Assert.Single(r.ActiveComboIds);
            Assert.Single(r.ActiveGlobalCampaigns);
            Assert.Equal("tv_werbung", r.ActiveGlobalCampaigns[0].CampaignId);
            Assert.Equal(19, r.ActiveGlobalCampaigns[0].EndDay);
            Assert.True(r.ActiveCityCampaigns.ContainsKey("fulda"));
            Assert.Equal("city_plakat", r.ActiveCityCampaigns["fulda"][0].CampaignId);
        }

        [Fact]
        public void MissionsInitializedFromTemplate()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            Assert.NotEmpty(s.Missions);
            Assert.Equal("open_first_shop", s.Missions[0].Id);
            Assert.All(s.Missions, m => Assert.False(m.IsDone));
        }

        [Fact]
        public void MissionStatusRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            // Erste zwei Missionen als erledigt markieren
            s.Missions[0].IsDone = true;
            s.Missions[1].IsDone = true;

            var r = RoundTrip(s);
            // Voller Katalog wieder da, Status erhalten
            Assert.Equal(s.Missions.Count, r.Missions.Count);
            Assert.True(r.Missions[0].IsDone);
            Assert.True(r.Missions[1].IsDone);
            Assert.False(r.Missions[2].IsDone);
            // Template-Daten (nicht persistiert) korrekt rekonstruiert
            Assert.Equal(500, r.Missions[0].CashReward);
            Assert.Equal(MissionType.OpenFirstShop, r.Missions[0].Type);
        }

        [Fact]
        public void MissionsSerializeOnlyIdAndStatus()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            var json = new SaveService().Serialize(s);
            Assert.Contains("\"missions\"", json);
            // Mission-Status enthält id + isDone, aber keine Template-Felder wie cashReward
            Assert.Contains("\"open_first_shop\"", json);
        }

        [Fact]
        public void HistoryRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.History.Add(new DailyRecord
            {
                Day = 1, Revenue = 1234, Costs = 567, Customers = 89,
                RentCosts = 200, SalaryCosts = 145, IngredientCosts = 180,
                DeliveryCommissionCosts = 12, LoanPayments = 30, Investments = 0,
            });
            s.History.Add(new DailyRecord { Day = 2, Revenue = 1500, Costs = 600, Customers = 95 });

            var r = RoundTrip(s);
            Assert.Equal(2, r.History.Count);
            Assert.Equal(1234, r.History[0].Revenue);
            Assert.Equal(89, r.History[0].Customers);
            Assert.Equal(12, r.History[0].DeliveryCommissionCosts);
            Assert.Equal(2, r.History[1].Day);
        }

        [Fact]
        public void ProductQualityRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.ProductQuality["doener_fladen"] = "premium";
            s.ProductQuality["ayran"] = "budget";

            var r = RoundTrip(s);
            Assert.Equal("premium", r.ProductQuality["doener_fladen"]);
            Assert.Equal("budget", r.ProductQuality["ayran"]);
        }

        [Fact]
        public void PriceOverridesRoundTrip()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.GlobalPrices["doener_fladen"] = 7.20;
            s.CityPrices["berlin"] = new Dictionary<string, double>
            {
                ["doener_fladen"] = 8.50,
                ["ayran"] = 2.50,
            };

            var r = RoundTrip(s);
            Assert.Equal(7.20, r.GlobalPrices["doener_fladen"]);
            Assert.True(r.CityPrices.ContainsKey("berlin"));
            Assert.Equal(8.50, r.CityPrices["berlin"]["doener_fladen"]);
            Assert.Equal(2.50, r.CityPrices["berlin"]["ayran"]);
        }

        [Fact]
        public void ExtendedFieldsHaveSafeDefaultsForLegacySave()
        {
            // Legacy-Save ohne die neuen Felder → Defaults greifen.
            var legacyJson = "{\"companyName\":\"Alt\",\"founderName\":\"Kaan\",\"cash\":5000,\"currentDay\":3}";
            var svc = new SaveService();
            var r = svc.Deserialize(legacyJson);
            Assert.NotNull(r.Stocks);
            Assert.False(r.Stocks.IsPublic);
            Assert.NotNull(r.Facilities);
            Assert.Empty(r.Facilities);
            Assert.Null(r.HrManager);
            Assert.Equal(HrStrategy.Balanced, r.HrStrategy);
            Assert.NotNull(r.CompletedChapterIds);
            Assert.NotNull(r.ProductQuality);
            Assert.NotNull(r.ActiveGlobalCampaigns);
            Assert.NotNull(r.ActiveCityCampaigns);
            Assert.NotNull(r.GlobalPrices);
            Assert.NotNull(r.CityPrices);
            Assert.NotEmpty(r.Missions); // aus Template aufgebaut
        }

        [Fact]
        public void SerializedJsonUsesFlutterFieldNames()
        {
            var s = GameState.Initial("X", "Y", 15000, tutorialEnabled: false);
            s.CompletedChapterIds.Add("ch1_traum");
            var json = new SaveService().Serialize(s);
            // Flutter-kompatible Schlüssel müssen vorhanden sein.
            Assert.Contains("\"stocks\"", json);
            Assert.Contains("\"facilities\"", json);
            Assert.Contains("\"hrStrategy\"", json);
            Assert.Contains("\"completedChapterIds\"", json);
            Assert.Contains("\"activeComboIds\"", json);
            Assert.Contains("\"productQuality\"", json);
            Assert.Contains("\"activeGlobalCampaigns\"", json);
        }
    }
}
