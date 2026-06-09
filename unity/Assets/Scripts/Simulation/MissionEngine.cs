// Döner Empire 3D — MissionEngine
// Port aus lib/services/mission_engine.dart.
//
// Wertet Mission-Bedingungen gegen den aktuellen GameState aus, markiert
// erfüllte Missionen als done und schreibt deren Cash-Belohnung gut.
//
// HINWEIS: CompanyPublic (IPO) bleibt vorerst auf 0, weil Stocks/IPO im
// Endgame-Port (Phase M5+) noch fehlt. Bedingung wird damit nie erfüllt —
// gewünschtes Verhalten, bis das Stocks-Modell portiert ist.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class MissionCheckResult
    {
        public GameState State;
        public Mission JustCompleted;
    }

    public static class MissionEngine
    {
        public static Mission ActiveMission(IReadOnlyList<Mission> missions)
        {
            foreach (var m in missions) if (!m.IsDone) return m;
            return null;
        }

        public static int DoneCount(IReadOnlyList<Mission> missions)
            => missions.Count(m => m.IsDone);

        public static double ActiveProgress(GameState state, IReadOnlyList<Mission> missions)
        {
            var m = ActiveMission(missions);
            if (m == null) return 1.0;
            var cur = ProgressAdjustedValue(m, state);
            return System.Math.Clamp(cur / m.Target, 0.0, 1.0);
        }

        public static double CurrentValueFor(Mission m, GameState state)
            => CurrentValue(m, state);

        public static MissionCheckResult CheckAndApply(
            GameState state, List<Mission> missions)
        {
            var m = ActiveMission(missions);
            if (m == null) return new MissionCheckResult { State = state, JustCompleted = null };

            var cur = ProgressAdjustedValue(m, state);
            if (cur >= m.Target)
            {
                m.IsDone = true;
                state.Cash += m.CashReward;
                return new MissionCheckResult { State = state, JustCompleted = m };
            }
            return new MissionCheckResult { State = state, JustCompleted = null };
        }

        // ── Private ──────────────────────────────────────────────────────────

        private static double ProgressAdjustedValue(Mission m, GameState state)
        {
            var current = CurrentValue(m, state);
            var speed = state.Modifiers.ProgressSpeedMultiplier;
            return m.Type switch
            {
                MissionType.TotalRevenue => current * speed,
                MissionType.ReachCash => current * speed,
                MissionType.UnlockCity => current * speed,
                _ => current,
            };
        }

        private static double CurrentValue(Mission m, GameState state)
        {
            switch (m.Type)
            {
                case MissionType.OpenFirstShop:
                    return state.Shops.Count;
                case MissionType.TotalRevenue:
                    return state.TotalRevenue;
                case MissionType.HireEmployees:
                    return state.EmployeeCount;
                case MissionType.BuyEquipment:
                    return state.Shops.Sum(s => s.Equipment.Count);
                case MissionType.UnlockProduct:
                    foreach (var shop in state.Shops)
                        foreach (var se in shop.Equipment)
                        {
                            var eq = GameCatalog.AllEquipment.FirstOrDefault(e => e.Id == se.EquipmentId);
                            if (eq != null && !string.IsNullOrEmpty(eq.UnlocksProductId)) return 1;
                        }
                    return 0;
                case MissionType.ReachCash:
                    return state.Cash;
                case MissionType.ShopCount:
                    if (m.Id == "metropole")
                    {
                        return state.Shops.Count(s =>
                        {
                            var city = GameData.AllCities.FirstOrDefault(c => c.Id == s.CityId);
                            return city != null && city.Tier == CityTier.Metropole;
                        });
                    }
                    return state.Shops.Count;
                case MissionType.UnlockCity:
                    // 3 Startstädte sind frei, erst ab 4. zählt
                    return System.Math.Clamp(state.UnlockedCityIds.Count - 3, 0, 1000);
                case MissionType.DaysSurvived:
                    return state.CurrentDay;
                case MissionType.ReputationLevel:
                    if (state.Shops.Count == 0) return 0;
                    return state.Shops.Max(s => s.Reputation);
                case MissionType.CompanyPublic:
                    return state.Stocks != null && state.Stocks.IsPublic ? 1 : 0;
                case MissionType.BrandAwareness:
                    return state.Brand.BrandAwareness;
                case MissionType.AcquiredShops:
                    return state.Shops.Count(s => s.WasAcquired);
                default:
                    return 0;
            }
        }
    }
}
