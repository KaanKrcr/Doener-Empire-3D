// Döner Empire 3D — Corporate: Auto-Hire + Auto-Pricing
// Port aus lib/services/corporate_engine.dart (applyAutoHire,
// applyManagerAutoPricing + Helfer).
//
// Mutativer C#-Stil (statt Dart copyWith): Operationen verändern State direkt.
// RNG ist injizierbar für deterministische Tests.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class AutoManagementEngine
    {
        private const int MaxAutoHiresPerShopPerDay = 2;
        private const int MaxAutoHiresPerShopPerDayWithManager = 3;
        private const int TopCandidateWindow = 4;
        private const double ReserveShareOfDailyCosts = 0.15;
        private const double MinCashReserve = 1500.0;
        private const double BaseHireFeeMultiplier = 1.25;
        private const double MinHireFeeMultiplier = 1.0;

        private static readonly Random DefaultRng = new();

        private static readonly IReadOnlyDictionary<GameDifficulty, double> AutoHireDifficultyReserve =
            new Dictionary<GameDifficulty, double>
            {
                [GameDifficulty.Easy] = 0.70,
                [GameDifficulty.Normal] = 1.00,
                [GameDifficulty.Hard] = 1.18,
                [GameDifficulty.Impossible] = 1.35,
            };

        // ──────────────────────────────────────────────────────────────────────
        // Auto-Pricing
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Manager-Filialen passen Preise leicht an: Premium-Adresse (Rep ≥ 4.0)
        /// +1%, schwache Filiale (Rep &lt; 2.5) -1%, sonst neutral. Mutiert State.
        /// </summary>
        public static GameState ApplyManagerAutoPricing(GameState state)
        {
            foreach (var shop in state.Shops)
            {
                if (!ManagerService.ShopHasActiveManager(shop, state)) continue;
                foreach (var sp in shop.Menu)
                {
                    if (!sp.IsActive) continue;
                    double adjust = 1.0;
                    if (shop.Reputation >= 4.0) adjust = 1.01;
                    else if (shop.Reputation < 2.5) adjust = 0.99;
                    sp.Price = Math.Clamp(sp.Price * adjust, 1.0, 30.0);
                }
            }
            return state;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Auto-Hire
        // ──────────────────────────────────────────────────────────────────────

        public static GameState ApplyAutoHire(GameState state, Random rng = null)
        {
            var r = rng ?? DefaultRng;
            var hrMods = HrEngine.RecruitmentModifiers(state);

            foreach (var shop in state.Shops)
            {
                if (!shop.AutoHire) continue;

                var maxEmp = GameEngineCore.MaxEmployeesForShop(shop);
                var hiresThisShop = 0;

                while (true)
                {
                    var hasLocalManager = ManagerService.ShopHasActiveManager(shop, state);
                    var baseMaxHires = hasLocalManager
                        ? MaxAutoHiresPerShopPerDayWithManager
                        : MaxAutoHiresPerShopPerDay;
                    var maxHiresToday = Math.Clamp(
                        (int)Math.Round(baseMaxHires * hrMods.AutoHireAggressivenessMultiplier),
                        1, 6);

                    var neededByCapacity = GameEngineCore.RecommendedExtraEmployees(
                        shop, state.CurrentDay, state);
                    var mustFill = shop.Employees.Count == 0;
                    if (!mustFill && neededByCapacity <= 0) break;

                    var targetHires = mustFill
                        ? maxHiresToday
                        : Math.Clamp(neededByCapacity, 1, maxHiresToday);
                    if (hiresThisShop >= targetHires) break;

                    var freeSlots = maxEmp - shop.Employees.Count;
                    if (freeSlots <= 0) break;

                    // Kein unendliches Auto-Nachfüllen: leerer Pool stoppt Auto-Hire.
                    if (state.EmployeePool.Count == 0) break;

                    var pick = PickCandidateForAutoHire(
                        state, hasLocalManager, TargetRoleTypeId(shop), r);
                    if (pick == null) break;
                    var (bestEmp, fee) = pick.Value;

                    state.EmployeePool.RemoveAll(e => e.Id == bestEmp.Id);
                    shop.Employees.Add(bestEmp);
                    state.Cash -= fee;
                    hiresThisShop++;
                }
            }
            return state;
        }

        // ── Private Helpers ──────────────────────────────────────────────────

        private static (Employee, double)? PickCandidateForAutoHire(
            GameState state, bool hasLocalManager, string preferredTypeId, Random rng)
        {
            var reserve = AutoHireCashReserve(state);
            var sorted = state.EmployeePool
                .OrderByDescending(e => AutoHireCandidateScore(e, preferredTypeId))
                .ToList();

            var topCount = Math.Min(sorted.Count, TopCandidateWindow);
            var topCandidates = sorted.Take(topCount).ToList();

            var affordableTop = new List<(Employee, double)>();
            foreach (var cand in topCandidates)
            {
                var fee = cand.SalaryPerDay * HireFeeMultiplier(state, hasLocalManager, cand);
                if (state.Cash - fee >= reserve) affordableTop.Add((cand, fee));
            }

            if (affordableTop.Count > 0)
                return affordableTop[rng.Next(affordableTop.Count)];

            foreach (var cand in sorted)
            {
                var fee = cand.SalaryPerDay * HireFeeMultiplier(state, hasLocalManager, cand);
                if (state.Cash - fee >= reserve) return (cand, fee);
            }
            return null;
        }

        private static double AutoHireCashReserve(GameState state)
        {
            var hrMods = HrEngine.RecruitmentModifiers(state);
            var dailyCosts = state.Shops.Sum(shop =>
                GameEngineCore.CalculateDailyCosts(shop, state.CurrentDay, state));
            var percentReserve = dailyCosts * ReserveShareOfDailyCosts;
            var reserve = Math.Max(percentReserve, MinCashReserve);
            var difficultyMult = AutoHireDifficultyReserve.TryGetValue(state.Difficulty, out var m) ? m : 1.0;
            return reserve
                 * state.Modifiers.EconomicPressureMultiplier
                 * difficultyMult
                 * hrMods.AutoHireReserveMultiplier;
        }

        private static double HireFeeMultiplier(
            GameState state, bool hasLocalManager, Employee candidate)
        {
            var hrMods = HrEngine.RecruitmentModifiers(state);
            var activeManagers = ActiveManagerCount(state);
            var globalReduction = Math.Clamp(activeManagers * 0.03, 0.0, 0.25);
            var localReduction = hasLocalManager ? 0.15 : 0.0;
            var hrRecruitingEffect = Math.Clamp(1.0 / hrMods.RefreshSpeedMultiplier, 0.70, 1.90);
            var salaryEffect = hrMods.CandidateSalaryMultiplier;
            var candidatePremium = candidate.Origin switch
            {
                CandidateOrigin.TopTalent => 1.10,
                CandidateOrigin.ExCompetitor => 1.08,
                CandidateOrigin.HiddenGem => 0.94,
                CandidateOrigin.JuniorPotential => 0.90,
                CandidateOrigin.TeamContact => 0.97,
                _ => 1.00,
            };
            var baseMult = BaseHireFeeMultiplier * hrRecruitingEffect * salaryEffect * candidatePremium;
            return Math.Clamp(baseMult - globalReduction - localReduction, MinHireFeeMultiplier, 2.10);
        }

        private static double AutoHireCandidateScore(Employee candidate, string preferredTypeId)
        {
            var typeFit = preferredTypeId == null || candidate.TypeId == preferredTypeId ? 1.0 : 0.88;
            var growthBonus = 1.0 + candidate.GrowthPotential * 0.25;
            var originBonus = candidate.Origin switch
            {
                CandidateOrigin.TopTalent => 1.10,
                CandidateOrigin.HiddenGem => 1.08,
                CandidateOrigin.ExCompetitor => 1.06,
                CandidateOrigin.TeamContact => 1.03,
                CandidateOrigin.JuniorPotential => 0.96,
                _ => 1.00,
            };
            return candidate.OverallScore * typeFit * growthBonus * originBonus;
        }

        /// <summary>Unterbesetzter Rollen-Typ der Filiale (für gezieltes Hiring).</summary>
        private static string TargetRoleTypeId(Shop shop)
        {
            if (shop.Employees.Count == 0) return null;
            var counts = new Dictionary<string, int>();
            foreach (var type in GameCatalog.EmployeeTypes) counts[type.Id] = 0;
            foreach (var emp in shop.Employees)
                counts[emp.TypeId] = (counts.TryGetValue(emp.TypeId, out var v) ? v : 0) + 1;
            if (counts.Count == 0) return null;
            return counts.OrderBy(kv => kv.Value).First().Key;
        }

        private static int ActiveManagerCount(GameState state)
        {
            var activeEmployeeIds = new HashSet<string>(
                state.Shops.SelectMany(s => s.Employees).Select(e => e.Id));
            return state.ManagerEmployeeIds.Count(activeEmployeeIds.Contains);
        }
    }
}
