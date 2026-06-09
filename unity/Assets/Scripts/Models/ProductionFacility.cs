// Döner Empire 3D — Vertikale Integration: Produktionsanlagen
// Port aus lib/models/production_model.dart.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public enum ProductionType { Fleisch, Brot, Gemuese }

    public enum FacilityTier { Klein, Mittel, Gross, Industrie }

    public static class ProductionInfo
    {
        public static string Label(ProductionType t) => t switch
        {
            ProductionType.Fleisch => "Fleisch-Fabrik",
            ProductionType.Brot => "Bäckerei",
            ProductionType.Gemuese => "Gemüse-Großhandel",
            _ => "",
        };

        public static string Emoji(ProductionType t) => t switch
        {
            ProductionType.Fleisch => "🥩",
            ProductionType.Brot => "🥖",
            ProductionType.Gemuese => "🥬",
            _ => "",
        };

        /// <summary>Anteil der Zutaten-Kosten, den diese Anlage abdeckt.</summary>
        public static double CostShareCovered(ProductionType t) => t switch
        {
            ProductionType.Fleisch => 0.55,
            ProductionType.Brot => 0.20,
            ProductionType.Gemuese => 0.25,
            _ => 0.0,
        };

        public static string ToDart(ProductionType t) => t.ToString().ToLowerInvariant();
        public static ProductionType TypeFromDart(string raw) => raw switch
        {
            "brot" => ProductionType.Brot,
            "gemuese" => ProductionType.Gemuese,
            _ => ProductionType.Fleisch,
        };
    }

    public static class FacilityTierInfo
    {
        public static string Label(FacilityTier t) => t switch
        {
            FacilityTier.Klein => "Klein",
            FacilityTier.Mittel => "Mittel",
            FacilityTier.Gross => "Groß",
            FacilityTier.Industrie => "Industriell",
            _ => "",
        };

        public static int MaxShops(FacilityTier t) => t switch
        {
            FacilityTier.Klein => 5,
            FacilityTier.Mittel => 15,
            FacilityTier.Gross => 30,
            FacilityTier.Industrie => 999,
            _ => 5,
        };

        /// <summary>Kosten-Reduktion auf Zutaten (klein 20% … industrie 50%).</summary>
        public static double IngredientSaving(FacilityTier t) => t switch
        {
            FacilityTier.Klein => 0.20,
            FacilityTier.Mittel => 0.30,
            FacilityTier.Gross => 0.40,
            FacilityTier.Industrie => 0.50,
            _ => 0.20,
        };

        public static string ToDart(FacilityTier t) => t.ToString().ToLowerInvariant();
        public static FacilityTier FromDart(string raw) => raw switch
        {
            "mittel" => FacilityTier.Mittel,
            "gross" => FacilityTier.Gross,
            "industrie" => FacilityTier.Industrie,
            _ => FacilityTier.Klein,
        };
    }

    public sealed class FacilityTemplate
    {
        public ProductionType Type;
        public FacilityTier Tier;
        public double BuildCost;
        public double DailyOperatingCost;
        public double B2BRevenuePerDay;
    }

    public sealed class ProductionFacility
    {
        public string Id;
        public ProductionType Type;
        public FacilityTier Tier;
        public int DayBuilt;

        public ProductionFacility Clone() => new()
        {
            Id = Id, Type = Type, Tier = Tier, DayBuilt = DayBuilt,
        };
    }

    public static class FacilityCatalog
    {
        public static readonly IReadOnlyList<FacilityTemplate> All = new List<FacilityTemplate>
        {
            new() { Type = ProductionType.Fleisch, Tier = FacilityTier.Klein,
                BuildCost = 80000, DailyOperatingCost = 250, B2BRevenuePerDay = 300 },
            new() { Type = ProductionType.Fleisch, Tier = FacilityTier.Mittel,
                BuildCost = 200000, DailyOperatingCost = 500, B2BRevenuePerDay = 800 },
            new() { Type = ProductionType.Fleisch, Tier = FacilityTier.Gross,
                BuildCost = 500000, DailyOperatingCost = 900, B2BRevenuePerDay = 1800 },
            new() { Type = ProductionType.Fleisch, Tier = FacilityTier.Industrie,
                BuildCost = 1200000, DailyOperatingCost = 1500, B2BRevenuePerDay = 4000 },

            new() { Type = ProductionType.Brot, Tier = FacilityTier.Klein,
                BuildCost = 35000, DailyOperatingCost = 120, B2BRevenuePerDay = 150 },
            new() { Type = ProductionType.Brot, Tier = FacilityTier.Mittel,
                BuildCost = 90000, DailyOperatingCost = 240, B2BRevenuePerDay = 400 },
            new() { Type = ProductionType.Brot, Tier = FacilityTier.Gross,
                BuildCost = 240000, DailyOperatingCost = 450, B2BRevenuePerDay = 900 },

            new() { Type = ProductionType.Gemuese, Tier = FacilityTier.Klein,
                BuildCost = 25000, DailyOperatingCost = 90, B2BRevenuePerDay = 120 },
            new() { Type = ProductionType.Gemuese, Tier = FacilityTier.Mittel,
                BuildCost = 70000, DailyOperatingCost = 180, B2BRevenuePerDay = 320 },
            new() { Type = ProductionType.Gemuese, Tier = FacilityTier.Gross,
                BuildCost = 180000, DailyOperatingCost = 350, B2BRevenuePerDay = 750 },
        };

        public static FacilityTemplate Find(ProductionType type, FacilityTier tier)
        {
            foreach (var t in All) if (t.Type == type && t.Tier == tier) return t;
            return All[0];
        }
    }
}
