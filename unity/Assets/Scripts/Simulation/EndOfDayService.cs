// Döner Empire 3D — End-of-Day-Orchestrierung
// Port aus lib/providers/game_provider.dart (endDay), Logik-Anteil.
//
// Bündelt nach dem ökonomischen ProcessDay die Content-Auswertung:
// Missionen, Story-Kampagne, Achievements, Event-Roll, Quartalsbericht,
// Wochenbericht, Monatssteuer, Daily Challenge. RNG injizierbar.
//
// Bewusst additiv: ProcessDay (Wirtschaft) bleibt unangetastet; dieser Service
// ruft es und ergänzt die Meta-Systeme. GameController kann optional hierauf
// umstellen, um den vollen Flutter-Tageszyklus abzubilden.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class EndOfDayResult
    {
        public DayResult Day;
        public Mission MissionCompleted;
        public CampaignChapter ChapterCompleted;
        public List<Achievement> NewAchievements = new();
        public GameEvent RolledEvent;
        public QuarterlyReport QuarterlyReport;
        public WeeklyReport WeeklyReport;
        public double TaxPaid;
        public bool ChallengeMet;
        public double ChallengeReward;
        public List<string> NewlyUnlockedCities = new();
    }

    public static class EndOfDayService
    {
        /// <summary>
        /// Führt den vollständigen Tagesabschluss aus: ProcessDay (Wirtschaft)
        /// gefolgt von Missionen/Kampagne/Achievements/Event/Reports/Steuer/
        /// Challenge. Mutiert den State und liefert ein Ergebnis-Bündel für die UI.
        /// </summary>
        public static EndOfDayResult Resolve(GameState state, Random rng = null)
        {
            var r = rng ?? new Random();
            var today = state.CurrentDay;

            // Pre-Mutation-Werte für Daily-Challenge-Vergleich.
            var yesterday = state.History.Count > 0 ? state.History[^1] : null;
            var hadShops = state.Shops.Count > 0;
            var anyLoss = state.Shops.Any(s =>
                GameEngineCore.CalculateDailyRevenue(s, today, state)
                - GameEngineCore.CalculateDailyCosts(s, today, state) < 0);

            // 1) Ökonomischer Tagesabschluss.
            var dayResult = DayProcessing.ProcessDay(state);
            var record = dayResult.Record;

            var result = new EndOfDayResult
            {
                Day = dayResult,
                NewlyUnlockedCities = dayResult.NewlyUnlockedCities,
            };

            // 2) Mission-Check.
            var missionRes = MissionEngine.CheckAndApply(state, state.Missions);
            result.MissionCompleted = missionRes.JustCompleted;

            // 3) Story-Kampagne (mehrere Kapitel möglich).
            for (var i = 0; i < CampaignData.Chapters.Count; i++)
            {
                var cr = CampaignEngine.CheckAndApply(state);
                if (cr.JustCompleted == null) break;
                result.ChapterCompleted ??= cr.JustCompleted;
            }

            // 4) Achievements.
            result.NewAchievements = AchievementService.ApplyNewlyUnlocked(state);

            // 5) Event ziehen (gewichtet, Chance skaliert mit Filialzahl).
            var eventChance = Math.Clamp(today >= 0 ? state.Shops.Count * 0.10 : 0, 0.0, 0.45);
            if (hadShops && r.NextDouble() < eventChance)
                result.RolledEvent = EventEngine.RollEvent(state, r);

            // 6) Quartalsbericht.
            if (CorporateStocksEngine.IsQuarterDue(state))
                result.QuarterlyReport = CorporateStocksEngine.GenerateQuarterlyReport(state);

            // 7) Wochenbericht (zu Wochenbeginn).
            if (state.CurrentDay > 7 && state.CurrentDay % 7 == 1)
                result.WeeklyReport = AnalyticsService.BuildWeeklyReport(state);

            // 8) Monatssteuer (alle 30 Tage).
            if (state.CurrentDay > 30 && (state.CurrentDay - 1) % 30 == 0)
            {
                var tax = AnalyticsService.MonthlyTaxDue(state);
                if (tax > 0)
                {
                    state.Cash -= tax;
                    result.TaxPaid = tax;
                }
            }

            // 9) Daily Challenge auswerten.
            if (hadShops)
            {
                var challenge = AnalyticsService.DailyChallengeFor(today);
                var met = AnalyticsService.IsChallengeMet(
                    challenge,
                    customersToday: record.Customers,
                    revenueToday: record.Revenue,
                    profitToday: record.Revenue - record.Costs,
                    yesterday: yesterday,
                    anyShopLoss: anyLoss);
                if (met)
                {
                    state.Cash += challenge.Reward;
                    result.ChallengeMet = true;
                    result.ChallengeReward = challenge.Reward;
                }
            }

            return result;
        }
    }
}
