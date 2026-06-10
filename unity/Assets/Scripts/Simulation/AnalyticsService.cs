// Döner Empire 3D — Analyse-/Retention-Helfer (Dashboard, Finanzen)
// Port aus lib/services/game_engine.dart: dailyChallenge, isChallengeMet,
// monthlyTaxDue, buildWeeklyReport. Reine, history-basierte Funktionen.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public enum ChallengeType { MoreCustomers, MoreRevenue, MoreProfit, AllProfitable }

    public sealed class DailyChallenge
    {
        public ChallengeType Type;
        public double Reward;

        public string Emoji => Type switch
        {
            ChallengeType.MoreCustomers => "👥",
            ChallengeType.MoreRevenue => "💰",
            ChallengeType.MoreProfit => "📈",
            ChallengeType.AllProfitable => "✅",
            _ => "",
        };

        public string Label => Type switch
        {
            ChallengeType.MoreCustomers => "Bediene heute mehr Kunden als gestern",
            ChallengeType.MoreRevenue => "Mach heute mehr Umsatz als gestern",
            ChallengeType.MoreProfit => "Mach heute mehr Gewinn als gestern",
            ChallengeType.AllProfitable => "Halte heute alle Filialen profitabel",
            _ => "",
        };
    }

    public sealed class WeeklyReport
    {
        public int WeekNumber;
        public double Revenue;
        public double Profit;
        public int Customers;
        public int BestDay;
        public double BestDayRevenue;
        public double ProfitGrowthPct;
    }

    public static class AnalyticsService
    {
        /// <summary>Deterministische, rotierende Tagesaufgabe für einen Tag.</summary>
        public static DailyChallenge DailyChallengeFor(int day)
        {
            var types = (ChallengeType[])Enum.GetValues(typeof(ChallengeType));
            var idx = ((day % types.Length) + types.Length) % types.Length;
            var reward = 500.0 + (((day % 4) + 4) % 4) * 250.0; // 500..1250
            return new DailyChallenge { Type = types[idx], Reward = reward };
        }

        public static bool IsChallengeMet(
            DailyChallenge c,
            int customersToday,
            double revenueToday,
            double profitToday,
            DailyRecord yesterday,
            bool anyShopLoss)
        {
            return c.Type switch
            {
                ChallengeType.MoreCustomers => yesterday != null && customersToday > yesterday.Customers,
                ChallengeType.MoreRevenue => yesterday != null && revenueToday > yesterday.Revenue,
                ChallengeType.MoreProfit => yesterday != null && profitToday > yesterday.OperatingProfit,
                ChallengeType.AllProfitable => !anyShopLoss,
                _ => false,
            };
        }

        /// <summary>
        /// Fällige Steuer auf den operativen Gewinn der letzten ~30 Tage
        /// (nur auf positiven Gewinn). Rein aus der History abgeleitet.
        /// </summary>
        public static double MonthlyTaxDue(GameState state)
        {
            var h = state.History;
            if (h.Count == 0) return 0;
            var window = h.Count >= 30 ? h.Skip(h.Count - 30) : h;
            var profit = window.Sum(r => r.OperatingProfit);
            return profit > 0 ? profit * DemandUtils.MonthlyTaxRate : 0;
        }

        /// <summary>
        /// Wochenbilanz der letzten 7 abgeschlossenen Tage inkl. Wachstum ggü.
        /// der Vorwoche. null, wenn noch keine volle Woche vorliegt.
        /// </summary>
        public static WeeklyReport BuildWeeklyReport(GameState state)
        {
            var h = state.History;
            if (h.Count < 7) return null;
            var last7 = h.Skip(h.Count - 7).ToList();
            var prev7 = h.Count >= 14
                ? h.Skip(h.Count - 14).Take(7).ToList()
                : new List<DailyRecord>();

            var revenue = last7.Sum(r => r.Revenue);
            var profit = last7.Sum(r => r.OperatingProfit);
            var customers = last7.Sum(r => r.Customers);
            var best = last7.Aggregate((a, b) => a.Revenue > b.Revenue ? a : b);

            var prevProfit = prev7.Sum(r => r.OperatingProfit);
            var growth = Math.Abs(prevProfit) > 0.01
                ? (profit - prevProfit) / Math.Abs(prevProfit) * 100
                : 0.0;

            return new WeeklyReport
            {
                WeekNumber = Math.Clamp((int)Math.Floor((state.CurrentDay - 1) / 7.0), 1, 9999),
                Revenue = revenue,
                Profit = profit,
                Customers = customers,
                BestDay = best.Day,
                BestDayRevenue = best.Revenue,
                ProfitGrowthPct = growth,
            };
        }
    }
}
