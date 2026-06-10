// Döner Empire 3D — Story-Kampagne (Campaign-Engine)
// Port aus lib/services/campaign_engine.dart.
//
// Nutzt MissionEngine.CurrentValueFor wieder, damit Kapitelziele dieselbe
// Bedingungslogik verwenden wie Missionen.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class CampaignCheckResult
    {
        public GameState State;
        public CampaignChapter JustCompleted;
    }

    public static class CampaignEngine
    {
        /// <summary>
        /// Erstes nicht abgeschlossenes Kapitel oder null, wenn die Kampagne
        /// komplett durchgespielt ist.
        /// </summary>
        public static CampaignChapter ActiveChapter(GameState state)
        {
            foreach (var c in CampaignData.Chapters)
                if (!state.CompletedChapterIds.Contains(c.Id)) return c;
            return null;
        }

        public static int CompletedCount(GameState state)
            => CampaignData.Chapters.Count(c => state.CompletedChapterIds.Contains(c.Id));

        public static bool IsComplete(GameState state) => ActiveChapter(state) == null;

        /// <summary>Aktueller Messwert eines Ziels (z.B. Anzahl Filialen).</summary>
        public static double ObjectiveCurrent(CampaignObjective obj, GameState state)
        {
            var probe = new Mission
            {
                Id = obj.SpecialId ?? "campaign",
                Title = "", Description = "", Emoji = "",
                CashReward = 0,
                Type = obj.Type,
                Target = obj.Target,
            };
            return MissionEngine.CurrentValueFor(probe, state);
        }

        public static bool ObjectiveDone(CampaignObjective obj, GameState state)
            => ObjectiveCurrent(obj, state) >= obj.Target;

        /// <summary>Fortschritt eines einzelnen Ziels (0..1).</summary>
        public static double ObjectiveProgress(CampaignObjective obj, GameState state)
        {
            if (obj.Target <= 0) return 1.0;
            return System.Math.Clamp(ObjectiveCurrent(obj, state) / obj.Target, 0.0, 1.0);
        }

        /// <summary>Gesamt-Fortschritt eines Kapitels (Mittel der Ziel-Fortschritte, 0..1).</summary>
        public static double ChapterProgress(CampaignChapter chapter, GameState state)
        {
            if (chapter.Objectives.Count == 0) return 1.0;
            var sum = chapter.Objectives.Sum(o => ObjectiveProgress(o, state));
            return System.Math.Clamp(sum / chapter.Objectives.Count, 0.0, 1.0);
        }

        public static bool IsChapterComplete(CampaignChapter chapter, GameState state)
            => chapter.Objectives.All(o => ObjectiveDone(o, state));

        /// <summary>
        /// Prüft das aktive Kapitel; ist es erfüllt, wird es als abgeschlossen
        /// markiert, die Cash-Belohnung gutgeschrieben und das Kapitel
        /// zurückgegeben (für die Abschluss-Feier).
        /// </summary>
        public static CampaignCheckResult CheckAndApply(GameState state)
        {
            var chapter = ActiveChapter(state);
            if (chapter == null) return new CampaignCheckResult { State = state, JustCompleted = null };
            if (!IsChapterComplete(chapter, state))
                return new CampaignCheckResult { State = state, JustCompleted = null };

            state.Cash += chapter.CashReward;
            state.CompletedChapterIds.Add(chapter.Id);
            return new CampaignCheckResult { State = state, JustCompleted = chapter };
        }
    }
}
