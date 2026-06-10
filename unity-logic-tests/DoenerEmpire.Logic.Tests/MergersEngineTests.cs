// Tests für MergersEngine — Konkurrenten-Übernahme.
// Spiegelt lib/services/corporate_engine.dart (acquisitionPrice, acquireCompetitor).

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class MergersEngineTests
    {
        private static Competitor Rival(int shops = 2, double rep = 3.0, string city = "fulda")
            => new()
            {
                Id = "rival1", Name = "Mehmet's Grill", CityId = city,
                ShopCount = shops, Reputation = rep, PriceLevel = 1.0,
                Personality = CompetitorPersonality.Balanced,
            };

        [Fact]
        public void AcquisitionPriceScalesWithShopsAndRep()
        {
            // rep 3.0 → factor 1.0; 2 shops × 60000 × 1.0 = 120000
            Assert.Equal(120000, MergersEngine.AcquisitionPrice(Rival(2, 3.0)));
            // höherer Ruf → teurer
            Assert.True(MergersEngine.AcquisitionPrice(Rival(2, 5.0))
                      > MergersEngine.AcquisitionPrice(Rival(2, 3.0)));
        }

        [Fact]
        public void AcquisitionPriceRepFactorClamped()
        {
            // rep 0.5 → factor clamp 0.7; rep 5 → 5/3=1.67 clamp 1.6
            var low = MergersEngine.AcquisitionPrice(Rival(1, 0.5));
            var high = MergersEngine.AcquisitionPrice(Rival(1, 5.0));
            Assert.Equal(60000 * 0.7, low, precision: 2);
            Assert.Equal(60000 * 1.6, high, precision: 2);
        }

        [Fact]
        public void AcquireAddsPlayerShopsAndRemovesCompetitor()
        {
            var s = GameState.Initial("Empire", "Boss", 500000, tutorialEnabled: false);
            var rival = Rival(3, 3.5);
            s.Competitors.Add(rival);

            MergersEngine.AcquireCompetitor(s, rival);

            Assert.Equal(3, s.Shops.Count);
            Assert.DoesNotContain(s.Competitors, c => c.Id == "rival1");
            Assert.All(s.Shops, shop =>
            {
                Assert.True(shop.WasAcquired);
                Assert.Equal("Mehmet's Grill", shop.OriginalCompetitorName);
                Assert.Equal("fulda", shop.CityId);
                Assert.Equal(3.5, shop.Reputation);
                Assert.NotEmpty(shop.Menu); // Default-Produkte
                Assert.Equal("Empire", shop.Name);
            });
        }

        [Fact]
        public void AcquireDeductsCash()
        {
            var s = GameState.Initial("Empire", "Boss", 500000, tutorialEnabled: false);
            var rival = Rival(2, 3.0); // price 120000
            s.Competitors.Add(rival);
            MergersEngine.AcquireCompetitor(s, rival);
            Assert.Equal(380000, s.Cash);
        }

        [Fact]
        public void AcquireNoOpWhenTooPoor()
        {
            var s = GameState.Initial("Empire", "Boss", 1000, tutorialEnabled: false);
            var rival = Rival(3, 4.0);
            s.Competitors.Add(rival);
            MergersEngine.AcquireCompetitor(s, rival);
            Assert.Empty(s.Shops);
            Assert.Single(s.Competitors);
            Assert.Equal(1000, s.Cash);
        }

        [Fact]
        public void AcquiredShopsUseDefaultProductsOnly()
        {
            var s = GameState.Initial("Empire", "Boss", 500000, tutorialEnabled: false);
            var rival = Rival(1, 3.0);
            s.Competitors.Add(rival);
            MergersEngine.AcquireCompetitor(s, rival);
            var shop = s.Shops[0];
            // Alle Menü-Produkte müssen Default-Produkte sein
            Assert.All(shop.Menu, sp =>
            {
                var pd = Data.GameData.AllProducts.First(p => p.Id == sp.ProductId);
                Assert.True(pd.IsDefault);
            });
        }

        [Fact]
        public void AcquiredShopsCountTowardMissionAcquiredShops()
        {
            var s = GameState.Initial("Empire", "Boss", 500000, tutorialEnabled: false);
            var rival = Rival(2, 3.0);
            s.Competitors.Add(rival);
            MergersEngine.AcquireCompetitor(s, rival);

            var probe = new Mission { Id = "p", Type = MissionType.AcquiredShops, Target = 2 };
            Assert.Equal(2.0, MissionEngine.CurrentValueFor(probe, s));
        }
    }
}
