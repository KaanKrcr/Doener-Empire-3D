// Döner Empire 3D — Corporate: Börse / IPO / Aktienkurs
// Port aus lib/services/corporate_engine.dart (Stocks-Teil).
//
// Facilities und M&A folgen in M7b/M7c. RNG ist im Tagespreis-Update
// injizierbar (deterministische Tests).

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class CorporateStocksEngine
    {
        private static readonly Random DefaultRng = new();

        /// <summary>Prüft, ob die IPO-Voraussetzungen erfüllt sind.</summary>
        public static bool CanDoIpo(GameState state)
        {
            if (state.Stocks.IsPublic) return false;
            return state.Shops.Count >= IpoRequirements.MinShops
                && state.Brand.BrandAwareness >= IpoRequirements.MinBrandAwareness
                && state.TotalRevenue >= IpoRequirements.MinTotalRevenue;
        }

        /// <summary>Bewertung: Filialwert + Markenwert + Cashflow-Multiple.</summary>
        public static double EstimateValuation(GameState state)
        {
            var shopValue = state.Shops.Count * 50000.0;
            var brandValue = state.Brand.BrandAwareness * 2500.0;
            var yearlyProfitEstimate = state.History.Count == 0
                ? 0.0
                : state.History.Sum(r => r.Profit) * (365.0 / state.History.Count);
            const double pe = 10.0;
            var cashflowValue = Math.Max(0.0, yearlyProfitEstimate * pe);
            return shopValue + brandValue + cashflowValue;
        }

        /// <summary>
        /// Führt den IPO durch. <paramref name="percentToFloat"/> wird auf
        /// 0.10..0.49 geclamped. Mutiert State (Cash + Stocks) und liefert ihn.
        /// </summary>
        public static GameState PerformIpo(GameState state, double percentToFloat)
        {
            if (!CanDoIpo(state)) return state;
            var p = Math.Clamp(percentToFloat, 0.10, 0.49);
            var valuation = EstimateValuation(state);

            const double initialSharePrice = 10.0;
            var totalShares = (int)Math.Round(valuation / initialSharePrice);
            var floatedShares = (int)Math.Round(totalShares * p);
            var playerShares = totalShares - floatedShares;

            var ipoProceeds = floatedShares * initialSharePrice * 0.95;

            state.Cash += ipoProceeds;
            state.Stocks.IsPublic = true;
            state.Stocks.IpoDay = state.CurrentDay;
            state.Stocks.SharePrice = initialSharePrice;
            state.Stocks.TotalShares = totalShares;
            state.Stocks.PlayerShares = playerShares;
            state.Stocks.PriceHistory = new List<double> { initialSharePrice };
            state.Stocks.LastQuarterDay = state.CurrentDay;
            state.Stocks.AnalystExpectation = valuation * 0.02 / 4;
            return state;
        }

        /// <summary>Tägliches Aktienkurs-Update (kleiner Random-Walk + Faktoren).</summary>
        public static StockState UpdateDailyPrice(GameState state, Random rng = null)
        {
            var s = state.Stocks;
            if (!s.IsPublic) return s;
            var r = rng ?? DefaultRng;

            var noise = (r.NextDouble() - 0.5) * 0.02;
            var brandImpact = (state.Brand.BrandAwareness - 50) / 5000.0;
            var shopImpact = (state.Shops.Count - 10) / 1000.0;

            var priceMove = Math.Clamp(1.0 + noise + brandImpact + shopImpact, 0.95, 1.05);
            var newPrice = Math.Max(0.01, s.SharePrice * priceMove);

            s.PriceHistory.Add(newPrice);
            if (s.PriceHistory.Count > 60)
                s.PriceHistory.RemoveRange(0, s.PriceHistory.Count - 60);
            s.SharePrice = newPrice;
            return s;
        }

        public static bool IsQuarterDue(GameState state)
        {
            if (!state.Stocks.IsPublic) return false;
            return state.CurrentDay - state.Stocks.LastQuarterDay >= 90;
        }

        /// <summary>
        /// Erzeugt den Quartalsbericht und aktualisiert den Aktienkurs anhand
        /// der Performance gegen die Analysten-Erwartung. Mutiert Stocks.
        /// </summary>
        public static QuarterlyReport GenerateQuarterlyReport(GameState state)
        {
            var s = state.Stocks;
            var quarterStart = s.LastQuarterDay;
            var quarterDays = state.History.Where(r => r.Day >= quarterStart).ToList();

            var qRevenue = quarterDays.Sum(r => r.Revenue);
            var qProfit = quarterDays.Sum(r => r.Profit);
            var qCustomers = quarterDays.Sum(r => r.Customers);

            var delta = qProfit - s.AnalystExpectation;
            var deltaPercent = s.AnalystExpectation == 0
                ? 0.0
                : Math.Clamp(delta / Math.Abs(s.AnalystExpectation), -0.50, 0.50);

            var priceMove = 1.0 + deltaPercent * 0.6;
            var newPrice = Math.Max(0.5, s.SharePrice * priceMove);
            var newExpectation = Math.Max(0.0, qProfit * 0.9 + s.AnalystExpectation * 0.1);

            string headline;
            if (deltaPercent > 0.20) headline = "SENSATION! Döner Empire übertrifft Erwartungen massiv";
            else if (deltaPercent > 0.05) headline = "Gewinn schlägt Analysten-Prognose";
            else if (deltaPercent > -0.05) headline = "Solides Quartal - im Rahmen der Erwartungen";
            else if (deltaPercent > -0.20) headline = "Aktie unter Druck - Quartal enttäuschend";
            else headline = "KURSEINBRUCH! Massive Gewinnwarnung";

            var report = new QuarterlyReport
            {
                Day = state.CurrentDay,
                Revenue = qRevenue,
                Profit = qProfit,
                Customers = qCustomers,
                ShopsAtStart = state.Shops.Count,
                ShopsAtEnd = state.Shops.Count,
                BrandAwarenessChange = 0,
                Expectation = s.AnalystExpectation,
                PriceMovePercent = (priceMove - 1) * 100,
                Headline = headline,
            };

            s.SharePrice = newPrice;
            s.LastQuarterProfit = qProfit;
            s.AnalystExpectation = newExpectation;
            s.LastQuarterDay = state.CurrentDay;
            return report;
        }
    }
}
