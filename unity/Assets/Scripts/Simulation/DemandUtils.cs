// Döner Empire 3D — Nachfrage-Utilities (Season / Tagesspecial / Preis-Nachfrage)
// Port aus lib/services/game_engine.dart (Season-Enum + statische Hilfen).
//
// Diese reinen Funktionen sind die Foundation für den GameEngine-Port (M5).
// Sie werden später von ShopDayStats / processDay direkt verwendet — vorab
// portiert, damit sie isoliert testbar sind.

using System;
using DoenerEmpire.Core;
using DoenerEmpire.Data;

namespace DoenerEmpire.Simulation
{
    public enum Season { Fruehling, Sommer, Herbst, Winter }

    public static class SeasonInfo
    {
        public static string Label(Season s) => s switch
        {
            Season.Fruehling => "Frühling",
            Season.Sommer => "Sommer",
            Season.Herbst => "Herbst",
            Season.Winter => "Winter",
            _ => "",
        };

        public static string Emoji(Season s) => s switch
        {
            Season.Fruehling => "🌸",
            Season.Sommer => "☀️",
            Season.Herbst => "🍂",
            Season.Winter => "❄️",
            _ => "",
        };
    }

    public static class DemandUtils
    {
        /// <summary>Nachfrage-Multiplikator für das Tagesspecial.</summary>
        public const double DailySpecialBoost = 1.6;

        /// <summary>Steuersatz auf Monatsgewinn (alle 30 Tage fällig).</summary>
        public const double MonthlyTaxRate = 0.12;

        /// <summary>
        /// Produkt, das an einem bestimmten Tag „Tagesspecial" ist. Rotiert
        /// deterministisch durch den Produktkatalog.
        /// </summary>
        public static string DailySpecialProductId(int day)
        {
            var products = GameData.AllProducts;
            if (products.Count == 0) return "";
            // Dart `%` für negative Werte unterscheidet sich von C# — wir
            // garantieren positives Ergebnis via abs-Trick.
            var idx = ((day % products.Count) + products.Count) % products.Count;
            return products[idx].Id;
        }

        /// <summary>Aktuelle Jahreszeit (wechselt alle 30 Tage).</summary>
        public static Season SeasonForDay(int day)
        {
            // Dart: ((day - 1) ~/ 30) % 4 — Integer-Division Richtung 0.
            var idx = ((day - 1) / 30) % 4;
            if (idx < 0) idx = 0;
            if (idx > 3) idx = 3;
            return (Season)idx;
        }

        /// <summary>Saisonaler Nachfrage-Multiplikator je Produktkategorie.</summary>
        public static double SeasonCategoryMultiplier(Season s, ProductCategory cat)
        {
            switch (s)
            {
                case Season.Sommer:
                    if (cat == ProductCategory.Getraenk) return 1.25;
                    if (cat == ProductCategory.Doener) return 0.95;
                    return 1.0;
                case Season.Winter:
                    if (cat == ProductCategory.Doener) return 1.12;
                    if (cat == ProductCategory.Box) return 1.12;
                    if (cat == ProductCategory.Getraenk) return 0.85;
                    return 1.0;
                case Season.Fruehling:
                    if (cat == ProductCategory.Beilage) return 1.08;
                    return 1.0;
                case Season.Herbst:
                    if (cat == ProductCategory.Box) return 1.08;
                    if (cat == ProductCategory.Getraenk) return 1.05;
                    return 1.0;
                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// Preis-Nachfrage-Faktor (0..~1.25). Bei Rabatt &gt; Basispreis kommt
        /// ein Discount-Boost dazu (geclamped), bei Aufpreis ein gaußischer
        /// Demand-Drop. Difficulty steuert die Preissensibilität.
        /// </summary>
        public static double PriceDemandFactor(
            double price,
            double basePrice,
            GameDifficulty difficulty = GameDifficulty.Normal)
        {
            if (price <= 0) return 0;
            var sensitivity = Models.DifficultyData.Get(difficulty)
                .CustomerPriceSensitivityMultiplier;
            var ratio = price / basePrice;

            if (ratio <= 1.0)
            {
                var discountBoost = 0.4 / sensitivity;
                return Math.Clamp(1.0 + (1.0 - ratio) * discountBoost, 0.6, 1.25);
            }

            var overshoot = ratio - 1.0;
            var demand = Math.Exp(-Math.Pow(overshoot * 1.6 * sensitivity, 2));
            return Math.Clamp(demand, 0.0, 1.0);
        }
    }
}
