using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class ShopSizeTierTests
    {
        [Fact]
        public void ConfigValuesMatchFlutter()
        {
            var klein = ShopSizing.ConfigFor(ShopSizeTier.Klein);
            Assert.Equal(3, klein.EmployeeCap);
            Assert.Equal(1.00, klein.CapacityMultiplier);
            Assert.Equal(0, klein.UpgradeCost);
            Assert.Equal(1.00, klein.RentMultiplier);

            var mittel = ShopSizing.ConfigFor(ShopSizeTier.Mittel);
            Assert.Equal(5, mittel.EmployeeCap);
            Assert.Equal(1.35, mittel.CapacityMultiplier);
            Assert.Equal(8000, mittel.UpgradeCost);
            Assert.Equal(1.25, mittel.RentMultiplier);
            Assert.Equal(-0.02, mittel.MoraleDeltaOnUpgrade);

            var gross = ShopSizing.ConfigFor(ShopSizeTier.Gross);
            Assert.Equal(8, gross.EmployeeCap);
            Assert.Equal(25000, gross.UpgradeCost);
            Assert.Equal(1.60, gross.RentMultiplier);

            var flag = ShopSizing.ConfigFor(ShopSizeTier.Flagship);
            Assert.Equal(12, flag.EmployeeCap);
            Assert.Equal(2.20, flag.CapacityMultiplier);
            Assert.Equal(70000, flag.UpgradeCost);
            Assert.Equal(2.10, flag.RentMultiplier);
            Assert.Equal(-0.08, flag.MoraleDeltaOnUpgrade);
        }

        [Fact]
        public void NextTierProgression()
        {
            Assert.Equal(ShopSizeTier.Mittel, ShopSizing.NextTier(ShopSizeTier.Klein));
            Assert.Equal(ShopSizeTier.Gross, ShopSizing.NextTier(ShopSizeTier.Mittel));
            Assert.Equal(ShopSizeTier.Flagship, ShopSizing.NextTier(ShopSizeTier.Gross));
            Assert.Null(ShopSizing.NextTier(ShopSizeTier.Flagship));
        }

        [Fact]
        public void ExpansionCostIsNextTierUpgradeCost()
        {
            Assert.Equal(8000, ShopSizing.ExpansionCost(ShopSizeTier.Klein));
            Assert.Equal(25000, ShopSizing.ExpansionCost(ShopSizeTier.Mittel));
            Assert.Equal(70000, ShopSizing.ExpansionCost(ShopSizeTier.Gross));
            Assert.Equal(0, ShopSizing.ExpansionCost(ShopSizeTier.Flagship));
        }

        [Fact]
        public void CityEmployeeCapByTier()
        {
            Assert.Equal(5, ShopSizing.CityEmployeeCap(CityTier.Klein));
            Assert.Equal(8, ShopSizing.CityEmployeeCap(CityTier.Mittel));
            Assert.Equal(10, ShopSizing.CityEmployeeCap(CityTier.Gross));
            Assert.Equal(12, ShopSizing.CityEmployeeCap(CityTier.Metropole));
        }

        // Kernregel der Spec: effektiver Cap = min(Stadt, Stufe).
        [Fact]
        public void EffectiveCapIsMinOfCityAndTier()
        {
            // Flagship-Stufe (12) in Kleinstadt (5) -> durch Stadt auf 5 gedeckelt.
            Assert.Equal(5, ShopSizing.EmployeeCap(CityTier.Klein, ShopSizeTier.Flagship));
            // Flagship in Metropole (12) -> volle 12.
            Assert.Equal(12, ShopSizing.EmployeeCap(CityTier.Metropole, ShopSizeTier.Flagship));
            // Klein-Stufe (3) in Metropole -> durch Stufe auf 3 gedeckelt.
            Assert.Equal(3, ShopSizing.EmployeeCap(CityTier.Metropole, ShopSizeTier.Klein));
            // Mittel-Stufe (5) in Großstadt (10) -> Stufe begrenzt auf 5.
            Assert.Equal(5, ShopSizing.EmployeeCap(CityTier.Gross, ShopSizeTier.Mittel));
        }

        [Fact]
        public void LegacyExpansionLevelMapsAndClamps()
        {
            Assert.Equal(ShopSizeTier.Klein, ShopSizing.FromLegacyExpansionLevel(0));
            Assert.Equal(ShopSizeTier.Flagship, ShopSizing.FromLegacyExpansionLevel(3));
            Assert.Equal(ShopSizeTier.Flagship, ShopSizing.FromLegacyExpansionLevel(99));
            Assert.Equal(ShopSizeTier.Klein, ShopSizing.FromLegacyExpansionLevel(-5));
        }

        [Fact]
        public void LabelsNonEmpty()
        {
            foreach (ShopSizeTier t in System.Enum.GetValues(typeof(ShopSizeTier)))
                Assert.False(string.IsNullOrWhiteSpace(ShopSizing.Label(t)));
        }
    }
}
