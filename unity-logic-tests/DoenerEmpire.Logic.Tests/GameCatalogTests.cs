using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;

namespace DoenerEmpire.Logic.Tests
{
    public class GameCatalogTests
    {
        [Fact]
        public void ThreeEmployeeTypes()
        {
            Assert.Equal(3, GameCatalog.EmployeeTypes.Count);
            var meister = GameCatalog.EmployeeTypes.Single(e => e.Id == "doener_meister");
            Assert.Equal(80, meister.BaseSalaryPerDay);
            Assert.Equal(0.40, meister.QualityContribution);
            Assert.Equal(0.20, meister.SpeedContribution);
        }

        [Fact]
        public void EightEquipmentItems()
        {
            Assert.Equal(8, GameCatalog.AllEquipment.Count);
        }

        [Fact]
        public void SpiessTierProgression()
        {
            var klein = GameCatalog.AllEquipment.Single(e => e.Id == "spiess_klein");
            var std = GameCatalog.AllEquipment.Single(e => e.Id == "spiess_standard");
            var profi = GameCatalog.AllEquipment.Single(e => e.Id == "spiess_profi");
            Assert.True(klein.Price < std.Price && std.Price < profi.Price);
            Assert.True(klein.CapacityBonus < std.CapacityBonus && std.CapacityBonus < profi.CapacityBonus);
            Assert.Equal(EquipmentCategory.Spiess, profi.Category);
            Assert.Equal(200, profi.CapacityBonus);
        }

        [Fact]
        public void FritteuseUnlocksPommesAndBox()
        {
            var f = GameCatalog.AllEquipment.Single(e => e.Id == "fritteuse_standard");
            Assert.Equal("pommes", f.UnlocksProductId);
            Assert.Contains("doenerbox", f.AdditionalUnlocks);
        }

        [Fact]
        public void FridgeSavesIngredients()
        {
            var fridge = GameCatalog.AllEquipment.Single(e => e.Id == "kuehlschrank");
            Assert.Equal(0.08, fridge.IngredientSavingBonus);
        }

        [Fact]
        public void EachCityTierHasFourLocationTemplates()
        {
            foreach (CityTier t in System.Enum.GetValues(typeof(CityTier)))
            {
                Assert.True(GameCatalog.LocationTemplates.ContainsKey(t));
                Assert.Equal(4, GameCatalog.LocationTemplates[t].Count);
            }
        }

        [Fact]
        public void MetropoleTopLocationHasHighestFactors()
        {
            var metro = GameCatalog.LocationTemplates[CityTier.Metropole];
            var top = metro.Single(l => l.Name == "Top-Lage Mitte");
            Assert.Equal(2.0, top.FootTrafficFactor);
            Assert.Equal(2.8, top.RentFactor);
            Assert.Equal(LocationPersonality.Touristic, top.Personality);
        }

        [Fact]
        public void KleinTemplatesCoverDistinctPersonalities()
        {
            var klein = GameCatalog.LocationTemplates[CityTier.Klein];
            var personalities = klein.Select(l => l.Personality).ToHashSet();
            Assert.Equal(4, personalities.Count); // touristic, business, transit, residential
        }
    }
}
