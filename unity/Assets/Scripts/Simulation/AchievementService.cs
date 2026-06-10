// Döner Empire 3D — Achievement-Auswertung
// Port aus lib/providers/game_provider.dart (_checkAchievements).

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class AchievementService
    {
        /// <summary>Baut die Metriken für die Achievement-Prüfung aus dem State.</summary>
        public static AchievementMetrics MetricsFor(GameState state)
        {
            var maxRep = state.Shops.Count == 0 ? 0.0 : state.Shops.Max(s => s.Reputation);
            return new AchievementMetrics(
                totalShops: state.Shops.Count,
                totalEmployees: state.EmployeeCount,
                totalRevenue: state.TotalRevenue,
                cash: state.Cash,
                currentDay: state.CurrentDay,
                customersTotal: state.CustomersServedTotal,
                maxShopRep: maxRep,
                brandAwareness: state.Brand?.BrandAwareness ?? 0,
                competitorsBeat: 0);
        }

        /// <summary>
        /// Liefert die NEU erfüllten Achievements (noch nicht in
        /// state.AchievementIds). Mutiert den State NICHT.
        /// </summary>
        public static List<Achievement> CheckNewlyUnlocked(GameState state)
        {
            var metrics = MetricsFor(state);
            var owned = new HashSet<string>(state.AchievementIds);
            var result = new List<Achievement>();
            foreach (var a in AchievementCatalog.All)
            {
                if (owned.Contains(a.Id)) continue;
                if (a.Check(metrics)) result.Add(a);
            }
            return result;
        }

        /// <summary>
        /// Prüft neue Achievements und trägt ihre IDs in den State ein.
        /// Liefert die neu freigeschalteten zurück (für UI/Feier).
        /// </summary>
        public static List<Achievement> ApplyNewlyUnlocked(GameState state)
        {
            var newOnes = CheckNewlyUnlocked(state);
            foreach (var a in newOnes) state.AchievementIds.Add(a.Id);
            return newOnes;
        }

        /// <summary>Summe der Trophäen-Punkte über alle freigeschalteten Achievements.</summary>
        public static int TotalPoints(GameState state)
        {
            var owned = new HashSet<string>(state.AchievementIds);
            return AchievementCatalog.All
                .Where(a => owned.Contains(a.Id))
                .Sum(a => AchievementTierInfo.Points(a.Tier));
        }
    }
}
