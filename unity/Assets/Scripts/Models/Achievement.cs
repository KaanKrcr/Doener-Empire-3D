// Döner Empire 3D — Achievements / Trophäen
// Port aus lib/models/achievement_model.dart.
// Ungeordnet, triggern einmalig bei Erfüllung (Long-Term-Engagement).

using System;
using System.Collections.Generic;
using System.Linq;

namespace DoenerEmpire.Models
{
    public enum AchievementTier { Bronze, Silber, Gold, Platin }

    public static class AchievementTierInfo
    {
        public static string Label(AchievementTier t) => t switch
        {
            AchievementTier.Bronze => "Bronze",
            AchievementTier.Silber => "Silber",
            AchievementTier.Gold => "Gold",
            AchievementTier.Platin => "Platin",
            _ => "",
        };

        public static int Points(AchievementTier t) => t switch
        {
            AchievementTier.Bronze => 10,
            AchievementTier.Silber => 25,
            AchievementTier.Gold => 50,
            AchievementTier.Platin => 100,
            _ => 0,
        };
    }

    /// <summary>Metriken, gegen die ein Achievement prüft.</summary>
    public readonly struct AchievementMetrics
    {
        public readonly int TotalShops;
        public readonly int TotalEmployees;
        public readonly double TotalRevenue;
        public readonly double Cash;
        public readonly int CurrentDay;
        public readonly int CustomersTotal;
        public readonly double MaxShopRep;
        public readonly double BrandAwareness;
        public readonly int CompetitorsBeat;

        public AchievementMetrics(int totalShops, int totalEmployees, double totalRevenue,
            double cash, int currentDay, int customersTotal, double maxShopRep,
            double brandAwareness, int competitorsBeat)
        {
            TotalShops = totalShops;
            TotalEmployees = totalEmployees;
            TotalRevenue = totalRevenue;
            Cash = cash;
            CurrentDay = currentDay;
            CustomersTotal = customersTotal;
            MaxShopRep = maxShopRep;
            BrandAwareness = brandAwareness;
            CompetitorsBeat = competitorsBeat;
        }
    }

    public sealed class Achievement
    {
        public string Id;
        public string Title;
        public string Description;
        public string Emoji;
        public AchievementTier Tier;
        public Func<AchievementMetrics, bool> Check;
    }

    public static class AchievementCatalog
    {
        public static readonly IReadOnlyList<Achievement> All = new List<Achievement>
        {
            // Bronze
            new() { Id = "first_shop", Title = "Tag 1 als Boss",
                Description = "Eröffne deine erste Filiale.", Emoji = "🥙",
                Tier = AchievementTier.Bronze, Check = m => m.TotalShops >= 1 },
            new() { Id = "first_week", Title = "Eine Woche überstanden",
                Description = "Erreiche Tag 7 ohne Pleite.", Emoji = "📅",
                Tier = AchievementTier.Bronze, Check = m => m.CurrentDay >= 7 },
            new() { Id = "first_5_employees", Title = "Kleines Team",
                Description = "Stelle insgesamt 5 Mitarbeiter ein.", Emoji = "👥",
                Tier = AchievementTier.Bronze, Check = m => m.TotalEmployees >= 5 },
            new() { Id = "thousand_customers", Title = "1.000 Kunden",
                Description = "Bediene insgesamt 1.000 Kunden.", Emoji = "👥",
                Tier = AchievementTier.Bronze, Check = m => m.CustomersTotal >= 1000 },

            // Silber
            new() { Id = "three_cities", Title = "Regional aufgestellt",
                Description = "Filialen in 3 verschiedenen Städten.", Emoji = "🗺️",
                Tier = AchievementTier.Silber, Check = m => m.TotalShops >= 3 },
            new() { Id = "rep_45", Title = "Premium-Adresse",
                Description = "Erreiche 4,5 Reputation in einer Filiale.", Emoji = "⭐",
                Tier = AchievementTier.Silber, Check = m => m.MaxShopRep >= 4.5 },
            new() { Id = "cash_50k", Title = "Fünfstellig",
                Description = "Erreiche 50.000 € Konto.", Emoji = "💵",
                Tier = AchievementTier.Silber, Check = m => m.Cash >= 50000 },
            new() { Id = "thirty_days", Title = "Ein Monat im Geschäft",
                Description = "Überlebe 30 Tage.", Emoji = "📆",
                Tier = AchievementTier.Silber, Check = m => m.CurrentDay >= 30 },

            // Gold
            new() { Id = "ten_shops", Title = "Kettenbetreiber",
                Description = "Eröffne 10 Filialen.", Emoji = "🏬",
                Tier = AchievementTier.Gold, Check = m => m.TotalShops >= 10 },
            new() { Id = "cash_250k", Title = "Viertelmillionär",
                Description = "Erreiche 250.000 € Konto.", Emoji = "💎",
                Tier = AchievementTier.Gold, Check = m => m.Cash >= 250000 },
            new() { Id = "brand_40", Title = "Etablierte Marke",
                Description = "Markenbekanntheit 40+.", Emoji = "📢",
                Tier = AchievementTier.Gold, Check = m => m.BrandAwareness >= 40 },
            new() { Id = "ten_thousand_customers", Title = "10.000 Kunden bedient",
                Description = "Bediene 10.000 Kunden insgesamt.", Emoji = "🥳",
                Tier = AchievementTier.Gold, Check = m => m.CustomersTotal >= 10000 },

            // Platin
            new() { Id = "million_revenue", Title = "Million-Euro-Boss",
                Description = "1.000.000 € Gesamtumsatz.", Emoji = "👑",
                Tier = AchievementTier.Platin, Check = m => m.TotalRevenue >= 1000000 },
            new() { Id = "brand_80", Title = "Legendäre Marke",
                Description = "Markenbekanntheit 80+.", Emoji = "🌟",
                Tier = AchievementTier.Platin, Check = m => m.BrandAwareness >= 80 },
            new() { Id = "twenty_shops", Title = "Imperium-Modus",
                Description = "20 Filialen aktiv.", Emoji = "🏰",
                Tier = AchievementTier.Platin, Check = m => m.TotalShops >= 20 },

            // Zusätzliche Langzeit-Ziele
            new() { Id = "five_star_shop", Title = "Perfekte Filiale",
                Description = "Erreiche die volle 5,0 Reputation in einer Filiale.", Emoji = "🌟",
                Tier = AchievementTier.Gold, Check = m => m.MaxShopRep >= 5.0 },
            new() { Id = "fifty_employees", Title = "Großer Arbeitgeber",
                Description = "Beschäftige insgesamt 50 Mitarbeiter.", Emoji = "👔",
                Tier = AchievementTier.Gold, Check = m => m.TotalEmployees >= 50 },
            new() { Id = "hundred_days", Title = "100 Tage am Spieß",
                Description = "Überlebe 100 Tage im Geschäft.", Emoji = "🗓️",
                Tier = AchievementTier.Gold, Check = m => m.CurrentDay >= 100 },
            new() { Id = "cash_500k", Title = "Halbe Million",
                Description = "Erreiche 500.000 € auf dem Konto.", Emoji = "🤑",
                Tier = AchievementTier.Platin, Check = m => m.Cash >= 500000 },
        };

        public static Achievement ById(string id) => All.FirstOrDefault(a => a.Id == id);
    }
}
