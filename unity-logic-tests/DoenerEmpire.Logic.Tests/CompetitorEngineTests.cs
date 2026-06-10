// Tests für CompetitorEngine — spiegeln das Verhalten der Dart-Engine
// (lib/services/competitor_engine.dart). Deterministisch via injizierbarem RNG.

using System;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class CompetitorEngineTests
    {
        private static GameState NewState(GameDifficulty diff = GameDifficulty.Normal)
            => GameState.Initial("Test GmbH", "Tester", 15000, diff,
                tutorialEnabled: false);

        // ──────────────────────────────────────────────────────────────────────
        // EnsureCompetitorsForCity
        // ──────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("fulda", 1)]         // Klein
        [InlineData("augsburg", 2)]      // Mittel
        [InlineData("frankfurt", 3)]     // Gross
        public void EnsureSpawnsExpectedCountByTier(string cityId, int expected)
        {
            var rng = new Random(42);
            var list = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), cityId, GameDifficulty.Normal,
                rng: rng);
            Assert.Equal(expected, list.Count(c => c.CityId == cityId));
        }

        [Fact]
        public void EnsureMetropoleSpawns3to4()
        {
            var rng = new Random(1);
            var list = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "berlin", GameDifficulty.Normal,
                rng: rng);
            var count = list.Count(c => c.CityId == "berlin");
            Assert.InRange(count, 3, 4);
        }

        [Fact]
        public void EnsureIsIdempotent()
        {
            var rng = new Random(42);
            var first = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "fulda", GameDifficulty.Normal, rng: rng);
            var second = CompetitorEngine.EnsureCompetitorsForCity(
                first, "fulda", GameDifficulty.Normal, rng: rng);
            Assert.Equal(first.Count, second.Count);
        }

        [Fact]
        public void EnsureHardAddsExtraCompetitor()
        {
            // Hard: competitorAggressivenessMultiplier = 1.4 → extra = round(0.4*2)=1
            var rng = new Random(7);
            var normal = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "fulda", GameDifficulty.Normal, rng: rng);
            var hard = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "fulda", GameDifficulty.Hard, rng: rng);
            Assert.True(hard.Count > normal.Count,
                $"hard={hard.Count} normal={normal.Count}");
        }

        [Fact]
        public void EnsureImpossibleCapsExtraAt2()
        {
            // Impossible: 1.9 → extra = round(0.9*2)=2 (clamp 0..2)
            var rng = new Random(7);
            var list = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "fulda", GameDifficulty.Impossible, rng: rng);
            // Klein(1) + extra(2) = 3
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void EnsureUnknownCityFallsBackToFirstCity()
        {
            var rng = new Random(5);
            var list = CompetitorEngine.EnsureCompetitorsForCity(
                Array.Empty<Competitor>(), "made_up_city",
                GameDifficulty.Normal, rng: rng);
            // Fallback ist erste Stadt (Klein) → 1 Konkurrent
            Assert.Single(list);
            Assert.Equal("made_up_city", list[0].CityId);
        }

        // ──────────────────────────────────────────────────────────────────────
        // CompetitionPressure
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void PressureIs1WhenNoCompetitors()
        {
            var state = NewState();
            Assert.Equal(1.0, CompetitorEngine.CompetitionPressure(state, "fulda", 3.0));
        }

        [Fact]
        public void PressureDecreasesWithMoreCompetitors()
        {
            var state = NewState();
            state.Competitors.Add(new Competitor
            {
                Id = "a", CityId = "fulda", Reputation = 3.0,
                PriceLevel = 1.0, ShopCount = 1,
                Personality = CompetitorPersonality.Balanced,
            });
            var p1 = CompetitorEngine.CompetitionPressure(state, "fulda", 3.0);

            state.Competitors.Add(new Competitor
            {
                Id = "b", CityId = "fulda", Reputation = 3.0,
                PriceLevel = 1.0, ShopCount = 3,
                Personality = CompetitorPersonality.Aggressive,
            });
            var p2 = CompetitorEngine.CompetitionPressure(state, "fulda", 3.0);

            Assert.True(p2 < p1, $"p1={p1} p2={p2}");
        }

        [Fact]
        public void PressureBetterRepHelps()
        {
            var state = NewState();
            state.Competitors.Add(new Competitor
            {
                Id = "x", CityId = "fulda", Reputation = 3.0,
                PriceLevel = 1.0, ShopCount = 2,
            });
            var weak = CompetitorEngine.CompetitionPressure(state, "fulda", 2.0);
            var strong = CompetitorEngine.CompetitionPressure(state, "fulda", 4.5);
            Assert.True(strong > weak, $"weak={weak} strong={strong}");
        }

        [Fact]
        public void PressureIsClampedToRange()
        {
            var state = NewState(GameDifficulty.Impossible);
            for (var i = 0; i < 10; i++)
            {
                state.Competitors.Add(new Competitor
                {
                    Id = $"c{i}", CityId = "fulda", Reputation = 4.5,
                    PriceLevel = 0.7, ShopCount = 5,
                    Personality = CompetitorPersonality.Aggressive,
                });
            }
            var p = CompetitorEngine.CompetitionPressure(state, "fulda", 1.0);
            Assert.InRange(p, 0.55, 1.10);
        }

        // ──────────────────────────────────────────────────────────────────────
        // ProcessDay
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void ProcessDayIncrementsDaysSinceLastAction()
        {
            var state = NewState();
            state.Competitors.Add(new Competitor
            {
                Id = "a", CityId = "fulda", Reputation = 3.0, PriceLevel = 1.0,
                ShopCount = 1, Personality = CompetitorPersonality.Traditional,
                DaysSinceLastAction = 0,
            });
            // Seed so MaybeAct skipped (gate: <minDays oder Wurf > Chance).
            // Traditional+normal: minDays=5; nach 1 Day-Call → DSLA=1 < 5 → safe.
            CompetitorEngine.ProcessDay(state, new Random(1));
            Assert.Equal(1, state.Competitors[0].DaysSinceLastAction);
        }

        [Fact]
        public void ProcessDayRecomputesMarketSharesSumsLessOrEqual1()
        {
            var state = NewState();
            state.Competitors.Add(new Competitor
            {
                Id = "a", CityId = "fulda", Reputation = 3.0, PriceLevel = 1.0,
                ShopCount = 2, Personality = CompetitorPersonality.Balanced,
            });
            state.Competitors.Add(new Competitor
            {
                Id = "b", CityId = "fulda", Reputation = 4.0, PriceLevel = 1.2,
                ShopCount = 1, Personality = CompetitorPersonality.Premium,
            });
            CompetitorEngine.ProcessDay(state, new Random(2));
            var sum = state.Competitors.Sum(c => c.MarketShare);
            Assert.InRange(sum, 0.0, 1.0);
            Assert.All(state.Competitors,
                c => Assert.InRange(c.MarketShare, 0.0, 1.0));
        }

        [Fact]
        public void ProcessDayWith60DaysKeepsValuesInBounds()
        {
            var state = NewState(GameDifficulty.Hard);
            var rng = new Random(99);
            for (var i = 0; i < 4; i++)
            {
                state.Competitors.Add(new Competitor
                {
                    Id = $"c{i}", CityId = "fulda", Reputation = 3.0,
                    PriceLevel = 1.0, ShopCount = 1,
                    Personality = (CompetitorPersonality)(i % 5),
                });
            }
            for (var day = 0; day < 60; day++)
            {
                CompetitorEngine.ProcessDay(state, rng);
            }
            foreach (var c in state.Competitors)
            {
                Assert.InRange(c.Reputation, 1.0, 5.0);
                Assert.InRange(c.PriceLevel, 0.65, 1.4);
                Assert.InRange(c.ShopCount, 1, 5);
                Assert.InRange(c.MarketShare, 0.0, 1.0);
            }
        }

        [Fact]
        public void ProcessDayShopCountNeverExceeds5()
        {
            var state = NewState(GameDifficulty.Impossible);
            state.Competitors.Add(new Competitor
            {
                Id = "x", CityId = "fulda", Reputation = 3.0, PriceLevel = 0.9,
                ShopCount = 5, Personality = CompetitorPersonality.Aggressive,
                DaysSinceLastAction = 100,
            });
            var rng = new Random(3);
            for (var day = 0; day < 200; day++)
            {
                CompetitorEngine.ProcessDay(state, rng);
            }
            Assert.InRange(state.Competitors[0].ShopCount, 1, 5);
        }
    }
}
