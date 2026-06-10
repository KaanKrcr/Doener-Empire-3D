// Tests für ProductAnalytics.ProductProfitBreakdown + CompanyAnalytics.ShopAlerts.
// Spiegelt lib/services/game_engine.dart (productProfitBreakdown, shopAlerts).

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ProductAndAlertAnalyticsTests
    {
        private static GameState State() => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop ProductiveShop(string id = "s1", int footTraffic = 6000, double rep = 4.0)
        {
            var s = new Shop
            {
                Id = id, CityId = "fulda", IsOpen = true, Reputation = rep,
                FootTraffic = footTraffic, WeeklyRent = 1400, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            s.Menu.Add(new ShopProduct { ProductId = "ayran", Price = 2.0 });
            s.Employees.Add(new Employee { Id = id + "_e", TypeId = "doener_meister",
                Speed = 8, Friendliness = 7, Reliability = 7, Experience = 8, SalaryPerDay = 80 });
            return s;
        }

        // ── ProductProfitBreakdown ───────────────────────────────────────────

        [Fact]
        public void EmptyWithoutShops()
        {
            Assert.Empty(ProductAnalytics.ProductProfitBreakdown(State()));
        }

        [Fact]
        public void BreaksDownPerProductSortedByProfit()
        {
            var s = State();
            s.Shops.Add(ProductiveShop());
            var breakdown = ProductAnalytics.ProductProfitBreakdown(s);
            Assert.NotEmpty(breakdown);
            // absteigend nach Gewinn
            for (var i = 1; i < breakdown.Count; i++)
                Assert.True(breakdown[i - 1].Profit >= breakdown[i].Profit);
            // jedes Produkt stammt aus dem Menü
            Assert.All(breakdown, p =>
                Assert.Contains(p.ProductId, new[] { "doener_fladen", "ayran" }));
        }

        [Fact]
        public void RevenueAndCostNonNegativeAndMarginInRange()
        {
            var s = State();
            s.Shops.Add(ProductiveShop());
            foreach (var p in ProductAnalytics.ProductProfitBreakdown(s))
            {
                Assert.True(p.Revenue >= 0);
                Assert.True(p.IngredientCost >= 0);
                Assert.True(p.Units > 0);
                Assert.InRange(p.Margin, -5.0, 1.0);
            }
        }

        [Fact]
        public void AggregatesAcrossMultipleShops()
        {
            var oneShop = State();
            oneShop.Shops.Add(ProductiveShop("a"));
            var twoShops = State();
            twoShops.Shops.Add(ProductiveShop("a"));
            twoShops.Shops.Add(ProductiveShop("b"));

            var one = ProductAnalytics.ProductProfitBreakdown(oneShop)
                .First(p => p.ProductId == "doener_fladen").Units;
            var two = ProductAnalytics.ProductProfitBreakdown(twoShops)
                .First(p => p.ProductId == "doener_fladen").Units;
            Assert.True(two > one);
        }

        // ── ShopAlerts ───────────────────────────────────────────────────────

        [Fact]
        public void NoAlertsForHealthyShop()
        {
            var s = State();
            s.Cash = 50000;
            s.Shops.Add(ProductiveShop());
            var alerts = CompanyAnalytics.ShopAlerts(s);
            // Gesunde Filiale + viel Cash → keine Verlust-/Rep-/Liquiditäts-Warnung
            Assert.DoesNotContain(alerts, a => a.Level == AlertLevel.Danger);
        }

        [Fact]
        public void LossMakingShopRaisesDangerAlert()
        {
            var s = State();
            s.Cash = 50000;
            // Kein Personal + hohe Miete + wenig Traffic → Verlust
            var shop = new Shop
            {
                Id = "loss", CityId = "fulda", IsOpen = true, Reputation = 3.0,
                FootTraffic = 300, WeeklyRent = 5000, DayOpened = 1,
                Personality = LocationPersonality.Touristic,
            };
            shop.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = 6.5 });
            s.Shops.Add(shop);
            var alerts = CompanyAnalytics.ShopAlerts(s);
            Assert.Contains(alerts, a => a.Level == AlertLevel.Danger && a.ShopId == "loss");
        }

        [Fact]
        public void BadReputationRaisesWarnAlert()
        {
            var s = State();
            s.Cash = 50000;
            var shop = ProductiveShop("badrep", footTraffic: 6000, rep: 1.5);
            s.Shops.Add(shop);
            var alerts = CompanyAnalytics.ShopAlerts(s);
            // Profitabel aber schlechter Ruf → Warn (kein Danger)
            Assert.Contains(alerts, a => a.Level == AlertLevel.Warn && a.ShopId == "badrep");
        }

        [Fact]
        public void LowLiquidityRaisesWarnAlert()
        {
            var s = State();
            s.Shops.Add(ProductiveShop());
            s.Cash = 50; // sehr wenig → unter 2× Tageskosten
            var alerts = CompanyAnalytics.ShopAlerts(s);
            Assert.Contains(alerts, a => a.Message.Contains("Liquidität"));
        }
    }
}
