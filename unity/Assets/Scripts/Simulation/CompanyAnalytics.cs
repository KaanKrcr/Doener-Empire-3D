// Döner Empire 3D — Konzern-Analyse (Ranking, Marktanteil, Gesundheit)
// Port aus lib/services/game_engine.dart: shopsByProfit, playerMarketShareIn,
// healthScore. Reine Lesefunktionen für die Analyse-Screens.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct ShopProfit
    {
        public readonly Shop Shop;
        public readonly double Profit;
        public ShopProfit(Shop shop, double profit) { Shop = shop; Profit = profit; }
    }

    public sealed class HealthScore
    {
        public double Score;
        public string Label;
    }

    public static class CompanyAnalytics
    {
        /// <summary>Filialen nach geschätztem Tagesgewinn (Umsatz − Kosten), absteigend.</summary>
        public static List<ShopProfit> ShopsByProfit(GameState state)
        {
            var list = state.Shops.Select(s =>
            {
                var rev = GameEngineCore.CalculateDailyRevenue(s, state.CurrentDay, state);
                var cost = GameEngineCore.CalculateDailyCosts(s, state.CurrentDay, state);
                return new ShopProfit(s, rev - cost);
            }).ToList();
            list.Sort((a, b) => b.Profit.CompareTo(a.Profit));
            return list;
        }

        /// <summary>
        /// Geschätzter Marktanteil des Spielers in einer Stadt (0..1) =
        /// 1 − Summe der Konkurrenz-Anteile, sofern der Spieler dort vertreten ist.
        /// </summary>
        public static double PlayerMarketShareIn(GameState state, string cityId)
        {
            if (!state.HasShopIn(cityId)) return 0;
            var compSum = state.CompetitorsIn(cityId).Sum(c => c.MarketShare);
            return Math.Clamp(1 - compSum, 0.0, 1.0);
        }

        /// <summary>
        /// Unternehmens-Gesundheit (0..100) aus Liquidität, Profitabilität,
        /// Verschuldung und Reputation. Rein abgeleitet.
        /// </summary>
        public static HealthScore Health(GameState state)
        {
            if (state.Shops.Count == 0)
                return new HealthScore { Score = 50, Label = "Neu gegründet" };

            double dailyRev = 0, dailyCost = 0;
            foreach (var shop in state.Shops)
            {
                dailyRev += GameEngineCore.CalculateDailyRevenue(shop, state.CurrentDay, state);
                dailyCost += GameEngineCore.CalculateDailyCosts(shop, state.CurrentDay, state);
            }
            var dailyProfit = dailyRev - dailyCost;

            var runway = dailyCost > 0 ? state.Cash / dailyCost : 30.0;
            var liq = Math.Clamp(runway / 14.0, 0.0, 1.0);

            var margin = dailyRev > 0 ? dailyProfit / dailyRev : 0.0;
            var profScore = Math.Clamp((margin + 0.10) / 0.40, 0.0, 1.0);

            var debt = state.ActiveLoansTotal;
            var debtScore = state.Cash > 0
                ? 1 - Math.Clamp(debt / (state.Cash + debt), 0.0, 1.0)
                : 0.0;

            var avgRep = state.Shops.Sum(sh => sh.Reputation) / state.Shops.Count;
            var repScore = Math.Clamp(avgRep / 5.0, 0.0, 1.0);

            var score = (liq * 0.30 + profScore * 0.35 + debtScore * 0.15 + repScore * 0.20) * 100;

            var label = score >= 80 ? "Exzellent"
                : score >= 62 ? "Stark"
                : score >= 45 ? "Solide"
                : score >= 28 ? "Angeschlagen"
                : "Kritisch";

            return new HealthScore { Score = Math.Clamp(score, 0, 100), Label = label };
        }
    }
}
