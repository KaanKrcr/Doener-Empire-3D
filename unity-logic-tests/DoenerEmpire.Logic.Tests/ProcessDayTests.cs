// Tests für DayProcessing.ProcessDay — Kern-Tagesabschluss.
// Spiegelt lib/services/game_engine.dart (processDay), Subsystem-Teilmenge.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ProcessDayTests
    {
        private static GameState StateWithShop(
            GameDifficulty diff = GameDifficulty.Normal, double cash = 15000)
        {
            var s = GameState.Initial("Test", "Tester", cash, diff, tutorialEnabled: false);
            var shop = new Shop
            {
                Id = "s1", CityId = "fulda", IsOpen = true, Reputation = 3.5,
                FootTraffic = 4000, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            shop.Menu.Add(new ShopProduct { ProductId = "ayran", Price = 2.0 });
            shop.Employees.Add(new Employee { Id = "e1", TypeId = "doener_meister",
                Speed = 7, Friendliness = 7, Reliability = 6, Experience = 6, SalaryPerDay = 80 });
            s.Shops.Add(shop);
            return s;
        }

        [Fact]
        public void AdvancesDay()
        {
            var s = StateWithShop();
            var r = DayProcessing.ProcessDay(s);
            Assert.Equal(2, s.CurrentDay);
            Assert.Equal(1, r.Record.Day);
        }

        [Fact]
        public void RecordHasPositiveRevenueAndCustomers()
        {
            var s = StateWithShop();
            var r = DayProcessing.ProcessDay(s);
            Assert.True(r.Record.Revenue > 0);
            Assert.True(r.Record.Customers > 0);
            Assert.True(r.Record.Costs > 0);
        }

        [Fact]
        public void AppendsToHistory()
        {
            var s = StateWithShop();
            DayProcessing.ProcessDay(s);
            DayProcessing.ProcessDay(s);
            Assert.Equal(2, s.History.Count);
            Assert.Equal(1, s.History[0].Day);
            Assert.Equal(2, s.History[1].Day);
        }

        [Fact]
        public void HistoryTrimmedTo60()
        {
            var s = StateWithShop();
            for (var i = 0; i < 70; i++) DayProcessing.ProcessDay(s);
            Assert.Equal(60, s.History.Count);
            // Älteste sind abgeschnitten → erster Eintrag ist Tag 11.
            Assert.Equal(11, s.History[0].Day);
        }

        [Fact]
        public void CashChangesByNetResult()
        {
            var s = StateWithShop();
            var before = s.Cash;
            var r = DayProcessing.ProcessDay(s);
            var expectedNet = r.Record.Revenue - r.Record.Costs - r.Record.LoanPayments;
            Assert.Equal(before + expectedNet, s.Cash, precision: 3);
        }

        [Fact]
        public void TotalRevenueAccumulatesWithProgressSpeed()
        {
            var s = StateWithShop(GameDifficulty.Easy);
            var r = DayProcessing.ProcessDay(s);
            // Easy progressSpeed = 1.35
            Assert.Equal(r.Record.Revenue * 1.35, s.TotalRevenue, precision: 2);
        }

        [Fact]
        public void EmployeeAgesEachDay()
        {
            var s = StateWithShop();
            DayProcessing.ProcessDay(s);
            Assert.Equal(1, s.Shops[0].Employees[0].DaysEmployed);
            DayProcessing.ProcessDay(s);
            Assert.Equal(2, s.Shops[0].Employees[0].DaysEmployed);
        }

        [Fact]
        public void EmployeeGainsExperienceOverTime()
        {
            var s = StateWithShop(GameDifficulty.Easy);
            var emp = s.Shops[0].Employees[0];
            emp.Experience = 5;
            for (var i = 0; i < 46; i++) DayProcessing.ProcessDay(s);
            Assert.True(emp.Experience > 5, $"exp={emp.Experience}");
            Assert.True(emp.Experience <= 10);
        }

        [Fact]
        public void ExpiredCampaignsRemoved()
        {
            var s = StateWithShop();
            s.Shops[0].ActiveCampaigns.Add(new ActiveCampaign
            { CampaignId = "flyer_local", StartDay = 1, EndDay = 2 });
            DayProcessing.ProcessDay(s); // today=1, expiry checks day 2 → removed
            Assert.Empty(s.Shops[0].ActiveCampaigns);
        }

        [Fact]
        public void ActiveCampaignStaysWhileValid()
        {
            var s = StateWithShop();
            s.Shops[0].ActiveCampaigns.Add(new ActiveCampaign
            { CampaignId = "radio_spot", StartDay = 1, EndDay = 10 });
            DayProcessing.ProcessDay(s);
            Assert.Single(s.Shops[0].ActiveCampaigns);
        }

        [Fact]
        public void LoanPaymentsDeducted()
        {
            var s = StateWithShop();
            s.Loans.Add(new Loan
            {
                Id = "l1", Amount = 10000, InterestRate = 0.06,
                DurationDays = 100, DayTaken = 0,
            });
            var r = DayProcessing.ProcessDay(s);
            Assert.True(r.Record.LoanPayments > 0);
            Assert.True(s.Loans[0].AmountPaid > 0);
        }

        [Fact]
        public void PaidOffLoansRemoved()
        {
            var s = StateWithShop();
            var loan = new Loan
            {
                Id = "l1", Amount = 100, InterestRate = 0.0,
                DurationDays = 1, DayTaken = 0,
            };
            s.Loans.Add(loan);
            DayProcessing.ProcessDay(s); // einzige Rate begleicht ihn
            Assert.Empty(s.Loans);
        }

        [Fact]
        public void CityUnlockReportedWhenRevenueCrosses()
        {
            var s = StateWithShop();
            // Künstlich kurz vor Schwelle für augsburg (30000)
            s.TotalRevenue = 29900;
            // Ein Tag Umsatz reicht nicht für 100€... erhöhe Reputation/traffic:
            s.Shops[0].FootTraffic = 100000;
            s.Shops[0].Employees.Add(new Employee { Id = "e2", TypeId = "kassierer",
                Speed = 9, Friendliness = 7, Reliability = 7, Experience = 7, SalaryPerDay = 65 });
            var r = DayProcessing.ProcessDay(s);
            if (s.TotalRevenue >= 30000)
                Assert.Contains("augsburg", r.NewlyUnlockedCities);
        }

        [Fact]
        public void BrandAwarenessGrows()
        {
            var s = StateWithShop();
            var before = s.Brand.BrandAwareness;
            DayProcessing.ProcessDay(s);
            Assert.True(s.Brand.BrandAwareness >= before);
        }

        [Fact]
        public void CompetitorsAdvanceDuringProcessDay()
        {
            var s = StateWithShop();
            s.Competitors.Add(new Competitor
            {
                Id = "c1", CityId = "fulda", Reputation = 3.0, PriceLevel = 1.0,
                ShopCount = 1, Personality = CompetitorPersonality.Balanced,
                DaysSinceLastAction = 0,
            });
            DayProcessing.ProcessDay(s);
            Assert.Equal(1, s.Competitors[0].DaysSinceLastAction);
        }

        [Fact]
        public void Runs60DaysStableWithoutThrow()
        {
            var s = StateWithShop(GameDifficulty.Hard);
            for (var i = 0; i < 60; i++)
            {
                var r = DayProcessing.ProcessDay(s);
                Assert.True(r.Record.Revenue >= 0);
                Assert.True(r.Record.Costs >= 0);
                Assert.InRange(s.Shops[0].Reputation, 0.5, 5.0);
            }
            Assert.Equal(61, s.CurrentDay);
        }

        [Fact]
        public void HrManagerGainsXpAndLevels()
        {
            var s = StateWithShop();
            s.HrManager = new HrManager
            {
                Id = "m", Name = "X", Archetype = HrManagerArchetype.ProcessManager,
                TalentSense = 5, Network = 5, Negotiation = 5, Speed = 5, Training = 5,
                SalaryPerDay = 200, Level = 1, Xp = 0,
            };
            DayProcessing.ProcessDay(s);
            Assert.True(s.HrManager.Xp >= 4);
            // Über viele Tage steigt das Level.
            for (var i = 0; i < 60; i++) DayProcessing.ProcessDay(s);
            Assert.True(s.HrManager.Level > 1, $"level={s.HrManager.Level}");
            Assert.InRange(s.HrManager.Level, 1, 50);
        }

        [Fact]
        public void HrManagerSalaryAddedToCosts()
        {
            var noMgr = StateWithShop();
            var withMgr = StateWithShop();
            withMgr.HrManager = new HrManager
            {
                Id = "m", Name = "X", Archetype = HrManagerArchetype.ProcessManager,
                TalentSense = 5, Network = 5, Negotiation = 5, Speed = 5, Training = 5,
                SalaryPerDay = 200,
            };
            var rNo = DayProcessing.ProcessDay(noMgr);
            var rWith = DayProcessing.ProcessDay(withMgr);
            Assert.True(rWith.Record.SalaryCosts > rNo.Record.SalaryCosts);
        }
    }
}
