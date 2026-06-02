// Döner Empire 3D — Katalog-Daten: Mitarbeiter, Equipment, Standort-Templates
// 1:1-Port aus lib/core/constants.dart (kEmployeeTypes, kAllEquipment,
// kLocationTemplates).

using System.Collections.Generic;
using DoenerEmpire.Core;

namespace DoenerEmpire.Data
{
    public sealed class EmployeeTypeData
    {
        public string Id;
        public string Title;
        public string Emoji;
        public string Description;
        public double BaseSalaryPerDay;
        public double QualityContribution;
        public double SpeedContribution;
    }

    public sealed class EquipmentData
    {
        public string Id;
        public string Name;
        public string Emoji;
        public string Description;
        public double Price;
        public double QualityBonus;
        public int CapacityBonus;          // Stück/Tag
        public double SpeedBonus;
        public double IngredientSavingBonus;
        public EquipmentCategory Category;
        public string UnlocksProductId;    // null = keins
        public IReadOnlyList<string> AdditionalUnlocks = new List<string>();
    }

    public sealed class LocationTemplate
    {
        public string Name;
        public double FootTrafficFactor;
        public double RentFactor;
        public LocationPersonality Personality;
    }

    public static class GameCatalog
    {
        public static readonly IReadOnlyList<EmployeeTypeData> EmployeeTypes = new List<EmployeeTypeData>
        {
            new() { Id = "doener_meister", Title = "Döner-Meister", Emoji = "👨‍🍳",
                    Description = "Am Spieß. Qualität und Tempo bestimmen den Ruf.",
                    BaseSalaryPerDay = 80, QualityContribution = 0.40, SpeedContribution = 0.20 },
            new() { Id = "kassierer", Title = "Kassierer/in", Emoji = "💰",
                    Description = "Schnelle Abwicklung, weniger Warteschlangen.",
                    BaseSalaryPerDay = 65, QualityContribution = 0.05, SpeedContribution = 0.40 },
            new() { Id = "kuechen_hilfe", Title = "Küchenhilfe", Emoji = "🧑‍🍽️",
                    Description = "Unterstützt bei allem, erhöht Gesamtkapazität.",
                    BaseSalaryPerDay = 55, QualityContribution = 0.10, SpeedContribution = 0.25 },
        };

        public static readonly IReadOnlyList<EquipmentData> AllEquipment = new List<EquipmentData>
        {
            new() { Id = "spiess_klein", Name = "Döner-Spieß Klein", Emoji = "🔥",
                    Description = "Reicht für kleine Läden. Begrenzte Kapazität.",
                    Price = 800, QualityBonus = 0.15, CapacityBonus = 40,
                    Category = EquipmentCategory.Spiess },
            new() { Id = "spiess_standard", Name = "Döner-Spieß Standard", Emoji = "🔥",
                    Description = "Gute Kapazität, zuverlässige Qualität.",
                    Price = 2500, QualityBonus = 0.40, CapacityBonus = 100,
                    Category = EquipmentCategory.Spiess },
            new() { Id = "spiess_profi", Name = "Döner-Spieß Profi", Emoji = "🔥",
                    Description = "Höchste Qualität, maximale Kapazität für Stoßzeiten.",
                    Price = 7000, QualityBonus = 0.80, CapacityBonus = 200,
                    Category = EquipmentCategory.Spiess },
            new() { Id = "kasse_basic", Name = "Kasse Basic", Emoji = "🧾",
                    Description = "Einfache Kasse. Funktioniert.",
                    Price = 300, QualityBonus = 0.0, SpeedBonus = 0.05,
                    Category = EquipmentCategory.Kasse },
            new() { Id = "kasse_digital", Name = "Digitale Kasse", Emoji = "💳",
                    Description = "Kartenleser, schnellere Abwicklung, Statistiken.",
                    Price = 1200, QualityBonus = 0.05, SpeedBonus = 0.15,
                    Category = EquipmentCategory.Kasse },
            new() { Id = "fritteuse_standard", Name = "Fritteuse", Emoji = "🍟",
                    Description = "Ermöglicht Pommes und Döner-Box im Sortiment.",
                    Price = 700, QualityBonus = 0.10, Category = EquipmentCategory.Sonstiges,
                    UnlocksProductId = "pommes",
                    AdditionalUnlocks = new List<string> { "doenerbox" } },
            new() { Id = "ofen_lahmacun", Name = "Lahmacun-Ofen", Emoji = "🔆",
                    Description = "Traditioneller Steinofen für authentisches Lahmacun.",
                    Price = 1500, QualityBonus = 0.15, Category = EquipmentCategory.Sonstiges,
                    UnlocksProductId = "lahmacun" },
            new() { Id = "kuehlschrank", Name = "Profi-Kühlschrank", Emoji = "❄️",
                    Description = "Spart Zutatenkosten durch bessere Lagerung.",
                    Price = 600, QualityBonus = 0.05, IngredientSavingBonus = 0.08,
                    Category = EquipmentCategory.Sonstiges },
        };

