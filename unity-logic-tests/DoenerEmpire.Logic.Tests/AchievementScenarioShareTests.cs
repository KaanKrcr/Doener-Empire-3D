// Tests für AchievementService, ScenarioCatalog und ShareService.
// Spiegelt lib/models/achievement_model.dart + game_provider._checkAchievements,
// lib/models/scenario_model.dart, lib/services/share_util.dart.

using System.Linq;
using Xunit;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;

namespace DoenerEmpire.Logic.Tests
{
    public class AchievementScenarioShareTests
    {
        private static GameState State() => GameState.Initial("Sultan Döner", "Kaan", 15000, tutorialEnabled: false);

        private static Shop Shop(string id, double rep = 3.5)
            => new() { Id = id, CityId = "fulda", Reputation = rep, IsOpen = true };

        // ── Achievements ─────────────────────────────────────────────────────

        [Fact]
        public void NoAchievementsAtFreshStart()
        {
            var s = State();
            // Tag 1, keine Shops → keine Achievements (first_shop braucht >=1 Shop,
            // first_week braucht Tag 7). first_shop NICHT erfüllt ohne Shop.
            var newOnes = AchievementService.CheckNewlyUnlocked(s);
            Assert.DoesNotContain(newOnes, a => a.Id == "first_shop");
        }

        [Fact]
        public void FirstShopUnlocksWithOneShop()
        {
            var s = State();
            s.Shops.Add(Shop("a"));
            var newOnes = AchievementService.CheckNewlyUnlocked(s);
            Assert.Contains(newOnes, a => a.Id == "first_shop");
        }

        [Fact]
        public void AlreadyOwnedNotReturnedAgain()
        {
            var s = State();
            s.Shops.Add(Shop("a"));
            s.AchievementIds.Add("first_shop");
            var newOnes = AchievementService.CheckNewlyUnlocked(s);
            Assert.DoesNotContain(newOnes, a => a.Id == "first_shop");
        }

        [Fact]
        public void ApplyAddsIdsToState()
        {
            var s = State();
            s.Shops.Add(Shop("a"));
            s.CurrentDay = 7;
            var applied = AchievementService.ApplyNewlyUnlocked(s);
            Assert.Contains("first_shop", s.AchievementIds);
            Assert.Contains("first_week", s.AchievementIds);
            // Erneut anwenden → nichts Neues
            Assert.Empty(AchievementService.ApplyNewlyUnlocked(s));
            Assert.NotEmpty(applied);
        }

        [Fact]
        public void TierThresholdsUnlockProgressively()
        {
            var s = State();
            s.Cash = 60000;       // cash_50k (Silber)
            s.TotalRevenue = 0;
            s.Brand.BrandAwareness = 45;  // brand_40 (Gold)
            s.Shops.Add(Shop("a", rep: 5.0)); // rep_45 + five_star_shop
            var ids = AchievementService.CheckNewlyUnlocked(s).Select(a => a.Id).ToHashSet();
            Assert.Contains("cash_50k", ids);
            Assert.Contains("brand_40", ids);
            Assert.Contains("rep_45", ids);
            Assert.Contains("five_star_shop", ids);
            // Platin-Schwellen noch nicht erreicht
            Assert.DoesNotContain("cash_500k", ids);
            Assert.DoesNotContain("brand_80", ids);
        }

        [Fact]
        public void TotalPointsSumsTiers()
        {
            var s = State();
            s.AchievementIds.Add("first_shop");   // Bronze 10
            s.AchievementIds.Add("cash_50k");     // Silber 25
            s.AchievementIds.Add("ten_shops");    // Gold 50
            s.AchievementIds.Add("million_revenue"); // Platin 100
            Assert.Equal(185, AchievementService.TotalPoints(s));
        }

        [Fact]
        public void CatalogByIdResolves()
        {
            Assert.NotNull(AchievementCatalog.ById("twenty_shops"));
            Assert.Null(AchievementCatalog.ById("does_not_exist"));
        }

        // ── Scenarios ────────────────────────────────────────────────────────

        [Fact]
        public void ClassicScenarioIsNeutralStart()
        {
            var c = ScenarioCatalog.ById("classic");
            Assert.NotNull(c);
            Assert.Equal(15000, c.StartCash);
            Assert.Equal(GameDifficulty.Normal, c.Difficulty);
            Assert.True(c.TutorialEnabled);
            Assert.Equal(0, c.StartingLoan);
        }

        [Fact]
        public void SchuldenstartHasStartingLoan()
        {
            var c = ScenarioCatalog.ById("schuldenstart");
            Assert.Equal(20000, c.StartingLoan);
            Assert.Equal(GameDifficulty.Hard, c.Difficulty);
        }

        [Fact]
        public void AllScenariosWellFormed()
        {
            Assert.Equal(4, ScenarioCatalog.All.Count);
            Assert.All(ScenarioCatalog.All, s =>
            {
                Assert.False(string.IsNullOrEmpty(s.Id));
                Assert.False(string.IsNullOrEmpty(s.Name));
                Assert.True(s.StartCash > 0);
                Assert.True(s.StartingLoan >= 0);
            });
            // IDs eindeutig
            Assert.Equal(ScenarioCatalog.All.Count,
                ScenarioCatalog.All.Select(s => s.Id).Distinct().Count());
        }

        // ── Share ────────────────────────────────────────────────────────────

        [Fact]
        public void EmpireSummaryContainsCompanyDayAndHashtag()
        {
            var s = State();
            s.CurrentDay = 12;
            s.Shops.Add(Shop("a"));
            var text = ShareService.EmpireSummaryText(s);
            Assert.Contains("Sultan Döner", text);
            Assert.Contains("Tag 12", text);
            Assert.Contains("#DönerEmpire", text);
            Assert.Contains("Filialen", text);
            Assert.Contains("Trophäen", text);
        }

        [Fact]
        public void EmpireSummaryShowsTrophyAndChapterCounts()
        {
            var s = State();
            s.AchievementIds.Add("first_shop");
            s.CompletedChapterIds.Add("ch1_traum");
            var text = ShareService.EmpireSummaryText(s);
            // 1 von N Trophäen, 1 von N Kapiteln
            Assert.Contains($"1/{AchievementCatalog.All.Count}", text);
            Assert.Contains($"1/{CampaignData.Chapters.Count}", text);
        }
    }
}
