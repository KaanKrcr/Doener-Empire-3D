// Döner Empire 3D — Upgrade-Katalog (Filial- + Konzern-Upgrades)
// 1:1-Port aus lib/models/upgrade_model.dart.

using System.Collections.Generic;
using System.Linq;

namespace DoenerEmpire.Data
{
    public enum UpgradeScope { Shop, Global }

    public enum UpgradeCategory { Komfort, Ambiente, Service, Hygiene }

    public sealed class UpgradeData
    {
        public string Id;
        public string Name;
        public string Description;
        public string Emoji;
        public UpgradeCategory Category;
        public UpgradeScope Scope = UpgradeScope.Shop;
        public double InstallCost;
        public double MonthlyCost;
        public double CustomerBoost;
        public double ReputationPerDay;
        public double AvgOrderValueBoost;
        public double BrandPerDay;
        public double DeliveryRevenueFraction;
        public double DeliveryCommissionRate;

        public double DailyCost => MonthlyCost / 30.0;
        public bool IsDelivery => DeliveryRevenueFraction > 0;
        public bool IsGlobal => Scope == UpgradeScope.Global;
    }

    public static class UpgradeCatalog
    {
        // IDs der globalen Spieß-Upgrades (entsprechen GameEngineCore-Konstanten).
        public const string GlobalSpiessBasicId = "doener_spiess_global_basic";
        public const string GlobalSpiessStandardId = "doener_spiess_global_standard";
        public const string GlobalSpiessProfiId = "doener_spiess_global_profi";

        public static readonly IReadOnlyList<string> GlobalSpiessUpgradeOrder = new List<string>
        {
            GlobalSpiessBasicId, GlobalSpiessStandardId, GlobalSpiessProfiId,
        };

        public static readonly IReadOnlyList<UpgradeData> ShopUpgrades = new List<UpgradeData>
        {
            new() { Id = "wifi", Name = "Gratis-WLAN",
                Description = "Stammkunden bleiben länger sitzen, neue Kunden kommen rein.",
                Emoji = "📶", Category = UpgradeCategory.Komfort,
                InstallCost = 800, MonthlyCost = 90, CustomerBoost = 0.06, ReputationPerDay = 0.005 },
            new() { Id = "klima", Name = "Klimaanlage",
                Description = "Im Sommer ein Magnet - frische Kühle macht den Unterschied.",
                Emoji = "❄️", Category = UpgradeCategory.Komfort,
                InstallCost = 2500, MonthlyCost = 360, CustomerBoost = 0.08, ReputationPerDay = 0.008 },
            new() { Id = "heizpilz", Name = "Außenbereich + Heizpilze",
                Description = "Sitzplätze draußen das ganze Jahr - gemütliche Atmosphäre.",
                Emoji = "🔥", Category = UpgradeCategory.Komfort,
                InstallCost = 1800, MonthlyCost = 240, CustomerBoost = 0.10, AvgOrderValueBoost = 0.05 },
            new() { Id = "musik", Name = "Lizenzierte Musik (GEMA)",
                Description = "Türkische + deutsche Hits, legal abgespielt. Stimmung pur.",
                Emoji = "🎶", Category = UpgradeCategory.Ambiente,
                InstallCost = 400, MonthlyCost = 120, CustomerBoost = 0.05, ReputationPerDay = 0.006 },
            new() { Id = "deko_premium", Name = "Premium-Inneneinrichtung",
                Description = "Türkische Lampen, Mosaike - instagrammable.",
                Emoji = "🪔", Category = UpgradeCategory.Ambiente,
                InstallCost = 3500, MonthlyCost = 100, CustomerBoost = 0.08,
                ReputationPerDay = 0.010, BrandPerDay = 0.02 },
            new() { Id = "tv_sport", Name = "TV mit Sportübertragung",
                Description = "Fußball läuft - Gruppen kommen und bestellen mehr.",
                Emoji = "📺", Category = UpgradeCategory.Ambiente,
                InstallCost = 1200, MonthlyCost = 160, CustomerBoost = 0.07, AvgOrderValueBoost = 0.08 },
            new() { Id = "kartenzahlung", Name = "Kartenzahlung (alle Karten)",
                Description = "Apple Pay, Visa, EC - niemand geht mehr ohne zu zahlen.",
                Emoji = "💳", Category = UpgradeCategory.Service,
                InstallCost = 500, MonthlyCost = 80, CustomerBoost = 0.04, AvgOrderValueBoost = 0.06 },
            new() { Id = "bio_zutaten", Name = "Bio-Zutaten",
                Description = "Bio-Fleisch und -Gemüse. Höhere Marge möglich, aber teurer.",
                Emoji = "🥦", Category = UpgradeCategory.Hygiene,
                InstallCost = 0, MonthlyCost = 700, CustomerBoost = 0.06,
                AvgOrderValueBoost = 0.15, ReputationPerDay = 0.012 },
            new() { Id = "premium_reinigung", Name = "Premium-Reinigungsservice",
                Description = "Tägliche Profi-Reinigung. Lebensmittelkontrollen lieben dich.",
                Emoji = "✨", Category = UpgradeCategory.Hygiene,
                InstallCost = 200, MonthlyCost = 560, ReputationPerDay = 0.015, CustomerBoost = 0.03 },
        };

