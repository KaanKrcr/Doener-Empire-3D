// Döner Empire 3D — HR-Manager + Strategie + Recruiting-Modifier
// Port aus lib/models/hr_manager_model.dart.
//
// Reine POCO-Modelle + statische Lookup-Tabellen. RNG-Logik liegt in
// Simulation/HrEngine.cs.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public enum HrManagerArchetype
    {
        TalentScout,
        CostOptimizer,
        ProcessManager,
        PremiumRecruiter,
        TrainingCoach,
    }

    public enum HrStrategy
    {
        Balanced,
        SaveCosts,
        PrioritizeQuality,
        FillFast,
        TrainJuniors,
    }

    public sealed class HrManager
    {
        public string Id;
        public string Name;
        public HrManagerArchetype Archetype;
        public int TalentSense;     // 1..10
        public int Network;         // 1..10
        public int Negotiation;     // 1..10
        public int Speed;           // 1..10
        public int Training;        // 1..10
        public double SalaryPerDay;
        public int Level = 1;
        public int Xp = 0;

        public HrManager Clone() => new()
        {
            Id = Id, Name = Name, Archetype = Archetype,
            TalentSense = TalentSense, Network = Network,
            Negotiation = Negotiation, Speed = Speed, Training = Training,
            SalaryPerDay = SalaryPerDay, Level = Level, Xp = Xp,
        };
    }

    public readonly struct HrRecruitmentModifiers
    {
        public readonly double PoolSizeMultiplier;
        public readonly double RefreshSpeedMultiplier;
        public readonly double CandidateQualityMultiplier;
        public readonly double CandidateSalaryMultiplier;
        public readonly double SpecialCandidateChance;
        public readonly double JuniorPotentialChance;
        public readonly double AutoHireAggressivenessMultiplier;
        public readonly double AutoHireReserveMultiplier;
        public readonly double TrainingGrowthMultiplier;

        public HrRecruitmentModifiers(
            double poolSizeMultiplier,
            double refreshSpeedMultiplier,
            double candidateQualityMultiplier,
            double candidateSalaryMultiplier,
            double specialCandidateChance,
            double juniorPotentialChance,
            double autoHireAggressivenessMultiplier,
            double autoHireReserveMultiplier,
            double trainingGrowthMultiplier)
        {
            PoolSizeMultiplier = poolSizeMultiplier;
            RefreshSpeedMultiplier = refreshSpeedMultiplier;
            CandidateQualityMultiplier = candidateQualityMultiplier;
            CandidateSalaryMultiplier = candidateSalaryMultiplier;
            SpecialCandidateChance = specialCandidateChance;
            JuniorPotentialChance = juniorPotentialChance;
            AutoHireAggressivenessMultiplier = autoHireAggressivenessMultiplier;
            AutoHireReserveMultiplier = autoHireReserveMultiplier;
            TrainingGrowthMultiplier = trainingGrowthMultiplier;
        }
    }

    public readonly struct HrArchetypeBase
    {
        public readonly double Quality, Salary, Speed, Special, Junior, Training;
        public HrArchetypeBase(double quality, double salary, double speed,
                               double special, double junior, double training)
        {
            Quality = quality; Salary = salary; Speed = speed;
            Special = special; Junior = junior; Training = training;
        }
    }

    public static class HrData
    {
        public static readonly IReadOnlyDictionary<HrManagerArchetype, HrArchetypeBase> Archetypes =
            new Dictionary<HrManagerArchetype, HrArchetypeBase>
            {
                [HrManagerArchetype.TalentScout] = new(
                    quality: 1.12, salary: 1.04, speed: 1.00,
                    special: 1.18, junior: 1.00, training: 1.02),
                [HrManagerArchetype.CostOptimizer] = new(
                    quality: 0.94, salary: 0.90, speed: 1.00,
                    special: 0.95, junior: 1.08, training: 1.05),
                [HrManagerArchetype.ProcessManager] = new(
                    quality: 1.00, salary: 0.98, speed: 1.16,
                    special: 1.00, junior: 1.00, training: 1.00),
                [HrManagerArchetype.PremiumRecruiter] = new(
                    quality: 1.20, salary: 1.18, speed: 1.04,
                    special: 1.45, junior: 0.92, training: 0.98),
                [HrManagerArchetype.TrainingCoach] = new(
                    quality: 0.98, salary: 0.96, speed: 0.98,
                    special: 1.02, junior: 1.30, training: 1.20),
            };

        public static readonly IReadOnlyDictionary<HrStrategy, HrRecruitmentModifiers> Strategies =
            new Dictionary<HrStrategy, HrRecruitmentModifiers>
            {
                [HrStrategy.Balanced] = new(
                    poolSizeMultiplier: 1.00, refreshSpeedMultiplier: 1.00,
                    candidateQualityMultiplier: 1.00, candidateSalaryMultiplier: 1.00,
                    specialCandidateChance: 1.00, juniorPotentialChance: 1.00,
                    autoHireAggressivenessMultiplier: 1.00, autoHireReserveMultiplier: 1.00,
                    trainingGrowthMultiplier: 1.00),
                [HrStrategy.SaveCosts] = new(
                    poolSizeMultiplier: 1.00, refreshSpeedMultiplier: 1.05,
                    candidateQualityMultiplier: 0.94, candidateSalaryMultiplier: 0.90,
                    specialCandidateChance: 0.90, juniorPotentialChance: 1.12,
                    autoHireAggressivenessMultiplier: 0.92, autoHireReserveMultiplier: 1.08,
                    trainingGrowthMultiplier: 1.04),
                [HrStrategy.PrioritizeQuality] = new(
                    poolSizeMultiplier: 0.92, refreshSpeedMultiplier: 0.95,
                    candidateQualityMultiplier: 1.14, candidateSalaryMultiplier: 1.14,
                    specialCandidateChance: 1.25, juniorPotentialChance: 0.85,
                    autoHireAggressivenessMultiplier: 0.90, autoHireReserveMultiplier: 1.12,
                    trainingGrowthMultiplier: 1.03),
                [HrStrategy.FillFast] = new(
                    poolSizeMultiplier: 1.22, refreshSpeedMultiplier: 1.20,
                    candidateQualityMultiplier: 0.92, candidateSalaryMultiplier: 0.98,
                    specialCandidateChance: 0.95, juniorPotentialChance: 1.05,
                    autoHireAggressivenessMultiplier: 1.30, autoHireReserveMultiplier: 0.82,
                    trainingGrowthMultiplier: 0.98),
                [HrStrategy.TrainJuniors] = new(
                    poolSizeMultiplier: 1.08, refreshSpeedMultiplier: 1.04,
                    candidateQualityMultiplier: 0.90, candidateSalaryMultiplier: 0.88,
                    specialCandidateChance: 1.10, juniorPotentialChance: 1.45,
                    autoHireAggressivenessMultiplier: 1.06, autoHireReserveMultiplier: 0.95,
                    trainingGrowthMultiplier: 1.22),
            };

        public static string Label(HrManagerArchetype a) => a switch
        {
            HrManagerArchetype.TalentScout => "Talent Scout",
            HrManagerArchetype.CostOptimizer => "Cost Optimizer",
            HrManagerArchetype.ProcessManager => "Process Manager",
            HrManagerArchetype.PremiumRecruiter => "Premium Recruiter",
            HrManagerArchetype.TrainingCoach => "Training Coach",
            _ => "",
        };

        public static string ShortDescription(HrManagerArchetype a) => a switch
        {
            HrManagerArchetype.TalentScout => "Findet häufiger starke Talente und Geheimtipps.",
            HrManagerArchetype.CostOptimizer => "Drückt Gehälter und hält Recruiting effizient.",
            HrManagerArchetype.ProcessManager => "Besetzt offene Stellen verlässlich und schnell.",
            HrManagerArchetype.PremiumRecruiter => "Bringt seltene Top-Kandidaten aus dem Netzwerk.",
            HrManagerArchetype.TrainingCoach => "Fokussiert auf Nachwuchs mit starkem Entwicklungspfad.",
            _ => "",
        };

        public static string Label(HrStrategy s) => s switch
        {
            HrStrategy.Balanced => "Balanced",
            HrStrategy.SaveCosts => "Save Costs",
            HrStrategy.PrioritizeQuality => "Prioritize Quality",
            HrStrategy.FillFast => "Fill Fast",
            HrStrategy.TrainJuniors => "Train Juniors",
            _ => "",
        };

        public static string ShortDescription(HrStrategy s) => s switch
        {
            HrStrategy.Balanced => "Ausgewogen zwischen Kosten, Qualität und Tempo.",
            HrStrategy.SaveCosts => "Günstiger rekrutieren, etwas weniger Top-Qualität.",
            HrStrategy.PrioritizeQuality => "Bessere Kandidaten, dafür teurer und etwas langsamer.",
            HrStrategy.FillFast => "Stellen schnell besetzen, Qualität schwankt stärker.",
            HrStrategy.TrainJuniors => "Mehr günstige Juniors mit besserer Entwicklung.",
            _ => "",
        };
    }

    public static class HrEnumNames
    {
        // Dart enum.name: talentScout, costOptimizer etc.
        public static string ToDart(HrManagerArchetype a) => a switch
        {
            HrManagerArchetype.TalentScout => "talentScout",
            HrManagerArchetype.CostOptimizer => "costOptimizer",
            HrManagerArchetype.ProcessManager => "processManager",
            HrManagerArchetype.PremiumRecruiter => "premiumRecruiter",
            HrManagerArchetype.TrainingCoach => "trainingCoach",
            _ => "processManager",
        };

        public static HrManagerArchetype ArchetypeFromDart(string raw) => raw switch
        {
            "talentScout" => HrManagerArchetype.TalentScout,
            "costOptimizer" => HrManagerArchetype.CostOptimizer,
            "premiumRecruiter" => HrManagerArchetype.PremiumRecruiter,
            "trainingCoach" => HrManagerArchetype.TrainingCoach,
            _ => HrManagerArchetype.ProcessManager,
        };

        public static string ToDart(HrStrategy s) => s switch
        {
            HrStrategy.Balanced => "balanced",
            HrStrategy.SaveCosts => "saveCosts",
            HrStrategy.PrioritizeQuality => "prioritizeQuality",
            HrStrategy.FillFast => "fillFast",
            HrStrategy.TrainJuniors => "trainJuniors",
            _ => "balanced",
        };

        public static HrStrategy StrategyFromDart(string raw) => raw switch
        {
            "saveCosts" => HrStrategy.SaveCosts,
            "prioritizeQuality" => HrStrategy.PrioritizeQuality,
            "fillFast" => HrStrategy.FillFast,
            "trainJuniors" => HrStrategy.TrainJuniors,
            _ => HrStrategy.Balanced,
        };
    }
}
