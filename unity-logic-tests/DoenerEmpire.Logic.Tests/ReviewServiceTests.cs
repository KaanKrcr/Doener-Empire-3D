// Tests für ReviewService — prozedurale Kundenbewertungen.
// Spiegelt lib/services/review_util.dart (generateReviews) — Verhaltensvertrag.

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class ReviewServiceTests
    {
        private static GameState State() => GameState.Initial("X", "Y", 15000, tutorialEnabled: false);

        private static Shop Shop(string id, double rep, double price = 6.5)
        {
            var s = new Shop
            {
                Id = id, CityId = "fulda", IsOpen = true, Reputation = rep,
                LocationName = "Marktplatz", Name = "Sultan",
                Personality = LocationPersonality.Touristic,
            };
            s.Menu.Add(new ShopProduct { ProductId = "doener_fladen", Price = price });
            return s;
        }

        [Fact]
        public void NoReviewsWithoutShops()
        {
            Assert.Empty(ReviewService.GenerateReviews(State()));
        }

        [Fact]
        public void GeneratesRequestedCount()
        {
            var s = State();
            s.Shops.Add(Shop("a", 3.5));
            Assert.Equal(4, ReviewService.GenerateReviews(s).Count);
            Assert.Equal(7, ReviewService.GenerateReviews(s, count: 7).Count);
        }

        [Fact]
        public void StarsAlwaysBetween1And5()
        {
            var s = State();
            s.Shops.Add(Shop("a", 4.0));
            s.Shops.Add(Shop("b", 1.0));
            foreach (var r in ReviewService.GenerateReviews(s, count: 30))
                Assert.InRange(r.Stars, 1, 5);
        }

        [Fact]
        public void DeterministicPerDayAndShopCount()
        {
            var s = State();
            s.Shops.Add(Shop("a", 3.5));
            s.CurrentDay = 12;
            var first = ReviewService.GenerateReviews(s);
            var second = ReviewService.GenerateReviews(s);
            Assert.Equal(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
            {
                Assert.Equal(first[i].Stars, second[i].Stars);
                Assert.Equal(first[i].Text, second[i].Text);
                Assert.Equal(first[i].Author, second[i].Author);
            }
        }

        [Fact]
        public void DifferentDayProducesDifferentSeed()
        {
            var s = State();
            s.Shops.Add(Shop("a", 3.5));
            s.CurrentDay = 1;
            var day1 = ReviewService.GenerateReviews(s).Select(r => r.Text).ToList();
            s.CurrentDay = 2;
            var day2 = ReviewService.GenerateReviews(s).Select(r => r.Text).ToList();
            // Sehr wahrscheinlich unterschiedlich (anderer Seed)
            Assert.NotEqual(day1, day2);
        }

        [Fact]
        public void ReviewsCarryAuthorTextAndShopName()
        {
            var s = State();
            s.Shops.Add(Shop("a", 3.5));
            foreach (var r in ReviewService.GenerateReviews(s))
            {
                Assert.False(string.IsNullOrWhiteSpace(r.Author));
                Assert.False(string.IsNullOrWhiteSpace(r.Text));
                Assert.False(string.IsNullOrWhiteSpace(r.ShopName));
            }
        }

        [Fact]
        public void HighReputationTendsToHigherStarsThanLow()
        {
            var high = State();
            high.Shops.Add(Shop("hi", 5.0));
            var low = State();
            low.Shops.Add(Shop("lo", 1.0));

            var avgHigh = ReviewService.GenerateReviews(high, count: 40).Average(r => r.Stars);
            var avgLow = ReviewService.GenerateReviews(low, count: 40).Average(r => r.Stars);
            Assert.True(avgHigh > avgLow, $"high={avgHigh} low={avgLow}");
        }

        [Fact]
        public void PremiumQualityLiftsStars()
        {
            var standard = State();
            standard.Shops.Add(Shop("a", 3.0));

            var premium = State();
            premium.Shops.Add(Shop("a", 3.0));
            premium.ProductQuality["doener_fladen"] = "premium";

            var avgStd = ReviewService.GenerateReviews(standard, count: 40).Average(r => r.Stars);
            var avgPrem = ReviewService.GenerateReviews(premium, count: 40).Average(r => r.Stars);
            Assert.True(avgPrem >= avgStd, $"std={avgStd} prem={avgPrem}");
        }
    }
}
