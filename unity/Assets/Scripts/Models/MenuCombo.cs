// Döner Empire 3D — Menü-Angebote / Kombos
// Port aus lib/models/combo_model.dart.
//
// Konzernweit aktivierbar, wirkt aber nur in Filialen, die ALLE benötigten
// Produkte aktiv im Menü führen.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class MenuCombo
    {
        public string Id;
        public string Name;
        public string Emoji;
        public string Description;
        public List<string> ProductIds = new();
        public double CustomerBoost;
        public double AvgOrderBoost;
        public double ReputationPerDay;
        public double DailyCost;
    }

    public static class ComboData
    {
        public static readonly IReadOnlyList<MenuCombo> All = new List<MenuCombo>
        {
            new()
            {
                Id = "mittagsmenu", Name = "Mittagsmenü", Emoji = "🍱",
                Description = "Döner + Pommes + Cola zum Vorteilspreis. Zieht die "
                            + "Mittagspausen-Kundschaft an.",
                ProductIds = new() { "doener_fladen", "pommes", "cola" },
                CustomerBoost = 0.06, AvgOrderBoost = 0.10,
                ReputationPerDay = 0.004, DailyCost = 45,
            },
            new()
            {
                Id = "studenten_deal", Name = "Studenten-Deal", Emoji = "🎓",
                Description = "Günstiges Dürüm + Ayran. Studenten lieben den Preis.",
                ProductIds = new() { "doener_duerum", "ayran" },
                CustomerBoost = 0.08, AvgOrderBoost = 0.05,
                ReputationPerDay = 0.003, DailyCost = 35,
            },
            new()
            {
                Id = "familienbox", Name = "Familienbox", Emoji = "👨‍👩‍👧",
                Description = "Große Döner-Box + Pommes + Cola für die ganze Familie.",
                ProductIds = new() { "doenerbox", "pommes", "cola" },
                CustomerBoost = 0.05, AvgOrderBoost = 0.14,
                ReputationPerDay = 0.005, DailyCost = 60,
            },
            new()
            {
                Id = "veggie_kombo", Name = "Veggie-Kombo", Emoji = "🥗",
                Description = "Vegetarischer Döner + Ayran. Gesund und im Trend.",
                ProductIds = new() { "veg_doener", "ayran" },
                CustomerBoost = 0.05, AvgOrderBoost = 0.06,
                ReputationPerDay = 0.006, DailyCost = 35,
            },
            new()
            {
                Id = "snack_attacke", Name = "Snack-Attacke", Emoji = "🍟",
                Description = "Pommes + Ayran für den kleinen Hunger zwischendurch.",
                ProductIds = new() { "pommes", "ayran" },
                CustomerBoost = 0.06, AvgOrderBoost = 0.04,
                ReputationPerDay = 0.003, DailyCost = 30,
            },
            new()
            {
                Id = "doppel_doener", Name = "Doppel-Döner-Deal", Emoji = "🥙",
                Description = "Döner im Fladen + Dürüm — zwei für hungrige Gäste.",
                ProductIds = new() { "doener_fladen", "doener_duerum" },
                CustomerBoost = 0.07, AvgOrderBoost = 0.12,
                ReputationPerDay = 0.004, DailyCost = 50,
            },
        };

        public static MenuCombo ById(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }
    }
}
