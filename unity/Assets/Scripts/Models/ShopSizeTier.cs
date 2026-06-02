// Döner Empire 3D — Filialgrößen-Stufen & Cap-/Ausbau-Logik
// Port aus lib/models/shop_model.dart (kShopSizeTierConfig, ShopSizeTierX)
// und lib/services/game_engine.dart (employeeCapForTier, shopExpansionCost,
// _cityEmployeeCapForTier).  ShopSizeTier-Enum liegt in Core/Enums.cs.

using System;
using System.Collections.Generic;
using DoenerEmpire.Core;

namespace DoenerEmpire.Models
{
    public sealed class ShopSizeTierConfig
    {
        public int EmployeeCap;
        public double CapacityMultiplier;
        public double UpgradeCost;
        public double RentMultiplier;
        public double MoraleDeltaOnUpgrade;
    }

    public static class ShopSizing
    {
        public static readonly IReadOnlyDictionary<ShopSizeTier, ShopSizeTierConfig> Config =
            new Dictionary<ShopSizeTier, ShopSizeTierConfig>
            {
                [ShopSizeTier.Klein] = new ShopSizeTierConfig
                {
                    EmployeeCap = 3, CapacityMultiplier = 1.00, UpgradeCost = 0,
                    RentMultiplier = 1.00, MoraleDeltaOnUpgrade = 0,
                },
                [ShopSizeTier.Mittel] = new ShopSizeTierConfig
                {
                    EmployeeCap = 5, CapacityMultiplier = 1.35, UpgradeCost = 8000,
                    RentMultiplier = 1.25, MoraleDeltaOnUpgrade = -0.02,
                },
                [ShopSizeTier.Gross] = new ShopSizeTierConfig
                {
                    EmployeeCap = 8, CapacityMultiplier = 1.75, UpgradeCost = 25000,
                    RentMultiplier = 1.60, MoraleDeltaOnUpgrade = -0.05,
                },
                [ShopSizeTier.Flagship] = new ShopSizeTierConfig
                {
                    EmployeeCap = 12, CapacityMultiplier = 2.20, UpgradeCost = 70000,
                    RentMultiplier = 2.10, MoraleDeltaOnUpgrade = -0.08,
                },
            };

        public static ShopSizeTierConfig ConfigFor(ShopSizeTier tier)
            => Config.TryGetValue(tier, out var c) ? c : Config[ShopSizeTier.Klein];

        public static string Label(ShopSizeTier tier) => tier switch
        {
            ShopSizeTier.Klein => "Klein",
            ShopSizeTier.Mittel => "Mittel",
            ShopSizeTier.Gross => "Groß",
            ShopSizeTier.Flagship => "Flagship",
            _ => "Klein",
        };

        /// <summary>Nächsthöhere Stufe oder null bei Maximalstufe.</summary>
        public static ShopSizeTier? NextTier(ShopSizeTier tier)
        {
            var next = (int)tier + 1;
            var count = Enum.GetValues(typeof(ShopSizeTier)).Length;
            if (next >= count) return null;
            return (ShopSizeTier)next;
        }

        public static ShopSizeTier FromDartName(string value)
            => EnumNames.ShopSizeFromDart(value ?? "klein");

        public static ShopSizeTier FromLegacyExpansionLevel(int level)
        {
            var count = Enum.GetValues(typeof(ShopSizeTier)).Length;
            var normalized = Math.Max(0, Math.Min(level, count - 1));
            return (ShopSizeTier)normalized;
        }

        // ── Personal-Cap ─────────────────────────────────────────────────────
        // Harte Stadt-Obergrenze je CityTier (aus game_engine._cityEmployeeCapForTier).
        public static int CityEmployeeCap(CityTier tier) => tier switch
        {
            CityTier.Klein => 5,
            CityTier.Mittel => 8,
            CityTier.Gross => 10,
            CityTier.Metropole => 12,
            _ => 5,
        };

        /// <summary>Effektiver Cap = min(Stadt-Cap, Stufen-Cap).</summary>
        public static int EmployeeCap(CityTier cityTier, ShopSizeTier sizeTier)
            => Math.Min(CityEmployeeCap(cityTier), ConfigFor(sizeTier).EmployeeCap);

        /// <summary>Ausbaukosten auf die nächste Stufe (0 bei Maximalstufe).</summary>
        public static double ExpansionCost(ShopSizeTier currentTier)
        {
            var next = NextTier(currentTier);
            return next == null ? 0.0 : ConfigFor(next.Value).UpgradeCost;
        }
    }
}
