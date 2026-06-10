// Tests für CampaignEngine — spiegeln das Verhalten der Dart-Engine
// (lib/services/campaign_engine.dart).

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class CampaignEngineTests
    {
        private static GameState NewState()
            => GameState.Initial("Test GmbH", "Tester", 1000, GameDifficulty.Normal,
                tutorialEnabled: false);

        [Fact]
        public void ActiveChapterIsFirstAtStart()
        {
            var state = NewState();
            var ch = CampaignEngine.ActiveChapter(state);
            Assert.NotNull(ch);
            Assert.Equal("ch1_traum", ch.Id);
        }

        [Fact]
        public void ActiveChapterReturnsNullWhenAllCompleted()
        {
            var state = NewState();
            foreach (var c in CampaignData.Chapters)
                state.CompletedChapterIds.Add(c.Id);
            Assert.Null(CampaignEngine.ActiveChapter(state));
            Assert.True(CampaignEngine.IsComplete(state));
        }

        [Fact]
        public void CompletedCountTracksFlags()
        {
            var state = NewState();
            state.CompletedChapterIds.Add("ch1_traum");
            state.CompletedChapterIds.Add("ch2_stammkunden");
            Assert.Equal(2, CampaignEngine.CompletedCount(state));
        }

        [Fact]
        public void ObjectiveProgressClampsTo1WhenOverachieved()
        {
            var state = NewState();
            for (var i = 0; i < 10; i++)
                state.Shops.Add(new Shop { Id = $"s{i}", CityId = "fulda" });
            var ch3 = CampaignData.ById("ch3_expansion");
            var shopObj = ch3.Objectives.First(o => o.Type == MissionType.ShopCount);
            Assert.Equal(1.0, CampaignEngine.ObjectiveProgress(shopObj, state));
        }

        [Fact]
        public void ChapterProgressAveragesObjectives()
        {
            var state = NewState();
            // ch2: 2 Ziele — hireEmployees(2) + reputation(3.5)
            // Wir setzen 1 Mitarbeiter (50% Hire-Goal) und 0 Reputation
            state.Shops.Add(new Shop
            {
                Id = "s1", CityId = "fulda", Reputation = 0,
                Employees = new System.Collections.Generic.List<Employee> { new() },
            });
            var ch2 = CampaignData.ById("ch2_stammkunden");
            var p = CampaignEngine.ChapterProgress(ch2, state);
            // (0.5 + 0) / 2 = 0.25
            Assert.Equal(0.25, p, precision: 5);
        }

        [Fact]
        public void IsChapterCompleteOnlyWhenAllObjectivesDone()
        {
            var state = NewState();
            var ch1 = CampaignData.ById("ch1_traum");
            Assert.False(CampaignEngine.IsChapterComplete(ch1, state));
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });
            Assert.True(CampaignEngine.IsChapterComplete(ch1, state));
        }

        [Fact]
        public void CheckAndApplyCompletesChapterAndPaysReward()
        {
            var state = NewState();
            var startCash = state.Cash;
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });

            var res = CampaignEngine.CheckAndApply(state);
            Assert.NotNull(res.JustCompleted);
            Assert.Equal("ch1_traum", res.JustCompleted.Id);
            Assert.Contains("ch1_traum", state.CompletedChapterIds);
            Assert.Equal(startCash + 1500, state.Cash);
        }

        [Fact]
        public void CheckAndApplyReturnsNullWhenIncomplete()
        {
            var state = NewState();
            var res = CampaignEngine.CheckAndApply(state);
            Assert.Null(res.JustCompleted);
            Assert.Empty(state.CompletedChapterIds);
        }

        [Fact]
        public void CheckAndApplyDoesNothingWhenCampaignDone()
        {
            var state = NewState();
            foreach (var c in CampaignData.Chapters)
                state.CompletedChapterIds.Add(c.Id);
            var cash = state.Cash;
            var res = CampaignEngine.CheckAndApply(state);
            Assert.Null(res.JustCompleted);
            Assert.Equal(cash, state.Cash);
        }

        [Fact]
        public void MetropoleObjectiveRecognisesSpecialId()
        {
            var state = NewState();
            // fulda (Klein) erfüllt das Metropolen-Ziel nicht
            state.Shops.Add(new Shop { Id = "s1", CityId = "fulda" });
            var metroObj = CampaignData.ById("ch5_grossstadt").Objectives
                .First(o => o.SpecialId == "metropole");
            Assert.False(CampaignEngine.ObjectiveDone(metroObj, state));

            // berlin (Metropole) erfüllt
            state.Shops.Add(new Shop { Id = "s2", CityId = "berlin" });
            Assert.True(CampaignEngine.ObjectiveDone(metroObj, state));
        }

        [Fact]
        public void AggregatePerksSumsCompletedChapters()
        {
            var perks = CampaignData.AggregatePerks(new[]
            {
                "ch1_traum",          // CustomerBoost 0.03
                "ch2_stammkunden",    // AvgOrderBoost 0.04
                "ch3_expansion",      // IngredientSaving 0.04
                "ch6_boerse",         // RentSaving 0.05
            });
            Assert.Equal(0.03, perks.CustomerBoost, precision: 5);
            Assert.Equal(0.04, perks.AvgOrderBoost, precision: 5);
            Assert.Equal(0.04, perks.IngredientSaving, precision: 5);
            Assert.Equal(0.05, perks.RentSaving, precision: 5);
        }

        [Fact]
        public void AggregatePerksEmptyReturnsZero()
        {
            var perks = CampaignData.AggregatePerks(new string[0]);
            Assert.Equal(0, perks.CustomerBoost);
            Assert.Equal(0, perks.AvgOrderBoost);
            Assert.Equal(0, perks.IngredientSaving);
            Assert.Equal(0, perks.RentSaving);
        }

        [Fact]
        public void PerkEffectLabelFormatsActivePartsOnly()
        {
            var p = new CampaignPerk { CustomerBoost = 0.05, IngredientSaving = 0.03 };
            var label = p.EffectLabel;
            Assert.Contains("+5% Kunden", label);
            Assert.Contains("−3% Zutaten", label);
            Assert.DoesNotContain("Bestellwert", label);
            Assert.DoesNotContain("Miete", label);
        }
    }
}
