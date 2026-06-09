// Tests für AnalyticsService — Daily Challenge, Monatssteuer, Wochenbericht.
// Spiegelt lib/services/game_engine.dart (dailyChallenge, isChallengeMet,
// monthlyTaxDue, buildWeeklyReport).

using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class AnalyticsServiceTests
    {
        private static GameState State() => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        // ── Daily Challenge ──────────────────────────────────────────────────

        [Fact]
        public void DailyChallengeIsDeterministicAndRotates()
        {
            var c0 = AnalyticsService.DailyChallengeFor(0);
            var c0b = AnalyticsService.DailyChallengeFor(0);
            Assert.Equal(c0.Type, c0b.Type);
            // 4 Typen → rotiert mit Periode 4
            Assert.Equal(AnalyticsService.DailyChallengeFor(0).Type, AnalyticsService.DailyChallengeFor(4).Type);
        }

        [Fact]
        public void DailyChallengeRewardInRange()
        {
            for (var d = 0; d < 40; d++)
            {
                var c = AnalyticsService.DailyChallengeFor(d);
                Assert.InRange(c.Reward, 500.0, 1250.0);
                Assert.False(string.IsNullOrEmpty(c.Emoji));
                Assert.False(string.IsNullOrEmpty(c.Label));
            }
        }

        [Fact]
        public void MoreCustomersMetWhenAboveYesterday()
        {
            var c = new DailyChallenge { Type = ChallengeType.MoreCustomers };
            var yesterday = new DailyRecord { Customers = 100 };
            Assert.True(AnalyticsService.IsChallengeMet(c, 120, 0, 0, yesterday, false));
            Assert.False(AnalyticsService.IsChallengeMet(c, 90, 0, 0, yesterday, false));
            // ohne Vortag nicht erfüllbar
            Assert.False(AnalyticsService.IsChallengeMet(c, 120, 0, 0, null, false));
        }

        [Fact]
        public void MoreProfitComparesAgainstOperatingProfit()
        {
            var c = new DailyChallenge { Type = ChallengeType.MoreProfit };
            var yesterday = new DailyRecord { Revenue = 1000, Costs = 600 }; // opProfit 400
            Assert.True(AnalyticsService.IsChallengeMet(c, 0, 0, 500, yesterday, false));
            Assert.False(AnalyticsService.IsChallengeMet(c, 0, 0, 300, yesterday, false));
        }

        [Fact]
        public void AllProfitableMetWhenNoShopLoss()
        {
            var c = new DailyChallenge { Type = ChallengeType.AllProfitable };
            Assert.True(AnalyticsService.IsChallengeMet(c, 0, 0, 0, null, anyShopLoss: false));
            Assert.False(AnalyticsService.IsChallengeMet(c, 0, 0, 0, null, anyShopLoss: true));
        }

        // ── Monthly Tax ──────────────────────────────────────────────────────

        [Fact]
        public void NoHistoryNoTax()
        {
            Assert.Equal(0, AnalyticsService.MonthlyTaxDue(State()));
        }

        [Fact]
        public void TaxOnlyOnPositiveProfit()
        {
            var s = State();
            // 30 Tage mit operativem Verlust
            for (var d = 1; d <= 30; d++)
                s.History.Add(new DailyRecord { Day = d, Revenue = 100, Costs = 200 });
            Assert.Equal(0, AnalyticsService.MonthlyTaxDue(s));

            var s2 = State();
            for (var d = 1; d <= 30; d++)
                s2.History.Add(new DailyRecord { Day = d, Revenue = 1000, Costs = 600 }); // opProfit 400/Tag
            var profit = 30 * 400.0;
            Assert.Equal(profit * DemandUtils.MonthlyTaxRate, AnalyticsService.MonthlyTaxDue(s2), precision: 4);
        }

        [Fact]
        public void TaxUsesOnlyLast30Days()
        {
            var s = State();
            // 40 Tage: erste 10 hoch profitabel, letzte 30 break-even → ~0 Steuer
            for (var d = 1; d <= 10; d++) s.History.Add(new DailyRecord { Day = d, Revenue = 5000, Costs = 100 });
            for (var d = 11; d <= 40; d++) s.History.Add(new DailyRecord { Day = d, Revenue = 100, Costs = 100 });
            Assert.Equal(0, AnalyticsService.MonthlyTaxDue(s));
        }

        // ── Weekly Report ────────────────────────────────────────────────────

        [Fact]
        public void WeeklyReportNullBeforeFullWeek()
        {
            var s = State();
            for (var d = 1; d <= 6; d++) s.History.Add(new DailyRecord { Day = d, Revenue = 100 });
            Assert.Null(AnalyticsService.BuildWeeklyReport(s));
        }

        [Fact]
        public void WeeklyReportAggregatesLast7Days()
        {
            var s = State();
            s.CurrentDay = 8;
            for (var d = 1; d <= 7; d++)
                s.History.Add(new DailyRecord { Day = d, Revenue = 100 * d, Costs = 50, Customers = d });
            var r = AnalyticsService.BuildWeeklyReport(s);
            Assert.NotNull(r);
            Assert.Equal(7, r.BestDay); // Tag 7 hat höchsten Umsatz (700)
            Assert.Equal(700, r.BestDayRevenue);
            Assert.Equal(100 + 200 + 300 + 400 + 500 + 600 + 700, r.Revenue);
            Assert.Equal(1 + 2 + 3 + 4 + 5 + 6 + 7, r.Customers);
        }

        [Fact]
        public void WeeklyReportComputesGrowthVsPreviousWeek()
        {
            var s = State();
            s.CurrentDay = 15;
            // Vorwoche: opProfit 50/Tag → 350; aktuelle Woche: 150/Tag → 1050
            for (var d = 1; d <= 7; d++) s.History.Add(new DailyRecord { Day = d, Revenue = 100, Costs = 50 });
            for (var d = 8; d <= 14; d++) s.History.Add(new DailyRecord { Day = d, Revenue = 200, Costs = 50 });
            var r = AnalyticsService.BuildWeeklyReport(s);
            Assert.True(r.ProfitGrowthPct > 0, $"growth={r.ProfitGrowthPct}");
        }
    }
}
