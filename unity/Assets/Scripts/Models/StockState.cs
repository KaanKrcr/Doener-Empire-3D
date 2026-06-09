// Döner Empire 3D — Aktien-System & IPO
// Port aus lib/models/stock_model.dart.
//
// Nach dem Börsengang wird das Unternehmen handelbar. Aktienkurs schwankt
// basierend auf Quartalsergebnis, Markenbekanntheit, Filialzahl, Reputation.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class StockState
    {
        public bool IsPublic;
        public int IpoDay;
        public double SharePrice;
        public int TotalShares;
        public int PlayerShares;
        public List<double> PriceHistory = new();
        public double LastQuarterProfit;
        public double AnalystExpectation;
        public int LastQuarterDay;

        public double MarketCap => SharePrice * TotalShares;
        public double PlayerShareRatio => TotalShares == 0 ? 1.0 : (double)PlayerShares / TotalShares;
        public bool HasControl => PlayerShareRatio >= 0.51;
        public double PlayerStockValue => SharePrice * PlayerShares;

        public StockState Clone() => new()
        {
            IsPublic = IsPublic, IpoDay = IpoDay, SharePrice = SharePrice,
            TotalShares = TotalShares, PlayerShares = PlayerShares,
            PriceHistory = new List<double>(PriceHistory),
            LastQuarterProfit = LastQuarterProfit,
            AnalystExpectation = AnalystExpectation,
            LastQuarterDay = LastQuarterDay,
        };
    }

    public sealed class QuarterlyReport
    {
        public int Day;
        public double Revenue;
        public double Profit;
        public int Customers;
        public int ShopsAtStart;
        public int ShopsAtEnd;
        public double BrandAwarenessChange;
        public double Expectation;
        public double PriceMovePercent;
        public string Headline;

        public bool BeatsExpectation => Profit > Expectation;
    }

    public static class IpoRequirements
    {
        public const int MinShops = 10;
        public const double MinBrandAwareness = 35;
        public const double MinTotalRevenue = 300000;
    }
}