        public static readonly IReadOnlyList<UpgradeData> GlobalUpgrades = new List<UpgradeData>
        {
            new() { Id = GlobalSpiessBasicId, Name = "Döner-Spieß-Netzwerk Basis",
                Description = "Konzernweiter Spieß-Standard für alle Filialen mit monatlichen Fixkosten.",
                Emoji = "🔥", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 1800, MonthlyCost = 220 },
            new() { Id = GlobalSpiessStandardId, Name = "Döner-Spieß-Netzwerk Standard",
                Description = "Mehr Kapazität und Qualität konzernweit. Ersetzt die Basisstufe.",
                Emoji = "🔥", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 4200, MonthlyCost = 520 },
            new() { Id = GlobalSpiessProfiId, Name = "Döner-Spieß-Netzwerk Profi",
                Description = "Maximale zentrale Spießversorgung für alle Filialen. Ersetzt niedrigere Stufen.",
                Emoji = "🔥", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 9800, MonthlyCost = 980 },
            new() { Id = "lieferdienst", Name = "Lieferdienst (Lieferando)",
                Description = "Zentraler Lieferkanal für alle Filialen. Die Plattform behält 28 % Provision auf Lieferumsätze.",
                Emoji = "🛵", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 600, MonthlyCost = 500, CustomerBoost = 0.18,
                DeliveryRevenueFraction = 0.18, DeliveryCommissionRate = 0.28 },
            new() { Id = "loyalty_app", Name = "Stammkunden-App",
                Description = "Eigene App mit Stempelkarten und Gutscheinen. Einmalig kaufen - gilt für alle Filialen.",
                Emoji = "💳", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 8000, MonthlyCost = 350, CustomerBoost = 0.10,
                ReputationPerDay = 0.008, BrandPerDay = 0.05 },
            new() { Id = "kassensystem_zentral", Name = "Zentrales Kassensystem",
                Description = "Einheitliches POS in allen Filialen - Echtzeitdaten, weniger Fehler. Reduziert Zutatenverschwendung konzernweit.",
                Emoji = "🖥️", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 12000, MonthlyCost = 600, AvgOrderValueBoost = 0.03, ReputationPerDay = 0.005 },
            new() { Id = "schulung_online", Name = "Online-Schulungsplattform",
                Description = "Alle Mitarbeiter lernen schneller. Effektiv +10 % Speed und Freundlichkeit in jeder Filiale.",
                Emoji = "🎓", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 6000, MonthlyCost = 250, CustomerBoost = 0.05, ReputationPerDay = 0.010 },
            new() { Id = "eigen_lieferdienst", Name = "Eigene Liefer-App",
                Description = "Eigene Logistik ohne Provision. Benötigt Lieferdienst in mindestens 3 Filialen. Provision fällt auf 8 %.",
                Emoji = "🚀", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 35000, MonthlyCost = 1200, CustomerBoost = 0.08, BrandPerDay = 0.04 },
            new() { Id = "social_media_team", Name = "Social-Media-Team",
                Description = "Eigenes Team für TikTok, Instagram & Co. Hält die Marke landesweit präsent.",
                Emoji = "📣", Category = UpgradeCategory.Service, Scope = UpgradeScope.Global,
                InstallCost = 14000, MonthlyCost = 700, CustomerBoost = 0.07,
                ReputationPerDay = 0.004, BrandPerDay = 0.06 },
            new() { Id = "bio_zertifikat", Name = "Bio-Zertifizierung",
                Description = "Konzernweites Bio-Siegel. Premium-Image, höherer Bestellwert — aber laufende Audit-Kosten.",
                Emoji = "🌿", Category = UpgradeCategory.Hygiene, Scope = UpgradeScope.Global,
                InstallCost = 10000, MonthlyCost = 450, ReputationPerDay = 0.012,
                AvgOrderValueBoost = 0.05, BrandPerDay = 0.02 },
        };

        public static readonly IReadOnlyList<UpgradeData> All =
            ShopUpgrades.Concat(GlobalUpgrades).ToList();

        public static UpgradeData ById(string id) => All.FirstOrDefault(u => u.Id == id);
    }
}
