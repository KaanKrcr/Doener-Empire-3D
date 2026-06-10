// Tests für BrandThemeCatalog + TutorialInfo.
// Spiegelt lib/models/branding_model.dart + tutorial_model.dart.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class BrandThemeTutorialTests
    {
        // ── BrandTheme ───────────────────────────────────────────────────────

        [Fact]
        public void KlassikIsAlwaysUnlocked()
        {
            var klassik = BrandThemeCatalog.ById("klassik");
            Assert.Null(klassik.UnlockAchievementId);
            Assert.True(klassik.Unlocked(new HashSet<string>()));
        }

        [Fact]
        public void LockedThemeRequiresAchievement()
        {
            var gold = BrandThemeCatalog.ById("gold"); // braucht cash_250k
            Assert.False(gold.Unlocked(new HashSet<string>()));
            Assert.True(gold.Unlocked(new HashSet<string> { "cash_250k" }));
        }

        [Fact]
        public void UnknownThemeFallsBackToKlassik()
        {
            Assert.Equal("klassik", BrandThemeCatalog.ById("does_not_exist").Id);
        }

        [Fact]
        public void AllThemesWellFormedWithUniqueIds()
        {
            Assert.Equal(7, BrandThemeCatalog.All.Count);
            Assert.All(BrandThemeCatalog.All, t =>
            {
                Assert.False(string.IsNullOrEmpty(t.Id));
                Assert.False(string.IsNullOrEmpty(t.Name));
                Assert.NotEqual(0u, t.Accent);     // ARGB gesetzt
                Assert.NotEqual(0u, t.AccentDark);
            });
            Assert.Equal(BrandThemeCatalog.All.Count,
                BrandThemeCatalog.All.Select(t => t.Id).Distinct().Count());
        }

        [Fact]
        public void EveryUnlockReferencesRealAchievement()
        {
            foreach (var t in BrandThemeCatalog.All)
            {
                if (t.UnlockAchievementId == null) continue;
                Assert.NotNull(AchievementCatalog.ById(t.UnlockAchievementId));
            }
        }

        // ── Tutorial ─────────────────────────────────────────────────────────

        [Fact]
        public void StepCountMatchesEnum()
        {
            Assert.Equal(TutorialInfo.StepCount,
                System.Enum.GetValues(typeof(TutorialStep)).Length);
        }

        [Fact]
        public void FromIndexClampsBounds()
        {
            Assert.Equal(TutorialStep.OpenFirstShop, TutorialInfo.FromIndex(-3));
            Assert.Equal(TutorialStep.OpenFirstShop, TutorialInfo.FromIndex(0));
            Assert.Equal(TutorialStep.ChangeProductPrice, TutorialInfo.FromIndex(2));
            Assert.Equal(TutorialStep.FinishTutorial, TutorialInfo.FromIndex(999));
        }

        [Fact]
        public void EveryStepHasTitleDescriptionHintAndWhy()
        {
            foreach (TutorialStep s in System.Enum.GetValues(typeof(TutorialStep)))
            {
                Assert.False(string.IsNullOrWhiteSpace(TutorialInfo.Title(s)));
                Assert.False(string.IsNullOrWhiteSpace(TutorialInfo.Description(s)));
                Assert.False(string.IsNullOrWhiteSpace(TutorialInfo.Hint(s)));
                Assert.False(string.IsNullOrWhiteSpace(TutorialInfo.WhyItMatters(s)));
            }
        }

        [Fact]
        public void TargetTabIndexMatchesDartMapping()
        {
            Assert.Equal(1, TutorialInfo.TargetTabIndex(TutorialStep.OpenFirstShop));
            Assert.Equal(0, TutorialInfo.TargetTabIndex(TutorialStep.EndFirstDay));
            Assert.Equal(2, TutorialInfo.TargetTabIndex(TutorialStep.OpenEmpireMenu));
            Assert.Equal(3, TutorialInfo.TargetTabIndex(TutorialStep.UnderstandHrCompetitionGrowth));
            Assert.Null(TutorialInfo.TargetTabIndex(TutorialStep.FinishTutorial));
        }

        [Fact]
        public void ActionLabelOnlyForConfirmSteps()
        {
            Assert.Equal("Tutorial beenden", TutorialInfo.ActionLabel(TutorialStep.FinishTutorial));
            Assert.Equal("Weiter", TutorialInfo.ActionLabel(TutorialStep.UnderstandLocationValues));
            // Schritte ohne explizite Bestätigung → null
            Assert.Null(TutorialInfo.ActionLabel(TutorialStep.OpenFirstShop));
            Assert.Null(TutorialInfo.ActionLabel(TutorialStep.HireFirstEmployee));
        }
    }
}
