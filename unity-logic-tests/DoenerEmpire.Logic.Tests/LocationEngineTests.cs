// Tests für LocationEngine — spiegeln das Verhalten der Dart-Engine
// (lib/services/location_engine.dart). Pure read-only Logik, kein RNG.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class LocationEngineTests
    {
        private static CityData City(string id)
            => GameData.AllCities.First(c => c.Id == id);

        [Fact]
        public void LocationsForKleinstadtHas4Templates()
        {
            var locs = LocationEngine.LocationsFor(City("fulda"));
            Assert.Equal(4, locs.Count);
            Assert.All(locs, l => Assert.Equal("fulda", l.Id.Split('_')[0]));
        }

        [Theory]
        [InlineData("fulda")]
        [InlineData("augsburg")]
        [InlineData("frankfurt")]
        [InlineData("berlin")]
        public void LocationsForAllTiersProduceUniqueIds(string cityId)
        {
            var locs = LocationEngine.LocationsFor(City(cityId));
            Assert.NotEmpty(locs);
            var ids = locs.Select(l => l.Id).ToHashSet();
            Assert.Equal(locs.Count, ids.Count);
        }

        [Fact]
        public void LocationsCarryPersonalitySpecificMeta()
        {
            var locs = LocationEngine.LocationsFor(City("frankfurt"));
            foreach (var l in locs)
            {
                Assert.False(string.IsNullOrWhiteSpace(l.Audience));
                Assert.False(string.IsNullOrWhiteSpace(l.Risk));
                Assert.False(string.IsNullOrWhiteSpace(l.Recommendation));
                Assert.False(string.IsNullOrWhiteSpace(l.Icon));
            }
        }

        [Fact]
        public void FindLocationMatchesByLabel()
        {
            var city = City("fulda");
            var loc = LocationEngine.FindLocation(city, "Marktplatz");
            Assert.NotNull(loc);
            Assert.Equal("Marktplatz", loc.Label);
        }

        [Fact]
        public void FindLocationReturnsNullForUnknown()
        {
            Assert.Null(LocationEngine.FindLocation(City("fulda"), "Mondbasis"));
        }

        [Fact]
        public void SummarizeEmptyHasNoPresence()
        {
            var summary = LocationEngine.Summarize(City("fulda"), new List<Shop>());
            Assert.Equal(0, summary.ShopCount);
            Assert.False(summary.HasPresence);
            Assert.Equal(0.0, summary.AvgReputation);
            Assert.Equal(0, summary.TotalFootTraffic);
            Assert.Equal(0.0, summary.WeeklyRent);
        }

        [Fact]
        public void SummarizeFiltersByCityAndAggregates()
        {
            var shops = new List<Shop>
            {
                new() { Id = "s1", CityId = "fulda", Reputation = 3.0,
                        FootTraffic = 1000, WeeklyRent = 500 },
                new() { Id = "s2", CityId = "fulda", Reputation = 4.0,
                        FootTraffic = 2000, WeeklyRent = 700 },
                new() { Id = "s3", CityId = "bayreuth", Reputation = 5.0,
                        FootTraffic = 9000, WeeklyRent = 9999 },
            };
            var summary = LocationEngine.Summarize(City("fulda"), shops);
            Assert.Equal(2, summary.ShopCount);
            Assert.True(summary.HasPresence);
            Assert.Equal(3.5, summary.AvgReputation);
            Assert.Equal(3000, summary.TotalFootTraffic);
            Assert.Equal(1200, summary.WeeklyRent);
        }

        [Fact]
        public void AttractivenessScoreInRange0To100()
        {
            var city = City("frankfurt");
            foreach (var l in LocationEngine.LocationsFor(city))
            {
                var score = l.AttractivenessScore(city);
                Assert.InRange(score, 0.0, 100.0);
            }
        }

        [Fact]
        public void HighFootTrafficScoresHigherThanLow()
        {
            var city = City("frankfurt");
            var locs = LocationEngine.LocationsFor(city);
            var byScore = locs.OrderByDescending(l => l.AttractivenessScore(city)).ToList();
            var byTraffic = locs.OrderByDescending(l => l.FootTrafficFactor).ToList();
            // Mindestens das Top-1 sollte aus den Top-2 nach Traffic stammen.
            Assert.Contains(byScore[0], new[] { byTraffic[0], byTraffic[1] });
        }

        [Fact]
        public void DepositIsTwoWeeksRent()
        {
            var city = City("fulda");
            var loc = LocationEngine.LocationsFor(city)[0];
            Assert.Equal(loc.WeeklyRentFor(city) * 2, loc.DepositFor(city));
        }

        [Fact]
        public void FootTrafficScalesWithCityBase()
        {
            var loc = LocationEngine.LocationsFor(City("fulda"))[0];
            var small = loc.FootTrafficFor(City("fulda"));
            var big = loc.FootTrafficFor(City("berlin"));
            Assert.True(big > small);
        }
    }
}
