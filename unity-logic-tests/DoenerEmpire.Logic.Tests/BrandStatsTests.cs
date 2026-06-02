using Xunit;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class BrandStatsTests
    {
        [Fact]
        public void DefaultsAndInCity()
        {
            var b = new BrandStats();
            Assert.Equal(5.0, b.BrandAwareness);
            Assert.Equal(0.0, b.InCity("fulda"));
        }

        [Fact]
        public void CustomerMultiplierCombinesBrandAndCity()
        {
            var b = new BrandStats { BrandAwareness = 50 };
            b.CityReputation["fulda"] = 50;
            // 1 + 0.05*0.5 + 0.40*0.5 = 1 + 0.025 + 0.20 = 1.225
            Assert.Equal(1.225, b.CustomerMultiplier("fulda"), 3);
        }

        [Fact]
        public void CustomerMultiplierClampedHigh()
        {
            var b = new BrandStats { BrandAwareness = 100 };
            b.CityReputation["berlin"] = 100;
            // 1 + 0.05 + 0.40 = 1.45 (genau an der Obergrenze)
            Assert.Equal(1.45, b.CustomerMultiplier("berlin"), 3);
        }

        [Fact]
        public void CustomerMultiplierClampedLow()
        {
            var b = new BrandStats { BrandAwareness = 0 };
            // 1.0 liegt über der Untergrenze 0.85 → bleibt 1.0
            Assert.Equal(1.0, b.CustomerMultiplier("unknown"), 3);
        }

        [Theory]
        [InlineData(85, "Legendär 👑", 5)]
        [InlineData(65, "Weithin bekannt", 4)]
        [InlineData(45, "Etablierte Marke", 3)]
        [InlineData(25, "Bekannt", 2)]
        [InlineData(10, "Aufstrebend", 1)]
        [InlineData(2, "Unbekannt", 1)]
        public void TierLabelAndStars(double awareness, string label, int stars)
        {
            var b = new BrandStats { BrandAwareness = awareness };
            Assert.Equal(label, b.TierLabel);
            Assert.Equal(stars, b.TierStars);
        }

        [Fact]
        public void CloneIsIndependent()
        {
            var b = new BrandStats { BrandAwareness = 30 };
            b.CityReputation["fulda"] = 12;
            var c = b.Clone();
            c.BrandAwareness = 90;
            c.CityReputation["fulda"] = 99;
            Assert.Equal(30, b.BrandAwareness);
            Assert.Equal(12, b.InCity("fulda"));
        }
    }
}
