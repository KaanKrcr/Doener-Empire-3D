// Döner Empire 3D — Start-Szenarien
// Port aus lib/models/scenario_model.dart.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;

namespace DoenerEmpire.Models
{
    public sealed class Scenario
    {
        public string Id;
        public string Name;
        public string Emoji;
        public string Description;
        public double StartCash;
        public GameDifficulty Difficulty;
        public bool TutorialEnabled;
        public double StartingLoan;
    }

    public static class ScenarioCatalog
    {
        public static readonly IReadOnlyList<Scenario> All = new List<Scenario>
        {
            new() { Id = "classic", Name = "Klassischer Start", Emoji = "🥙",
                Description = "Der normale Einstieg mit 15.000 € Startkapital.",
                StartCash = 15000, Difficulty = GameDifficulty.Normal, TutorialEnabled = true },
            new() { Id = "schuldenstart", Name = "Schuldenstart", Emoji = "💳",
                Description = "25.000 € Kapital, aber 20.000 € Bankkredit im Nacken. Schnell profitabel werden!",
                StartCash = 25000, Difficulty = GameDifficulty.Hard, TutorialEnabled = false,
                StartingLoan = 20000 },
            new() { Id = "hardcore", Name = "Hardcore", Emoji = "🔥",
                Description = "Nur 6.000 € Startkapital, harte Schwierigkeit, kein Tutorial.",
                StartCash = 6000, Difficulty = GameDifficulty.Hard, TutorialEnabled = false },
            new() { Id = "highroller", Name = "High-Roller", Emoji = "💎",
                Description = "60.000 € Startkapital — aber gnadenlose Schwierigkeit. Expandiere aggressiv.",
                StartCash = 60000, Difficulty = GameDifficulty.Impossible, TutorialEnabled = false },
        };

        public static Scenario ById(string id) => All.FirstOrDefault(s => s.Id == id);
    }
}
