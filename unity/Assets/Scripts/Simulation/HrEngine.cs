// Döner Empire 3D — HR-Engine (Recruiting, Kandidaten, Manager)
// Port aus lib/services/hr_engine.dart.
//
// Verantwortlich für:
//   • Berechnung der effektiven Recruitment-Modifier (Difficulty × Strategie × Manager × Stats)
//   • Pool-Refresh-Intervall und -Kosten
//   • Generierung von Mitarbeiter-Kandidaten (origin, archetype, growth)
//   • Generierung von HR-Manager-Kandidaten (Archetype-spezifische Stat-Bänder)
//   • Training/XP-Intervall
//
// RNG ist durchgehend injizierbar — Engine hält keinen statischen RNG, alle
// Methoden nehmen optional einen Random.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class HrEngine
    {
        private const int DefaultHrCandidates = 3;
        private static readonly Random DefaultRng = new();

        private static readonly HrArchetypeBase NeutralArchetype = new(
            quality: 1.0, salary: 1.0, speed: 1.0,
            special: 1.0, junior: 1.0, training: 1.0);

        private static readonly IReadOnlyDictionary<HrManagerArchetype, (double, double)> SalaryBands =
            new Dictionary<HrManagerArchetype, (double, double)>
            {
                [HrManagerArchetype.TalentScout] = (180, 300),
                [HrManagerArchetype.CostOptimizer] = (80, 170),
                [HrManagerArchetype.ProcessManager] = (120, 220),
                [HrManagerArchetype.PremiumRecruiter] = (350, 600),
                [HrManagerArchetype.TrainingCoach] = (100, 200),
            };

        private static readonly IReadOnlyList<string> HrNames = new List<string>
        {
            "Ayla Demir", "Murat Kaya", "Selin Aslan", "Deniz Arslan",
            "Lara Yilmaz", "Berk Can", "Ece Toprak", "Hakan Öz",
            "Nisa Usta", "Emir Gür", "Leyla Acar", "Sinan Kurt",
        };

        // ──────────────────────────────────────────────────────────────────────
        // Recruitment-Modifier
        // ──────────────────────────────────────────────────────────────────────

        public static HrRecruitmentModifiers RecruitmentModifiers(GameState state)
        {
            var difficulty = state.Modifiers;
            var strategy = HrData.Strategies[state.HrStrategy];
            var manager = state.HrManager;
            var archetype = manager == null
                ? NeutralArchetype
                : (HrData.Archetypes.TryGetValue(manager.Archetype, out var a) ? a : NeutralArchetype);

            var talentN = NormalizedStat(manager?.TalentSense ?? 5);
            var networkN = NormalizedStat(manager?.Network ?? 5);
            var negotiationN = NormalizedStat(manager?.Negotiation ?? 5);
            var speedN = NormalizedStat(manager?.Speed ?? 5);
            var trainingN = NormalizedStat(manager?.Training ?? 5);

            var quality = Math.Clamp(
                difficulty.CandidateQualityMultiplier *
                strategy.CandidateQualityMultiplier *
                archetype.Quality *
                (1.0 + talentN * 0.16 + networkN * 0.07),
                0.65, 1.65);

            var salary = Math.Clamp(
                difficulty.CandidateSalaryMultiplier *
                strategy.CandidateSalaryMultiplier *
                archetype.Salary *
                (1.0 - negotiationN * 0.12),
                0.72, 1.55);

            var speed = Math.Clamp(
                difficulty.HrRecruitmentSpeedMultiplier *
                strategy.RefreshSpeedMultiplier *
                archetype.Speed *
                (1.0 + speedN * 0.20),
                0.55, 2.10);

            var poolSize = Math.Clamp(
                difficulty.HrRecruitmentSpeedMultiplier *
                strategy.PoolSizeMultiplier *
                archetype.Speed *
                (1.0 + speedN * 0.15),
                0.60, 2.20);

            var special = Math.Clamp(
                (manager == null ? 0.03 : 0.07) *
                strategy.SpecialCandidateChance *
                archetype.Special *
                (1.0 + networkN * 0.65),
                0.02, 0.40);

            var junior = Math.Clamp(
                0.18 *
                strategy.JuniorPotentialChance *
                archetype.Junior *
                (1.0 + trainingN * 0.45),
                0.08, 0.65);

            var autoHireAggressiveness = Math.Clamp(
                difficulty.HrRecruitmentSpeedMultiplier *
                strategy.AutoHireAggressivenessMultiplier *
                archetype.Speed *
                (1.0 + speedN * 0.20),
                0.55, 2.30);

            var autoHireReserve = Math.Clamp(
                strategy.AutoHireReserveMultiplier *
                (1.0 - negotiationN * 0.12) *
                (1.0 - speedN * 0.06),
                0.65, 1.35);

            var trainingGrowth = Math.Clamp(
                strategy.TrainingGrowthMultiplier *
                archetype.Training *
                (1.0 + trainingN * 0.28),
                0.80, 1.85);

            return new HrRecruitmentModifiers(
                poolSizeMultiplier: poolSize,
                refreshSpeedMultiplier: speed,
                candidateQualityMultiplier: quality,
                candidateSalaryMultiplier: salary,
                specialCandidateChance: special,
                juniorPotentialChance: junior,
                autoHireAggressivenessMultiplier: autoHireAggressiveness,
                autoHireReserveMultiplier: autoHireReserve,
                trainingGrowthMultiplier: trainingGrowth);
        }

        public static int PoolRefreshIntervalDays(GameState state)
        {
            var hr = RecruitmentModifiers(state);
            return Math.Clamp((int)Math.Round(7.0 / hr.RefreshSpeedMultiplier), 2, 14);
        }

        public static double PoolRefreshCost(GameState state)
        {
            var hr = RecruitmentModifiers(state);
            return Math.Clamp(500.0 / hr.RefreshSpeedMultiplier, 220.0, 1200.0);
        }

        public static double TrainingGrowthMultiplier(GameState state)
            => RecruitmentModifiers(state).TrainingGrowthMultiplier;

        public static int XpIntervalDays(
            GameDifficulty difficulty,
            double trainingGrowthMultiplier,
            double growthPotential)
        {
            var progress = DifficultyData.Get(difficulty).ProgressSpeedMultiplier;
            var training = Math.Clamp(
                trainingGrowthMultiplier + growthPotential * 0.35,
                0.80, 2.20);
            return Math.Clamp((int)Math.Round(30.0 / (progress * training)), 10, 46);
        }

        public static string CurrentEffectSummary(GameState state)
        {
            var hr = RecruitmentModifiers(state);
            var q = (int)Math.Round((hr.CandidateQualityMultiplier - 1.0) * 100);
            var s = (int)Math.Round((hr.CandidateSalaryMultiplier - 1.0) * 100);
            var sp = (int)Math.Round((hr.RefreshSpeedMultiplier - 1.0) * 100);
            var spec = (int)Math.Round(hr.SpecialCandidateChance * 100);
            var train = (int)Math.Round((hr.TrainingGrowthMultiplier - 1.0) * 100);
            return $"Qualität {Signed(q)}, Gehalt {Signed(s)}, "
                 + $"Tempo {Signed(sp)}, Spezial {spec}%, Training {Signed(train)}";
        }

        // ──────────────────────────────────────────────────────────────────────
        // HR-Manager-Kandidaten
        // ──────────────────────────────────────────────────────────────────────

        public static List<HrManager> GenerateHrCandidates(
            int count = DefaultHrCandidates,
            int daySeed = 0,
            Random rng = null)
        {
            var r = rng ?? new Random(unchecked((int)(DateTime.UtcNow.Ticks + daySeed)));
            var archetypes = ((HrManagerArchetype[])Enum.GetValues(typeof(HrManagerArchetype)))
                .ToList();
            Shuffle(archetypes, r);
            var names = new List<string>(HrNames);
            Shuffle(names, r);

            var target = Math.Clamp(count, 1,
                Enum.GetValues(typeof(HrManagerArchetype)).Length);
            var baseStamp = DateTime.UtcNow.Ticks;
            var output = new List<HrManager>(target);
            for (var i = 0; i < target; i++)
            {
                var archetype = archetypes[i % archetypes.Count];
                var name = names[i % names.Count];
                output.Add(BuildHrManager(archetype, name, r, i, baseStamp));
            }
            return output;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Mitarbeiter-Kandidaten
        // ──────────────────────────────────────────────────────────────────────

        public static List<Employee> GenerateCandidatePool(GameState state, Random rng = null)
        {
            var r = rng ?? DefaultRng;
            var hr = RecruitmentModifiers(state);
            var minCount = Math.Clamp((int)Math.Round(6 * hr.PoolSizeMultiplier), 4, 15);
            var maxExtra = Math.Clamp((int)Math.Round(3 * hr.PoolSizeMultiplier), 1, 7);
            var count = minCount + r.Next(maxExtra + 1);
            return GenerateCandidatesForRole(state, count, forcedType: null, rng: r);
        }

        public static List<Employee> GenerateCandidatesForRole(
            GameState state,
            int count,
            EmployeeTypeData forcedType = null,
            Random rng = null)
        {
            var r = rng ?? DefaultRng;
            var hr = RecruitmentModifiers(state);
            var factory = new EmployeeFactory(r);
            var cappedCount = Math.Clamp(count, 1, 20);
            var baseStamp = DateTime.UtcNow.Ticks;
            var candidates = new List<Employee>(cappedCount);
            for (var i = 0; i < cappedCount; i++)
            {
                var type = forcedType ?? GameCatalog.EmployeeTypes[r.Next(GameCatalog.EmployeeTypes.Count)];
                var name = r.NextDouble() < 0.5
                    ? EmployeeNames.Male[r.Next(EmployeeNames.Male.Count)]
                    : EmployeeNames.Female[r.Next(EmployeeNames.Female.Count)];

                var special = BuildSpecialOrigin(state, hr, forcedType, r);
                var archetype = CandidateArchetype(special, hr, r);
                var qualityMultiplier = QualityMultiplierForOrigin(hr.CandidateQualityMultiplier, special);
                var salaryMultiplier = SalaryMultiplierForOrigin(hr.CandidateSalaryMultiplier, special);
                var growthPotential = GrowthPotentialForOrigin(special, hr);

                candidates.Add(factory.CreateCandidate(
                    id: $"cand_{baseStamp}_{i}",
                    type: type, name: name, archetype: archetype,
                    qualityMultiplier: qualityMultiplier,
                    salaryMultiplier: salaryMultiplier,
                    origin: special, growthPotential: growthPotential));
            }
            return candidates;
        }

        // ── Private Helpers ──────────────────────────────────────────────────

        private static string Signed(int value) =>
            value > 0 ? $"+{value}%" : (value < 0 ? $"{value}%" : "0%");

        private static double NormalizedStat(int value)
        {
            var clamped = Math.Clamp(value, 1, 10);
            return Math.Clamp((clamped - 5) / 5.0, -1.0, 1.0);
        }

        private static CandidateOrigin BuildSpecialOrigin(
            GameState state,
            HrRecruitmentModifiers hr,
            EmployeeTypeData forcedType,
            Random rng)
        {
            var roll = rng.NextDouble();
            if (roll > hr.SpecialCandidateChance) return CandidateOrigin.Regular;

            var manager = state.HrManager;
            if (manager != null &&
                manager.Archetype == HrManagerArchetype.TrainingCoach &&
                rng.NextDouble() < 0.45)
                return CandidateOrigin.JuniorPotential;

            if (rng.NextDouble() < hr.JuniorPotentialChance * 0.55)
                return CandidateOrigin.JuniorPotential;

            if (manager != null &&
                manager.Archetype == HrManagerArchetype.PremiumRecruiter &&
                rng.NextDouble() < 0.40)
                return CandidateOrigin.TopTalent;

            if (rng.NextDouble() < 0.24) return CandidateOrigin.HiddenGem;
            if (rng.NextDouble() < 0.20) return CandidateOrigin.TeamContact;
            if (rng.NextDouble() < 0.18) return CandidateOrigin.ExCompetitor;
            if (forcedType != null && rng.NextDouble() < 0.40)
                return CandidateOrigin.HiddenGem;
            return CandidateOrigin.TopTalent;
        }

        private static string CandidateArchetype(
            CandidateOrigin origin, HrRecruitmentModifiers hr, Random rng)
        {
            switch (origin)
            {
                case CandidateOrigin.HiddenGem:
                    return rng.NextDouble() < 0.5 ? "balanced" : "veteran";
                case CandidateOrigin.TopTalent:
                    return "veteran";
                case CandidateOrigin.JuniorPotential:
                    return "rookie";
                case CandidateOrigin.ExCompetitor:
                    return rng.NextDouble() < 0.6 ? "veteran" : "balanced";
                case CandidateOrigin.TeamContact:
                    return "balanced";
                case CandidateOrigin.Regular:
                default:
                    if (hr.CandidateQualityMultiplier < 0.95 && rng.NextDouble() < 0.35)
                        return "rookie";
                    return rng.NextDouble() < 0.30 ? "veteran" : "balanced";
            }
        }

        private static double QualityMultiplierForOrigin(double baseQuality, CandidateOrigin origin)
        {
            var byOrigin = origin switch
            {
                CandidateOrigin.Regular => 1.00,
                CandidateOrigin.HiddenGem => 1.10,
                CandidateOrigin.TopTalent => 1.22,
                CandidateOrigin.JuniorPotential => 0.84,
                CandidateOrigin.ExCompetitor => 1.12,
                CandidateOrigin.TeamContact => 1.04,
                _ => 1.00,
            };
            return Math.Clamp(baseQuality * byOrigin, 0.60, 1.90);
        }

        private static double SalaryMultiplierForOrigin(double baseSalary, CandidateOrigin origin)
        {
            var byOrigin = origin switch
            {
                CandidateOrigin.Regular => 1.00,
                CandidateOrigin.HiddenGem => 0.90,
                CandidateOrigin.TopTalent => 1.20,
                CandidateOrigin.JuniorPotential => 0.78,
                CandidateOrigin.ExCompetitor => 1.18,
                CandidateOrigin.TeamContact => 0.95,
                _ => 1.00,
            };
            return Math.Clamp(baseSalary * byOrigin, 0.60, 1.90);
        }

        private static double GrowthPotentialForOrigin(CandidateOrigin origin, HrRecruitmentModifiers hr)
        {
            var originBonus = origin switch
            {
                CandidateOrigin.Regular => 0.00,
                CandidateOrigin.HiddenGem => 0.05,
                CandidateOrigin.TopTalent => 0.03,
                CandidateOrigin.JuniorPotential => 0.25,
                CandidateOrigin.ExCompetitor => 0.04,
                CandidateOrigin.TeamContact => 0.08,
                _ => 0.00,
            };
            var trainingBonus = Math.Clamp((hr.TrainingGrowthMultiplier - 1.0) * 0.20, 0.0, 0.20);
            return Math.Clamp(originBonus + trainingBonus, 0.0, 0.45);
        }

        private static HrManager BuildHrManager(
            HrManagerArchetype archetype, string name, Random rng, int i, long baseStamp)
        {
            var band = SalaryBands.TryGetValue(archetype, out var b) ? b : (120.0, 220.0);
            var salary = band.Item1 + rng.NextDouble() * (band.Item2 - band.Item1);
            var stats = StatSeedFor(archetype, rng);

            return new HrManager
            {
                Id = $"hr_{baseStamp}_{i}",
                Name = name,
                Archetype = archetype,
                TalentSense = stats.talent,
                Network = stats.network,
                Negotiation = stats.negotiation,
                Speed = stats.speed,
                Training = stats.training,
                SalaryPerDay = Math.Round(salary, 2),
                Level = 1, Xp = 0,
            };
        }

        private static (int talent, int network, int negotiation, int speed, int training)
            StatSeedFor(HrManagerArchetype archetype, Random rng)
        {
            int V(int min, int max) => min + rng.Next((max - min) + 1);
            return archetype switch
            {
                HrManagerArchetype.TalentScout =>
                    (V(7, 10), V(6, 9), V(4, 7), V(5, 8), V(4, 7)),
                HrManagerArchetype.CostOptimizer =>
                    (V(4, 7), V(4, 7), V(7, 10), V(5, 8), V(5, 8)),
                HrManagerArchetype.ProcessManager =>
                    (V(5, 8), V(4, 7), V(5, 8), V(7, 10), V(4, 7)),
                HrManagerArchetype.PremiumRecruiter =>
                    (V(8, 10), V(8, 10), V(4, 7), V(4, 7), V(3, 6)),
                HrManagerArchetype.TrainingCoach =>
                    (V(4, 7), V(5, 8), V(5, 8), V(4, 7), V(8, 10)),
                _ => (V(5, 8), V(5, 8), V(5, 8), V(5, 8), V(5, 8)),
            };
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
