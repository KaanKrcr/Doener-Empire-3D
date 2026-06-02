using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class ShopModelTests
    {
        private static Shop MakeShop() => new()
        {
            Id = "s1", Name = "Sultan Döner", CityId = "fulda", LocationName = "Marktplatz",
            FootTraffic = 5000, WeeklyRent = 1400, DayOpened = 1,
            Menu = { new ShopProduct { ProductId = "doener_fladen", Price = 6.5 } },
            Equipment = { new ShopEquipment { EquipmentId = "spiess_standard" } },
            UpgradeIds = { "wifi" },
        };

        [Fact]
        public void DailyRentIsWeeklyOverSeven()
        {
            Assert.Equal(1400.0 / 7.0, MakeShop().DailyRent, 6);
        }

        [Fact]
        public void DisplayNameAndBrandingHint()
        {
            var s = MakeShop();
            Assert.Equal("Sultan Döner", s.DisplayName);
            Assert.Equal("Marktplatz", s.BrandingHint);
            s.CustomName = "  Filiale Mitte  ";
            Assert.True(s.HasCustomName);
            Assert.Equal("Sultan Döner - Filiale Mitte", s.DisplayName);
            Assert.Equal("Sultan Döner", s.BrandingHint);
        }

        [Fact]
        public void AcquiredHintOnlyWhenAcquired()
        {
            var s = MakeShop();
            Assert.Null(s.AcquiredHint);
            s.WasAcquired = true;
            s.OriginalCompetitorName = "King Döner";
            Assert.Equal("ehemals King Döner", s.AcquiredHint);
        }

        [Fact]
        public void ExpansionLevelMirrorsSizeTier()
        {
            var s = MakeShop();
            Assert.Equal(0, s.ExpansionLevel);
            s.SizeTier = ShopSizeTier.Flagship;
            Assert.Equal(3, s.ExpansionLevel);
        }

        [Fact]
        public void HasEquipmentAndUpgrade()
        {
            var s = MakeShop();
            Assert.True(s.HasEquipment("spiess_standard"));
            Assert.False(s.HasEquipment("ofen_lahmacun"));
            Assert.True(s.HasUpgrade("wifi"));
            Assert.False(s.HasUpgrade("delivery"));
        }

        [Fact]
        public void CloneIsDeepForMutableChildren()
        {
            var s = MakeShop();
            var c = s.Clone();
            c.Menu[0].Price = 9.0;
            c.Reputation = 4.9;
            Assert.Equal(6.5, s.Menu[0].Price);   // Original-Menü unverändert
            Assert.Equal(3.0, s.Reputation);
            Assert.NotSame(s.Menu, c.Menu);
            Assert.NotSame(s.UpgradeIds, c.UpgradeIds);
        }

        [Fact]
        public void ActiveCampaignLifecycle()
        {
            var ac = new ActiveCampaign { CampaignId = "flyer", StartDay = 5, EndDay = 10 };
            Assert.False(ac.IsActive(4));
            Assert.True(ac.IsActive(5));
            Assert.True(ac.IsActive(9));
            Assert.False(ac.IsActive(10));         // endDay exklusiv
            Assert.Equal(5, ac.RemainingDays(5));
            Assert.Equal(0.0, ac.Progress(5), 3);
            Assert.Equal(0.4, ac.Progress(7), 3); // elapsed 2 / total 5
            Assert.Equal(1.0, ac.Progress(10), 3);
        }

        [Fact]
        public void MarketingCampaignCostPerDay()
        {
            var multi = new MarketingCampaign { Cost = 1000, DurationDays = 5 };
            Assert.True(multi.HasDuration);
            Assert.Equal(200, multi.CostPerDay);

            var once = new MarketingCampaign { Cost = 500, DurationDays = 0 };
            Assert.False(once.HasDuration);
            Assert.Equal(500, once.CostPerDay);
        }
    }
}
