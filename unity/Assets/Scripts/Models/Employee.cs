// Döner Empire 3D — Mitarbeiter
// Port aus lib/models/employee_model.dart (Employee, Traits, EmployeeFactory).
// EmployeeTypeData + kEmployeeTypes liegen in Data/GameCatalog.cs.
// RNG ist injizierbar (deterministische Tests); Dart-Enum-Namen für Save-Kompat.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DoenerEmpire.Models
{
    public enum CandidateOrigin
    {
        Regular, HiddenGem, TopTalent, JuniorPotential, ExCompetitor, TeamContact
    }

    public enum EmployeeSkill { Speed, Friendliness, Reliability, Experience }

    public enum EmployeeShift { Ganztags, Frueh, Mittag, Abend }

    public enum PersonalityTrait
    {
        Charmer, Workaholic, Mentor, Loyal, Influencer, Modest, Hothead, Lucky
    }

    /// <summary>Dart-`enum.name`-Mapping für Save-Kompatibilität.</summary>
    public static class EmployeeEnumNames
    {
        public static string ToDart(CandidateOrigin v) => v switch
        {
            CandidateOrigin.HiddenGem => "hiddenGem",
            CandidateOrigin.TopTalent => "topTalent",
            CandidateOrigin.JuniorPotential => "juniorPotential",
            CandidateOrigin.ExCompetitor => "exCompetitor",
            CandidateOrigin.TeamContact => "teamContact",
            _ => "regular",
        };

        public static CandidateOrigin OriginFromDart(string raw) => raw switch
        {
            "hiddenGem" => CandidateOrigin.HiddenGem,
            "topTalent" => CandidateOrigin.TopTalent,
            "juniorPotential" => CandidateOrigin.JuniorPotential,
            "exCompetitor" => CandidateOrigin.ExCompetitor,
            "teamContact" => CandidateOrigin.TeamContact,
            _ => CandidateOrigin.Regular,
        };

        // Shifts & Traits sind im Dart-Original einwortig-lowercase.
        public static string ToDart(EmployeeShift v) => v.ToString().ToLowerInvariant();

        public static EmployeeShift ShiftFromDart(string raw) => raw switch
        {
            "frueh" => EmployeeShift.Frueh,
            "mittag" => EmployeeShift.Mittag,
            "abend" => EmployeeShift.Abend,
            _ => EmployeeShift.Ganztags,
        };

        public static string ToDart(PersonalityTrait v) => v.ToString().ToLowerInvariant();

        public static PersonalityTrait? TraitFromDart(string raw)
        {
            foreach (PersonalityTrait t in Enum.GetValues(typeof(PersonalityTrait)))
                if (ToDart(t) == raw) return t;
            return null;
        }
    }

    /// <summary>
    /// Mitarbeiter mit Stats 1..10:
    /// Speed → Kapazität, Friendliness → Reputation/Stammkunden,
    /// Reliability → weniger Tagesschwankung, Experience → Qualität.
    /// </summary>
    public sealed class Employee
    {
        public string Id;
        public string TypeId;
        public string Name;
        public int Speed;          // 1..10
        public int Friendliness;   // 1..10
        public int Reliability;    // 1..10
        public int Experience;     // 1..10
        public double SalaryPerDay;
        public List<PersonalityTrait> Traits = new();
        public int DaysEmployed = 0;
        public CandidateOrigin Origin = CandidateOrigin.Regular;
        public double GrowthPotential = 0.0;
        public EmployeeShift Shift = EmployeeShift.Ganztags;

        public double OverallScore => ((Speed + Friendliness + Reliability + Experience) / 4.0) / 10.0;

        public int StarRating =>
            Clamp((int)Math.Round(OverallScore * 5, MidpointRounding.AwayFromZero), 1, 5);

        public int SkillLevel => StarRating;

        public double QualityFactor => Experience / 10.0;

        public bool IsSpecialCandidate => Origin != CandidateOrigin.Regular;

        public double SpeedFactor
        {
            get
            {
                var b = Speed / 10.0;
                return Traits.Contains(PersonalityTrait.Workaholic)
                    ? ClampD(b * 1.25, 0.0, 1.5) : b;
            }
        }

        public double FriendlinessFactor
        {
            get
            {
                var b = Friendliness / 10.0;
                return Traits.Contains(PersonalityTrait.Charmer)
                    ? ClampD(b * 1.20, 0.0, 1.5) : b;
            }
        }

        public double ReliabilityFactor
        {
            get
            {
                var b = Reliability / 10.0;
                if (Traits.Contains(PersonalityTrait.Workaholic))
                    b = ClampD(b - 0.10, 0.0, 1.0);
                return b;
            }
        }

        public bool HasTrait(PersonalityTrait t) => Traits.Contains(t);

        internal static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
        internal static double ClampD(double v, double lo, double hi) => v < lo ? lo : (v > hi ? hi : v);
    }

    /// <summary>Erzeugt zufällige Kandidaten; RNG injizierbar für Tests.</summary>
    public sealed class EmployeeFactory
    {
        private readonly Random _rng;
        public EmployeeFactory(Random rng = null) => _rng = rng ?? new Random();

        public Employee CreateCandidate(
            string id,
            Data.EmployeeTypeData type,
            string name,
            string archetype = null,
            double qualityMultiplier = 1.0,
            double salaryMultiplier = 1.0,
            CandidateOrigin origin = CandidateOrigin.Regular,
            double growthPotential = 0.0)
        {
            var pick = archetype ??
                new[] { "rookie", "balanced", "balanced", "veteran" }[_rng.Next(4)];

            int s, f, r, e;
            switch (pick)
            {
                case "rookie":
                    s = 2 + _rng.Next(5); f = 2 + _rng.Next(5);
                    r = 2 + _rng.Next(5); e = 1 + _rng.Next(4);
                    break;
                case "veteran":
                    s = 6 + _rng.Next(5); f = 6 + _rng.Next(5);
                    r = 6 + _rng.Next(5); e = 6 + _rng.Next(5);
                    break;
                default: // balanced
                    s = 3 + _rng.Next(6); f = 3 + _rng.Next(6);
                    r = 3 + _rng.Next(6); e = 3 + _rng.Next(6);
                    break;
            }

            switch (_rng.Next(4)) // Spezialisierung +2
            {
                case 0: s = Employee.Clamp(s + 2, 1, 10); break;
                case 1: f = Employee.Clamp(f + 2, 1, 10); break;
                case 2: r = Employee.Clamp(r + 2, 1, 10); break;
                case 3: e = Employee.Clamp(e + 2, 1, 10); break;
            }

            int ScaleStat(int stat)
            {
                var scaled = 1 + ((stat - 1) * qualityMultiplier);
                return Employee.Clamp(
                    (int)Math.Round(scaled, MidpointRounding.AwayFromZero), 1, 10);
            }
            s = ScaleStat(s); f = ScaleStat(f); r = ScaleStat(r); e = ScaleStat(e);

            var traits = new List<PersonalityTrait>();
            if (_rng.NextDouble() < 0.40) traits.Add(RollTrait(null));
            if (traits.Count > 0 && _rng.NextDouble() < 0.12)
            {
                var second = RollTrait(traits[0]);
                if (!traits.Contains(second)) traits.Add(second);
            }

            var overall = (s + f + r + e) / 40.0;
            var salary = type.BaseSalaryPerDay * (0.6 + overall * 1.2);
            if (traits.Contains(PersonalityTrait.Modest)) salary *= 0.9;
            if (traits.Contains(PersonalityTrait.Influencer)) salary *= 1.15;
            if (traits.Contains(PersonalityTrait.Workaholic)) salary *= 1.05;
            salary *= salaryMultiplier;

            return new Employee
            {
                Id = id,
                TypeId = type.Id,
                Name = name,
                Speed = s, Friendliness = f, Reliability = r, Experience = e,
                SalaryPerDay = Math.Round(salary, 2),
                Traits = traits,
                DaysEmployed = 0,
                Origin = origin,
                GrowthPotential = Employee.ClampD(growthPotential, 0.0, 1.0),
            };
        }

        private PersonalityTrait RollTrait(PersonalityTrait? exclude)
        {
            var pool = Enum.GetValues(typeof(PersonalityTrait)).Cast<PersonalityTrait>()
                .Where(t => exclude == null || t != exclude.Value).ToList();
            return pool[_rng.Next(pool.Count)];
        }
    }

    public static class EmployeeNames
    {
        public static readonly IReadOnlyList<string> Male = new List<string>
        {
            "Ali", "Mehmet", "Mustafa", "Kemal", "Ahmet", "Yusuf", "Hasan",
            "Ibrahim", "Lukas", "Noah", "Leon", "Maximilian", "Jonas", "Elias",
        };
        public static readonly IReadOnlyList<string> Female = new List<string>
        {
            "Fatma", "Ayşe", "Emine", "Hatice", "Zeynep", "Laura", "Jana",
            "Sarah", "Emma", "Lena", "Anna",
        };
    }
}
