// Döner Empire 3D — Story-Kampagne (Kapitel + Perks)
// Port aus lib/models/campaign_model.dart.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class CampaignPerk
    {
        public string Title;
        public string Emoji;
        public double CustomerBoost;      // 0.05 = +5% Kunden
        public double AvgOrderBoost;      // 0.05 = +5% Bestellwert
        public double IngredientSaving;   // 0.05 = -5% Zutaten
        public double RentSaving;         // 0.05 = -5% Miete

        public string EffectLabel
        {
            get
            {
                var parts = new List<string>();
                if (CustomerBoost > 0) parts.Add($"+{(int)System.Math.Round(CustomerBoost * 100)}% Kunden");
                if (AvgOrderBoost > 0) parts.Add($"+{(int)System.Math.Round(AvgOrderBoost * 100)}% Bestellwert");
                if (IngredientSaving > 0) parts.Add($"−{(int)System.Math.Round(IngredientSaving * 100)}% Zutaten");
                if (RentSaving > 0) parts.Add($"−{(int)System.Math.Round(RentSaving * 100)}% Miete");
                return string.Join(" · ", parts);
            }
        }
    }

    public sealed class CampaignObjective
    {
        public MissionType Type;
        public double Target;
        public string Label;
        /// <summary>Optionaler Spezial-Marker (z.B. "metropole" für Metropolen-Filiale).</summary>
        public string SpecialId;
    }

    public sealed class CampaignChapter
    {
        public string Id;
        public int Number;
        public string Title;
        public string Story;
        public string Emoji;
        public List<CampaignObjective> Objectives = new();
        public double CashReward;
        public string RewardLabel;
        public CampaignPerk Perk;
    }

    public static class CampaignData
    {
        /// <summary>Story-Kampagne in Reihenfolge. Kapitel N wird erst aktiv, wenn N-1 abgeschlossen ist.</summary>
        public static readonly IReadOnlyList<CampaignChapter> Chapters = new List<CampaignChapter>
        {
            new()
            {
                Id = "ch1_traum", Number = 1, Title = "Der Traum vom eigenen Spieß",
                Story = "Jahrelang hast du in fremden Läden gestanden und zugesehen, wie andere "
                      + "das große Geld machten. Heute ist Schluss damit. Mit deinen Ersparnissen "
                      + "eröffnest du deinen ersten eigenen Döner-Imbiss. Der erste Schritt zu "
                      + "deinem Imperium.",
                Emoji = "🌱",
                Objectives = new()
                {
                    new() { Type = MissionType.OpenFirstShop, Target = 1,
                            Label = "Eröffne deine erste Filiale" },
                },
                CashReward = 1500, RewardLabel = "Startkapital für die Zukunft",
                Perk = new() { Title = "Gründer-Instinkt", Emoji = "🔥", CustomerBoost = 0.03 },
            },
            new()
            {
                Id = "ch2_stammkunden", Number = 2, Title = "Erste Stammkunden",
                Story = "Die Leute reden über deinen Laden. Doch allein schaffst du den Ansturm "
                      + "nicht. Es wird Zeit, ein Team aufzubauen und dir einen guten Ruf in der "
                      + "Stadt zu erarbeiten.",
                Emoji = "🤝",
                Objectives = new()
                {
                    new() { Type = MissionType.HireEmployees, Target = 2, Label = "Stelle 2 Mitarbeiter ein" },
                    new() { Type = MissionType.ReputationLevel, Target = 3.5, Label = "Erreiche 3,5 ⭐ Reputation" },
                },
                CashReward = 3500, RewardLabel = "Dein Name hat jetzt Gewicht",
                Perk = new() { Title = "Treue Stammkunden", Emoji = "❤️", AvgOrderBoost = 0.04 },
            },
            new()
            {
                Id = "ch3_expansion", Number = 3, Title = "Über die Stadtgrenze",
                Story = "Eine Filiale reicht dir nicht mehr. Du witterst Chancen in anderen "
                      + "Städten. Bau eine kleine Kette auf und erschließe einen neuen Markt.",
                Emoji = "🗺️",
                Objectives = new()
                {
                    new() { Type = MissionType.ShopCount, Target = 3, Label = "Betreibe 3 Filialen" },
                    new() { Type = MissionType.UnlockCity, Target = 1, Label = "Schalte eine neue Stadt frei" },
                },
                CashReward = 7000, RewardLabel = "Expansion in vollem Gange",
                Perk = new() { Title = "Mengenrabatt", Emoji = "📦", IngredientSaving = 0.04 },
            },
            new()
            {
                Id = "ch4_marke", Number = 4, Title = "Eine Marke entsteht",
                Story = "Aus dem Imbiss wird ein Unternehmen. Filiale für Filiale wächst dein "
                      + "Einfluss — und dein Kontostand. Zeit, richtig Kapital aufzubauen.",
                Emoji = "🏷️",
                Objectives = new()
                {
                    new() { Type = MissionType.ReachCash, Target = 50000, Label = "Erreiche 50.000 € Kapital" },
                    new() { Type = MissionType.ShopCount, Target = 5, Label = "Betreibe 5 Filialen" },
                },
                CashReward = 15000, RewardLabel = "Eine echte Marke",
                Perk = new() { Title = "Markenzug", Emoji = "🧲", CustomerBoost = 0.05 },
            },
            new()
            {
                Id = "ch5_grossstadt", Number = 5, Title = "Der Sprung in die Großstadt",
                Story = "Die wahre Bühne sind die Metropolen. Teure Mieten, harte Konkurrenz — "
                      + "aber auch gewaltige Laufkundschaft. Wer es hier schafft, schafft es überall.",
                Emoji = "🌆",
                Objectives = new()
                {
                    new() { Type = MissionType.ShopCount, Target = 1,
                            Label = "Eröffne eine Filiale in einer Metropole", SpecialId = "metropole" },
                    new() { Type = MissionType.ReputationLevel, Target = 4.2, Label = "Erreiche 4,2 ⭐ Reputation" },
                },
                CashReward = 30000, RewardLabel = "Angekommen in der ersten Liga",
                Perk = new() { Title = "Premium-Lagen", Emoji = "💎", AvgOrderBoost = 0.05 },
            },
            new()
            {
                Id = "ch6_boerse", Number = 6, Title = "Börsen-Legende",
                Story = "Dein Unternehmen ist zu groß für deine Hosentasche. Investoren klopfen "
                      + "an. Wag den Börsengang und mach deine Marke deutschlandweit bekannt — "
                      + "jetzt spielst du in der Liga der Großen.",
                Emoji = "📈",
                Objectives = new()
                {
                    new() { Type = MissionType.CompanyPublic, Target = 1, Label = "Führe den Börsengang (IPO) durch" },
                    new() { Type = MissionType.BrandAwareness, Target = 40, Label = "Erreiche 40 Markenbekanntheit" },
                },
                CashReward = 40000, RewardLabel = "An der Börse notiert",
                Perk = new() { Title = "Verhandlungsmacht", Emoji = "🤵", RentSaving = 0.05 },
            },
            new()
            {
                Id = "ch7_markt", Number = 7, Title = "Marktbeherrschung",
                Story = "Konkurrenz? Die kaufst du einfach auf. Übernimm rivalisierende Läden "
                      + "und festige deine Vormachtstellung, bis dir niemand mehr das Wasser "
                      + "reichen kann.",
                Emoji = "🤝",
                Objectives = new()
                {
                    new() { Type = MissionType.AcquiredShops, Target = 2, Label = "Übernimm 2 Konkurrenz-Filialen" },
                    new() { Type = MissionType.ReachCash, Target = 120000, Label = "Erreiche 120.000 € Kapital" },
                },
                CashReward = 50000, RewardLabel = "Unangefochtener Marktführer",
                Perk = new() { Title = "Lieferantenmacht", Emoji = "🚚", IngredientSaving = 0.06 },
            },
            new()
            {
                // Bewusst gleicher Dart-Bug-Style: id="ch6_imperium" obwohl Nummer=8.
                // Save-Kompat: ID wird so persistiert wie im Dart-Master.
                Id = "ch6_imperium", Number = 8, Title = "Döner-Imperium",
                Story = "Vom kleinen Spieß zum landesweiten Imperium. Dein Name steht für Döner "
                      + "in ganz Deutschland. Krön dein Lebenswerk und schreib Geschichte.",
                Emoji = "👑",
                Objectives = new()
                {
                    new() { Type = MissionType.ReachCash, Target = 150000, Label = "Erreiche 150.000 € Kapital" },
                    new() { Type = MissionType.ShopCount, Target = 8, Label = "Betreibe 8 Filialen" },
                    new() { Type = MissionType.DaysSurvived, Target = 40, Label = "Überlebe 40 Tage" },
                },
                CashReward = 75000, RewardLabel = "Döner-Legende 👑",
                Perk = new() { Title = "Legendäre Marke", Emoji = "👑", CustomerBoost = 0.08 },
            },
        };

        public static CampaignChapter ById(string id)
        {
            foreach (var c in Chapters) if (c.Id == id) return c;
            return null;
        }

        /// <summary>Summiert Perks aller abgeschlossenen Kapitel zu einem konzernweiten Gesamt-Bonus.</summary>
        public static CampaignPerk AggregatePerks(IEnumerable<string> completedChapterIds)
        {
            double cust = 0, aov = 0, ing = 0, rent = 0;
            foreach (var id in completedChapterIds)
            {
                var perk = ById(id)?.Perk;
                if (perk == null) continue;
                cust += perk.CustomerBoost;
                aov += perk.AvgOrderBoost;
                ing += perk.IngredientSaving;
                rent += perk.RentSaving;
            }
            return new CampaignPerk
            {
                Title = "Kampagnen-Boni", Emoji = "⭐",
                CustomerBoost = cust, AvgOrderBoost = aov,
                IngredientSaving = ing, RentSaving = rent,
            };
        }
    }
}
