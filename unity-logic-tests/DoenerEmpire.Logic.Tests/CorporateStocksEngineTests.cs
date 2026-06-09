// Tests für CorporateStocksEngine — IPO, Aktienkurs, Quartalsbericht.
// Spiegelt lib/services/corporate_engine.dart (Stocks-Teil).

using System;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class CorporateStocksEngineTests
    {
        private static GameState IpoReadyState()
        {
            var s = GameState.Initial("Empire", "Boss", 50000, tutorialEnabled: false);
            for (var i = 0; i < 12; i++)
                s.Shops.Add(new Shop { Id = $"s{i}", CityId = "fulda", Reputation = 4.0 });
            s.Brand.BrandAwareness = 40;
            s.TotalRevenue = 350000;
            return s;
        }

        // ── CanDoIpo ─────────────────────────────────────────────────────────

        [Fact]
        public void CanDoIpoTrueWhenRequirementsMet()
        {
            Assert.True(CorporateStocksEngine.CanDoIpo(IpoReadyState()));
        }

        [Fact]
        public void CanDoIpoFalseBelowThresholds()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            Assert.False(CorporateStocksEngine.CanDoIpo(s));
        }

        [Fact]
        public void CanDoIpoFalseWhenAlreadyPublic()
        {
            var s = IpoReadyState();
            s.Stocks.IsPublic = true;
            Assert.False(CorporateStocksEngine.CanDoIpo(s));
        }

        // ── Valuation ────────────────────────────────────────────────────────

        [Fact]
        public void ValuationCombinesShopAndBrand()
        {
            var s = IpoReadyState();
            var v = CorporateStocksEngine.EstimateValuation(s);
            // 12×50000 + 40×2500 = 600000 + 100000 = 700000 (+ Cashflow ≥ 0)
            Assert.True(v >= 700000);
        }

        // ── PerformIpo ───────────────────────────────────────────────────────

        [Fact]
        public void PerformIpoMakesPublicAndAddsCash()
        {
            var s = IpoReadyState();
            var cashBefore = s.Cash;
            CorporateStocksEngine.PerformIpo(s, 0.30);
            Assert.True(s.Stocks.IsPublic);
            Assert.True(s.Cash > cashBefore);
            Assert.Equal(10.0, s.Stocks.SharePrice);
            Assert.True(s.Stocks.TotalShares > 0);
        }

        [Fact]
        public void PerformIpoKeepsPlayerControlAt30Percent()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            Assert.True(s.Stocks.HasControl); // 70% bleibt beim Spieler
            Assert.True(s.Stocks.PlayerShareRatio > 0.5);
        }

        [Fact]
        public void PerformIpoClampsFloatTo49Percent()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.90); // → 0.49
            Assert.True(s.Stocks.PlayerShareRatio >= 0.51);
        }

        [Fact]
        public void PerformIpoNoOpWhenNotEligible()
        {
            var s = GameState.Initial("X", "Y", 50000, tutorialEnabled: false);
            CorporateStocksEngine.PerformIpo(s, 0.30);
            Assert.False(s.Stocks.IsPublic);
        }

        // ── UpdateDailyPrice ─────────────────────────────────────────────────

        [Fact]
        public void DailyPriceNoOpWhenPrivate()
        {
            var s = IpoReadyState();
            var stocks = CorporateStocksEngine.UpdateDailyPrice(s, new Random(1));
            Assert.False(stocks.IsPublic);
            Assert.Equal(0, stocks.SharePrice);
        }

        [Fact]
        public void DailyPriceMovesWithinBandAndRecordsHistory()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            var startPrice = s.Stocks.SharePrice;
            var rng = new Random(42);
            for (var d = 0; d < 30; d++)
            {
                var prev = s.Stocks.SharePrice;
                CorporateStocksEngine.UpdateDailyPrice(s, rng);
                var ratio = s.Stocks.SharePrice / prev;
                Assert.InRange(ratio, 0.95, 1.05);
            }
            Assert.True(s.Stocks.PriceHistory.Count > 1);
        }

        [Fact]
        public void PriceHistoryTrimmedTo60()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            var rng = new Random(7);
            for (var d = 0; d < 80; d++) CorporateStocksEngine.UpdateDailyPrice(s, rng);
            Assert.True(s.Stocks.PriceHistory.Count <= 60);
        }

        // ── Quarterly ────────────────────────────────────────────────────────

        [Fact]
        public void IsQuarterDueAfter90Days()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            s.Stocks.LastQuarterDay = 0;
            s.CurrentDay = 90;
            Assert.True(CorporateStocksEngine.IsQuarterDue(s));
            s.CurrentDay = 89;
            Assert.False(CorporateStocksEngine.IsQuarterDue(s));
        }

        [Fact]
        public void QuarterlyReportBeatingExpectationRaisesPrice()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            var priceBefore = s.Stocks.SharePrice;
            s.Stocks.AnalystExpectation = 1000;
            // History mit hohem Gewinn
            for (var d = 1; d <= 90; d++)
                s.History.Add(new DailyRecord { Day = d, Revenue = 5000, Costs = 1000 });
            var report = CorporateStocksEngine.GenerateQuarterlyReport(s);
            Assert.True(report.BeatsExpectation);
            Assert.True(s.Stocks.SharePrice > priceBefore);
            Assert.Equal(s.CurrentDay, s.Stocks.LastQuarterDay);
        }

        [Fact]
        public void QuarterlyReportMissingExpectationLowersPrice()
        {
            var s = IpoReadyState();
            CorporateStocksEngine.PerformIpo(s, 0.30);
            var priceBefore = s.Stocks.SharePrice;
            s.Stocks.AnalystExpectation = 100000; // unerreichbar hoch
            for (var d = 1; d <= 90; d++)
                s.History.Add(new DailyRecord { Day = d, Revenue = 500, Costs = 400 });
            var report = CorporateStocksEngine.GenerateQuarterlyReport(s);
            Assert.False(report.BeatsExpectation);
            Assert.True(s.Stocks.SharePrice < priceBefore);
        }
    }
}
