// Döner Empire 3D — Mission/Quest
// Port aus lib/models/mission_model.dart.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public enum MissionType
    {
        OpenFirstShop,
        TotalRevenue,
        HireEmployees,
        BuyEquipment,
        UnlockProduct,
        ReachCash,
        ShopCount,
        UnlockCity,
        DaysSurvived,
        ReputationLevel,
        CompanyPublic,   // Börsengang (stocks.isPublic) — Stocks-Port pending
        BrandAwareness,
        AcquiredShops,
    }

    public sealed class Mission
    {
        public string Id;
        public string Title;
        public string Description;
        public string Emoji;
        public double CashReward;
        public MissionType Type;
        public double Target;
        public bool IsDone;
    }

    public static class MissionTemplates
    {
        /// <summary>Frische Mission-Liste in Reihenfolge (entspricht Dart-buildMissionsTemplate).</summary>
        public static List<Mission> Build() => new()
        {
            new Mission { Id = "open_first_shop", Title = "Erster Imbiss",
                Description = "Eröffne deinen ersten Döner-Imbiss in einer Stadt.",
                Emoji = "🏪", CashReward = 500,
                Type = MissionType.OpenFirstShop, Target = 1 },
            new Mission { Id = "first_1000", Title = "Erste 1.000 €",
                Description = "Erwirtschafte insgesamt 1.000 € Umsatz.",
                Emoji = "💵", CashReward = 500,
                Type = MissionType.TotalRevenue, Target = 1000 },
            new Mission { Id = "hire_first", Title = "Erster Mitarbeiter",
                Description = "Stelle deinen ersten Mitarbeiter ein.",
                Emoji = "👨‍🍳", CashReward = 800,
                Type = MissionType.HireEmployees, Target = 1 },
            new Mission { Id = "first_equipment", Title = "Profi-Ausrüstung",
                Description = "Kaufe dein erstes Equipment für eine Filiale.",
                Emoji = "🔥", CashReward = 1000,
                Type = MissionType.BuyEquipment, Target = 1 },
            new Mission { Id = "first_product_unlock", Title = "Sortiment erweitern",
                Description = "Schalte ein neues Produkt durch Equipment frei (z.B. Pommes).",
                Emoji = "🍟", CashReward = 1500,
                Type = MissionType.UnlockProduct, Target = 1 },
            new Mission { Id = "cash_10k", Title = "10.000 € auf dem Konto",
                Description = "Erreiche einen Kontostand von 10.000 €.",
                Emoji = "💰", CashReward = 2000,
                Type = MissionType.ReachCash, Target = 10000 },
            new Mission { Id = "unlock_city", Title = "Stadt-Expansion",
                Description = "Schalte eine zweite Stadt frei.",
                Emoji = "🏙️", CashReward = 3000,
                Type = MissionType.UnlockCity, Target = 1 },
            new Mission { Id = "three_shops", Title = "Kleine Kette",
                Description = "Eröffne 3 Filialen.",
                Emoji = "🏬", CashReward = 5000,
                Type = MissionType.ShopCount, Target = 3 },
            new Mission { Id = "reputation_4", Title = "Stadt-Liebling",
                Description = "Erreiche 4.0 Reputation in mindestens einer Filiale.",
                Emoji = "⭐", CashReward = 4000,
                Type = MissionType.ReputationLevel, Target = 4.0 },
            new Mission { Id = "cash_100k", Title = "Sechsstelliger Boss",
                Description = "Erreiche 100.000 € auf dem Konto.",
                Emoji = "💎", CashReward = 10000,
                Type = MissionType.ReachCash, Target = 100000 },
            new Mission { Id = "survive_30", Title = "30 Tage am Spieß",
                Description = "Überlebe 30 Tage im Geschäft.",
                Emoji = "📅", CashReward = 5000,
                Type = MissionType.DaysSurvived, Target = 30 },
            new Mission { Id = "metropole", Title = "Metropolen-Boss",
                Description = "Eröffne eine Filiale in Berlin, Hamburg oder München.",
                Emoji = "👑", CashReward = 25000,
                Type = MissionType.ShopCount, Target = 1 },
        };
    }
}
