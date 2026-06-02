// Döner Empire 3D — Markenbekanntheit + Stadt-Reputation
// Port aus lib/models/brand_model.dart (BrandStats).

using System;
using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class BrandStats
    {
        public double BrandAwareness = 5.0;                 // 0..100, deutschlandweit
        public Dictionary<string, double> CityReputation = new(); // key = cityId, 0..100

        /// <summary>Reputation in einer Stadt (0, wenn kein Eintrag).</summary>
        public double InCity(string cityId)
            => CityReputation.TryGetValue(cityId, out var v) ? v : 0.0;

        /// <summary>Kundenstrom-Faktor (0.85..1.45): Brand schwach (überall), City-Rep stark (lokal).</summary>
        public double CustomerMultiplier(string cityId)
        {
            var brandPart = 0.05 * (BrandAwareness / 100.0); // max +5%
            var cityPart = 0.40 * (InCity(cityId) / 100.0);  // max +40%
            return Math.Clamp(1.0 + brandPart + cityPart, 0.85, 1.45);
        }

        public string TierLabel => BrandAwareness switch
        {
            >= 80 => "Legendär 👑",
            >= 60 => "Weithin bekannt",
            >= 40 => "Etablierte Marke",
            >= 20 => "Bekannt",
            >= 5 => "Aufstrebend",
            _ => "Unbekannt",
        };

        public int TierStars => BrandAwareness switch
        {
            >= 80 => 5,
            >= 60 => 4,
            >= 40 => 3,
            >= 20 => 2,
            _ => 1,
        };

        public BrandStats Clone() => new()
        {
            BrandAwareness = BrandAwareness,
            CityReputation = new Dictionary<string, double>(CityReputation),
        };
    }
}
