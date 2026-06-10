// Tests für HrEngine — spiegeln das Verhalten der Dart-Engine
// (lib/services/hr_engine.dart). RNG durchgängig injizierbar.

using System;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class HrEngineTests
    {
        private static GameState NewState(
            GameDifficulty diff = GameDifficulty.Normal,
            HrStrategy strategy = HrStrategy.Balanced,
            HrManager manager = null)
        {
            var s = GameState.Initial("Test", "Tester", 15000, diff, tutorialEnabled: false);
            s.HrStrategy = strategy;
            s.HrManager = manager;
            return s;
        }

        // ──────────────────────────────────────────────────────────────────────
        // RecruitmentModifiers
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void DefaultStateProducesNeutralishModifiers()
        {
            var state = NewState();
            var hr = HrEngine.RecruitmentModifiers(state);
            Assert.InRange(hr.CandidateQualityMultiplier, 0.65, 1.65);
            Assert.InRange(hr.CandidateSalaryMultiplier, 0.72, 1.55);
            Assert.InRange(hr.RefreshSpeedMultiplier, 0.55, 2.10);
            Assert.InRange(hr.PoolSizeMultiplier, 0.60, 2.20);
            Assert.InRange(hr.SpecialCandidateChance, 0.02, 0.40);
            Assert.InRange(hr.JuniorPotentialChance, 0.08, 0.65);
        }

        [Fact]
        public void EasyHasMoreAggressiveAutoHireThanHard()
        {
            var easy = HrEngine.RecruitmentModifiers(NewState(GameDifficulty.Easy));
            var hard = HrEngine.RecruitmentModifiers(NewState(GameDifficulty.Hard));
            Assert.True(easy.AutoHireAggressivenessMultiplier > hard.AutoHireAggressivenessMultiplier,
                $"easy={easy.AutoHireAggressivenessMultiplier} hard={hard.AutoHireAggressivenessMultiplier}");
        }

        [Fact]
        public void SaveCostsStrategyLowersSalary()
        {
            var balanced = HrEngine.RecruitmentModifiers(NewState());
            var save = HrEngine.RecruitmentModifiers(NewState(strategy: HrStrategy.SaveCosts));
            Assert.True(save.CandidateSalaryMultiplier < balanced.CandidateSalaryMultiplier);
        }

        [Fact]
        public void PrioritizeQualityRaisesQuality()
        {
            var balanced = HrEngine.RecruitmentModifiers(NewState());
            var prio = HrEngine.RecruitmentModifiers(NewState(strategy: HrStrategy.PrioritizeQuality));
            Assert.True(prio.CandidateQualityMultiplier > balanced.CandidateQualityMultiplier);
        }

        [Fact]
        public void TrainJuniorsRaisesJuniorChance()
        {
            var balanced = HrEngine.RecruitmentModifiers(NewState());
            var train = HrEngine.RecruitmentModifiers(NewState(strategy: HrStrategy.TrainJuniors));
            Assert.True(train.JuniorPotentialChance > balanced.JuniorPotentialChance);
            Assert.True(train.TrainingGrowthMultiplier > balanced.TrainingGrowthMultiplier);
        }

        [Fact]
        public void PremiumRecruiterManagerBoostsSpecialChance()
        {
            var noManager = HrEngine.RecruitmentModifiers(NewState());
            var withManager = HrEngine.RecruitmentModifiers(NewState(manager: new HrManager
            {
                Id = "m1", Name = "X", Archetype = HrManagerArchetype.PremiumRecruiter,
                TalentSense = 10, Network = 10, Negotiation = 7, Speed = 7, Training = 5,
                SalaryPerDay = 400,
            }));
            Assert.True(withManager.SpecialCandidateChance > noManager.SpecialCandidateChance);
        }

        [Fact]
        public void CostOptimizerManagerLowersSalary()
        {
            var noManager = HrEngine.RecruitmentModifiers(NewState());
            var withManager = HrEngine.RecruitmentModifiers(NewState(manager: new HrManager
            {
                Id = "m1", Name = "X", Archetype = HrManagerArchetype.CostOptimizer,
                TalentSense = 5, Network = 5, Negotiation = 10, Speed = 7, Training = 6,
                SalaryPerDay = 120,
            }));
            Assert.True(withManager.CandidateSalaryMultiplier < noManager.CandidateSalaryMultiplier);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Pool refresh
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void PoolRefreshIntervalIsBetween2And14()
        {
            foreach (GameDifficulty d in Enum.GetValues(typeof(GameDifficulty)))
                foreach (HrStrategy s in Enum.GetValues(typeof(HrStrategy)))
                {
                    var state = NewState(d, s);
                    Assert.InRange(HrEngine.PoolRefreshIntervalDays(state), 2, 14);
                }
        }

        [Fact]
        public void PoolRefreshCostIsClamped()
        {
            foreach (GameDifficulty d in Enum.GetValues(typeof(GameDifficulty)))
                foreach (HrStrategy s in Enum.GetValues(typeof(HrStrategy)))
                {
                    var cost = HrEngine.PoolRefreshCost(NewState(d, s));
                    Assert.InRange(cost, 220.0, 1200.0);
                }
        }

        // ──────────────────────────────────────────────────────────────────────
        // XpIntervalDays
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void XpIntervalDaysClampedTo10_46()
        {
            foreach (GameDifficulty d in Enum.GetValues(typeof(GameDifficulty)))
                for (var growth = 0.0; growth <= 0.5; growth += 0.05)
                    for (var train = 0.8; train <= 1.85; train += 0.1)
                    {
                        var xp = HrEngine.XpIntervalDays(d, train, growth);
                        Assert.InRange(xp, 10, 46);
                    }
        }

        [Fact]
        public void HigherTrainingGrowsFaster()
        {
            var slow = HrEngine.XpIntervalDays(GameDifficulty.Normal, 0.8, 0.0);
            var fast = HrEngine.XpIntervalDays(GameDifficulty.Normal, 1.8, 0.3);
            Assert.True(fast < slow, $"fast={fast} slow={slow}");
        }

        // ──────────────────────────────────────────────────────────────────────
        // Candidate generation
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void GenerateCandidatePoolHonorsSize()
        {
            var state = NewState();
            var pool = HrEngine.GenerateCandidatePool(state, new Random(123));
            Assert.InRange(pool.Count, 4, 22);  // upper bound = (minCount+maxExtra) max
            Assert.All(pool, c =>
            {
                Assert.False(string.IsNullOrEmpty(c.Id));
                Assert.False(string.IsNullOrEmpty(c.Name));
                Assert.False(string.IsNullOrEmpty(c.TypeId));
                Assert.InRange(c.Speed, 1, 10);
                Assert.InRange(c.Friendliness, 1, 10);
                Assert.InRange(c.Reliability, 1, 10);
                Assert.InRange(c.Experience, 1, 10);
                Assert.InRange(c.GrowthPotential, 0.0, 0.45);
                Assert.True(c.SalaryPerDay > 0);
            });
        }

        [Fact]
        public void GenerateCandidatesForRoleRespectsForcedType()
        {
            var state = NewState();
            var type = Data.GameCatalog.EmployeeTypes[0]; // doener_meister
            var cands = HrEngine.GenerateCandidatesForRole(
                state, count: 5, forcedType: type, rng: new Random(7));
            Assert.Equal(5, cands.Count);
            Assert.All(cands, c => Assert.Equal(type.Id, c.TypeId));
        }

        [Fact]
        public void GenerateCandidatesForRoleClampsCount()
        {
            var state = NewState();
            // count=0 → mind. 1
            var one = HrEngine.GenerateCandidatesForRole(state, count: 0, rng: new Random(1));
            Assert.Single(one);
            // count>20 → max 20
            var many = HrEngine.GenerateCandidatesForRole(state, count: 50, rng: new Random(2));
            Assert.Equal(20, many.Count);
        }

        // ──────────────────────────────────────────────────────────────────────
        // HR-Manager-Kandidaten
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void GenerateHrCandidatesDefaultProduces3()
        {
            var list = HrEngine.GenerateHrCandidates(rng: new Random(11));
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void HrCandidatesHaveStatsInRange()
        {
            var list = HrEngine.GenerateHrCandidates(count: 5, rng: new Random(22));
            Assert.All(list, m =>
            {
                Assert.False(string.IsNullOrEmpty(m.Id));
                Assert.False(string.IsNullOrEmpty(m.Name));
                Assert.InRange(m.TalentSense, 1, 10);
                Assert.InRange(m.Network, 1, 10);
                Assert.InRange(m.Negotiation, 1, 10);
                Assert.InRange(m.Speed, 1, 10);
                Assert.InRange(m.Training, 1, 10);
                Assert.True(m.SalaryPerDay > 0);
                Assert.Equal(1, m.Level);
                Assert.Equal(0, m.Xp);
            });
        }

        [Fact]
        public void HrCandidatesArchetypesAreDistinctUpToPoolSize()
        {
            var list = HrEngine.GenerateHrCandidates(count: 5, rng: new Random(33));
            var arches = list.Select(m => m.Archetype).Distinct().Count();
            // 5 Kandidaten, 5 Archetypen → alle distinct
            Assert.Equal(5, arches);
        }

        [Fact]
        public void TalentScoutSalaryWithinBand()
        {
            // Wir generieren viele TalentScout-Manager und prüfen ob ihre Salaries
            // im Band liegen — Sample über mehrere Seeds für Stabilität.
            for (var seed = 0; seed < 20; seed++)
            {
                var list = HrEngine.GenerateHrCandidates(count: 5, rng: new Random(seed));
                foreach (var m in list)
                {
                    if (m.Archetype != HrManagerArchetype.TalentScout) continue;
                    Assert.InRange(m.SalaryPerDay, 180.0, 300.0);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Summary
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void CurrentEffectSummaryContainsExpectedSections()
        {
            var summary = HrEngine.CurrentEffectSummary(NewState());
            Assert.Contains("Qualität", summary);
            Assert.Contains("Gehalt", summary);
            Assert.Contains("Tempo", summary);
            Assert.Contains("Spezial", summary);
            Assert.Contains("Training", summary);
        }
    }
}