        public static readonly IReadOnlyDictionary<CityTier, IReadOnlyList<LocationTemplate>> LocationTemplates =
            new Dictionary<CityTier, IReadOnlyList<LocationTemplate>>
            {
                [CityTier.Klein] = new List<LocationTemplate>
                {
                    new() { Name = "Marktplatz", FootTrafficFactor = 1.2, RentFactor = 1.3, Personality = LocationPersonality.Touristic },
                    new() { Name = "Hauptstraße", FootTrafficFactor = 1.0, RentFactor = 1.0, Personality = LocationPersonality.Business },
                    new() { Name = "Bahnhofsnähe", FootTrafficFactor = 0.9, RentFactor = 0.9, Personality = LocationPersonality.Transit },
                    new() { Name = "Randlage", FootTrafficFactor = 0.5, RentFactor = 0.6, Personality = LocationPersonality.Residential },
                },
                [CityTier.Mittel] = new List<LocationTemplate>
                {
                    new() { Name = "Fußgängerzone", FootTrafficFactor = 1.4, RentFactor = 1.6, Personality = LocationPersonality.Touristic },
                    new() { Name = "Einkaufszentrum", FootTrafficFactor = 1.3, RentFactor = 1.5, Personality = LocationPersonality.Business },
                    new() { Name = "Bahnhof", FootTrafficFactor = 1.1, RentFactor = 1.2, Personality = LocationPersonality.Transit },
                    new() { Name = "Wohnviertel", FootTrafficFactor = 0.7, RentFactor = 0.8, Personality = LocationPersonality.Residential },
                },
                [CityTier.Gross] = new List<LocationTemplate>
                {
                    new() { Name = "Innenstadt-Premium", FootTrafficFactor = 1.6, RentFactor = 2.0, Personality = LocationPersonality.Business },
                    new() { Name = "Shoppingcenter", FootTrafficFactor = 1.4, RentFactor = 1.7, Personality = LocationPersonality.Touristic },
                    new() { Name = "Uni-Viertel", FootTrafficFactor = 1.2, RentFactor = 1.3, Personality = LocationPersonality.University },
                    new() { Name = "Stadtrand", FootTrafficFactor = 0.8, RentFactor = 0.9, Personality = LocationPersonality.Residential },
                },
                [CityTier.Metropole] = new List<LocationTemplate>
                {
                    new() { Name = "Top-Lage Mitte", FootTrafficFactor = 2.0, RentFactor = 2.8, Personality = LocationPersonality.Touristic },
                    new() { Name = "Touristenviertel", FootTrafficFactor = 1.8, RentFactor = 2.4, Personality = LocationPersonality.Touristic },
                    new() { Name = "Businessviertel", FootTrafficFactor = 1.5, RentFactor = 2.0, Personality = LocationPersonality.Business },
                    new() { Name = "Trendbezirk", FootTrafficFactor = 1.3, RentFactor = 1.8, Personality = LocationPersonality.Nightlife },
                },
            };
    }
}
