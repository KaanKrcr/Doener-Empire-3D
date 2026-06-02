using System;
using System.Linq;
using Xunit;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class EmployeeModelTests
    {
        [Fact]
        public void ShopProductAndEquipmentClone()
        {
            var sp = new ShopProduct { ProductId = "doener_fladen", Price = 7.0, IsActive = true };
            var c = sp.Clone();
            c.Price = 9.0;
            Assert.Equal(7.0, sp.Price); // Original unverändert
            Assert.Equal(9.0, c.Price);

            var se = new ShopEquipment { EquipmentId = "spiess_standard" };
            Assert.Equal("spiess_standard", se.Clone().EquipmentId);
        }

        [Fact]
        public void ComputedFactorsMatchDart()
        {
            var e = new Employee { Speed = 10, Friendliness = 5, Reliability = 8, Experience = 6 };
            Assert.Equal(1.0, e.SpeedFactor);
            Assert.Equal(0.5, e.FriendlinessFactor);
            Assert.Equal(0.8, e.ReliabilityFactor);
            Assert.Equal(0.6, e.QualityFactor);
            // overall = (10+5+8+6)/4/10 = 0.725 → *5 = 3.625 → round = 4
            Assert.Equal(0.725, e.OverallScore, 3);
            Assert.Equal(4, e.StarRating);
        }

        [Fact]
        public void WorkaholicAndCharmerTraitsModifyFactors()
        {
            var w = new Employee { Speed = 8, Reliability = 8 };
            w.Traits.Add(PersonalityTrait.Workaholic);
            Assert.Equal(0.8 * 1.25, w.SpeedFactor, 5);   // +25% Tempo
            Assert.Equal(0.8 - 0.10, w.ReliabilityFactor, 5); // -0.10 Zuverlässigkeit

            var ch = new Employee { Friendliness = 6 };
            ch.Traits.Add(PersonalityTrait.Charmer);
            Assert.Equal(0.6 * 1.20, ch.FriendlinessFactor, 5);
        }

        [Fact]
        public void StarRatingClampedOneToFive()
        {
            var low = new Employee { Speed = 1, Friendliness = 1, Reliability = 1, Experience = 1 };
            Assert.Equal(1, low.StarRating);
            var high = new Employee { Speed = 10, Friendliness = 10, Reliability = 10, Experience = 10 };
            Assert.Equal(5, high.StarRating);
        }

        [Fact]
        public void IsSpecialCandidateOnlyForNonRegular()
        {
            Assert.False(new Employee { Origin = CandidateOrigin.Regular }.IsSpecialCandidate);
            Assert.True(new Employee { Origin = CandidateOrigin.TopTalent }.IsSpecialCandidate);
        }

        private static EmployeeTypeData Meister =>
            GameCatalog.EmployeeTypes.Single(t => t.Id == "doener_meister");

        [Fact]
        public void FactoryStatsWithinBoundsAndSalaryPositive()
        {
            var f = new EmployeeFactory(new Random(42));
            for (var i = 0; i < 300; i++)
            {
                var emp = f.CreateCandidate($"e{i}", Meister, "Test");
                Assert.InRange(emp.Speed, 1, 10);
                Assert.InRange(emp.Friendliness, 1, 10);
                Assert.InRange(emp.Reliability, 1, 10);
                Assert.InRange(emp.Experience, 1, 10);
                Assert.True(emp.SalaryPerDay > 0);
                Assert.InRange(emp.Traits.Count, 0, 2);
                Assert.Equal("doener_meister", emp.TypeId);
            }
        }

        [Fact]
        public void VeteransOutperformRookiesOnAverage()
        {
            var f = new EmployeeFactory(new Random(7));
            double rookieSum = 0, vetSum = 0;
            const int n = 200;
            for (var i = 0; i < n; i++)
            {
                rookieSum += f.CreateCandidate("r", Meister, "R", "rookie").OverallScore;
                vetSum += f.CreateCandidate("v", Meister, "V", "veteran").OverallScore;
            }
            Assert.True(vetSum / n > rookieSum / n);
        }

        [Fact]
        public void QualityMultiplierRaisesStats()
        {
            var f = new EmployeeFactory(new Random(99));
            double lowSum = 0, highSum = 0;
            const int n = 150;
            for (var i = 0; i < n; i++)
            {
                lowSum += f.CreateCandidate("l", Meister, "L", "balanced", qualityMultiplier: 0.5).OverallScore;
                highSum += f.CreateCandidate("h", Meister, "H", "balanced", qualityMultiplier: 1.5).OverallScore;
            }
            Assert.True(highSum / n > lowSum / n);
        }

        [Fact]
        public void EmployeeEnumDartNamesRoundTrip()
        {
            foreach (CandidateOrigin o in Enum.GetValues(typeof(CandidateOrigin)))
                Assert.Equal(o, EmployeeEnumNames.OriginFromDart(EmployeeEnumNames.ToDart(o)));
            foreach (EmployeeShift s in Enum.GetValues(typeof(EmployeeShift)))
                Assert.Equal(s, EmployeeEnumNames.ShiftFromDart(EmployeeEnumNames.ToDart(s)));
            foreach (PersonalityTrait t in Enum.GetValues(typeof(PersonalityTrait)))
                Assert.Equal(t, EmployeeEnumNames.TraitFromDart(EmployeeEnumNames.ToDart(t)));

            Assert.Equal("hiddenGem", EmployeeEnumNames.ToDart(CandidateOrigin.HiddenGem));
            Assert.Equal("exCompetitor", EmployeeEnumNames.ToDart(CandidateOrigin.ExCompetitor));
        }
    }
}
