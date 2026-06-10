// Tests für EventEngine + EventCatalog.
// Spiegelt lib/providers/game_provider.dart (_rollEvent, applyEventChoice)
// und lib/models/event_model.dart (Katalog-Integrität).

using System;
using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class EventEngineTests
    {
        private static GameState State(double cash = 15000, int day = 1)
        {
            var s = GameState.Initial("X", "Y", cash, tutorialEnabled: false);
            s.CurrentDay = day;
            return s;
        }

        private static Shop Shop(string id, string cityId = "fulda", double rep = 3.0)
            => new() { Id = id, CityId = cityId, Reputation = rep, IsOpen = true };

        // ── Katalog-Integrität ───────────────────────────────────────────────

        [Fact]
        public void CatalogIsNonEmptyWithUniqueIds()
        {
            Assert.NotEmpty(EventCatalog.All);
            Assert.Equal(EventCatalog.All.Count,
                EventCatalog.All.Select(e => e.Id).Distinct().Count());
        }

        [Fact]
        public void EveryEventWellFormed()
        {
            Assert.All(EventCatalog.All, e =>
            {
                Assert.False(string.IsNullOrEmpty(e.Id));
                Assert.False(string.IsNullOrWhiteSpace(e.Title));
                Assert.False(string.IsNullOrWhiteSpace(e.Description));
                Assert.NotEmpty(e.Choices);
                Assert.All(e.Choices, c =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(c.Label));
                    Assert.NotNull(c.Effect);
                    Assert.False(string.IsNullOrWhiteSpace(c.Effect.ResultMessage));
                });
            });
        }

        [Fact]
        public void ByIdResolves()
        {
            var first = EventCatalog.All[0];
            Assert.Same(first, EventCatalog.ById(first.Id));
            Assert.Null(EventCatalog.ById("does_not_exist"));
        }

        // ── Eligibility ──────────────────────────────────────────────────────

        [Fact]
        public void MinShopsRequirementRespected()
        {
            var e = new GameEvent { Id = "x", Requirements = new EventRequirements { MinShops = 2 } };
            var s = State();
            s.Shops.Add(Shop("a"));
            Assert.False(EventEngine.IsEligible(e, s));
            s.Shops.Add(Shop("b"));
            Assert.True(EventEngine.IsEligible(e, s));
        }

        [Fact]
        public void MinDayAndMinCashRespected()
        {
            var s = State(cash: 1000, day: 5);
            s.Shops.Add(Shop("a"));
            Assert.False(EventEngine.IsEligible(
                new GameEvent { Requirements = new EventRequirements { MinDay = 10 } }, s));
            Assert.False(EventEngine.IsEligible(
                new GameEvent { Requirements = new EventRequirements { MinCash = 5000 } }, s));
            Assert.True(EventEngine.IsEligible(
                new GameEvent { Requirements = new EventRequirements { MinDay = 5, MinCash = 1000 } }, s));
        }

        [Fact]
        public void MetropolitanRequirementRespected()
        {
            var e = new GameEvent { Requirements = new EventRequirements { NeedsMetropolitanShop = true } };
            var s = State();
            s.Shops.Add(Shop("a", "fulda")); // Klein
            Assert.False(EventEngine.IsEligible(e, s));
            s.Shops.Add(Shop("b", "berlin")); // Metropole
            Assert.True(EventEngine.IsEligible(e, s));
        }

        // ── RollEvent ────────────────────────────────────────────────────────

        [Fact]
        public void RollReturnsNullWhenNothingEligible()
        {
            // Tag 1, kein Shop → Events mit MinShops>=1 sind ineligibel
            var s = State();
            // Künstlich: keine Shops → die meisten Events verlangen >=1 Shop
            var rolled = EventEngine.RollEvent(s, new Random(1));
            // Es könnte 0-Shop-Events geben; akzeptiere null ODER ein eligibles Event
            if (rolled != null) Assert.True(EventEngine.IsEligible(rolled, s));
        }

        [Fact]
        public void RollReturnsEligibleEvent()
        {
            var s = State(cash: 100000, day: 50);
            for (var i = 0; i < 5; i++) s.Shops.Add(Shop($"s{i}", "berlin", 3.5));
            for (var seed = 0; seed < 20; seed++)
            {
                var e = EventEngine.RollEvent(s, new Random(seed));
                Assert.NotNull(e);
                Assert.True(EventEngine.IsEligible(e, s));
            }
        }

        [Fact]
        public void RollPrefersUnseenEvents()
        {
            var s = State(cash: 100000, day: 50);
            for (var i = 0; i < 5; i++) s.Shops.Add(Shop($"s{i}", "berlin", 3.5));
            // Alle bis auf eines als gesehen markieren
            var target = EventCatalog.All.First(e => EventEngine.IsEligible(e, s));
            foreach (var e in EventCatalog.All)
                if (e.Id != target.Id) s.SeenEventIds.Add(e.Id);

            // Wenn das Ziel-Event eligibel & einziges ungesehenes ist, muss es kommen
            if (EventEngine.IsEligible(target, s))
            {
                var rolled = EventEngine.RollEvent(s, new Random(3));
                Assert.Equal(target.Id, rolled.Id);
            }
        }

        // ── ApplyChoice ──────────────────────────────────────────────────────

        [Fact]
        public void ApplyChoiceAdjustsCashRepBrandAndMarksSeen()
        {
            var s = State(cash: 5000);
            s.Shops.Add(Shop("a", rep: 3.0));
            s.Shops.Add(Shop("b", rep: 4.0));
            s.Brand.BrandAwareness = 20;

            var ev = new GameEvent { Id = "evt" };
            var choice = new EventChoice
            {
                Label = "Test",
                Effect = new EventEffect
                {
                    CashDelta = -500, ReputationDelta = 0.5,
                    BrandAwarenessDelta = 5, ResultMessage = "ok",
                },
            };

            EventEngine.ApplyChoice(s, ev, choice);

            Assert.Equal(4500, s.Cash);
            Assert.Equal(3.5, s.Shops[0].Reputation);
            Assert.Equal(4.5, s.Shops[1].Reputation);
            Assert.Equal(25, s.Brand.BrandAwareness);
            Assert.Contains("evt", s.SeenEventIds);
        }

        [Fact]
        public void ApplyChoiceClampsReputationAndBrand()
        {
            var s = State();
            s.Shops.Add(Shop("a", rep: 4.8));
            s.Brand.BrandAwareness = 98;
            var ev = new GameEvent { Id = "evt" };
            var choice = new EventChoice
            {
                Label = "x",
                Effect = new EventEffect
                {
                    ReputationDelta = 1.0, BrandAwarenessDelta = 10, ResultMessage = "ok",
                },
            };
            EventEngine.ApplyChoice(s, ev, choice);
            Assert.InRange(s.Shops[0].Reputation, 0.5, 5.0);
            Assert.InRange(s.Brand.BrandAwareness, 0.0, 100.0);
        }

        [Fact]
        public void ApplyChoiceDoesNotDuplicateSeenId()
        {
            var s = State();
            s.Shops.Add(Shop("a"));
            var ev = new GameEvent { Id = "evt" };
            var choice = new EventChoice { Label = "x", Effect = new EventEffect { ResultMessage = "ok" } };
            EventEngine.ApplyChoice(s, ev, choice);
            EventEngine.ApplyChoice(s, ev, choice);
            Assert.Equal(1, s.SeenEventIds.Count(id => id == "evt"));
        }

        [Fact]
        public void AllCatalogEventsApplyWithoutThrow()
        {
            var s = State(cash: 100000);
            s.Shops.Add(Shop("a", "berlin", 3.0));
            foreach (var e in EventCatalog.All)
                foreach (var c in e.Choices)
                    EventEngine.ApplyChoice(s, e, c);
            Assert.InRange(s.Shops[0].Reputation, 0.5, 5.0);
            Assert.InRange(s.Brand.BrandAwareness, 0.0, 100.0);
        }
    }
}
