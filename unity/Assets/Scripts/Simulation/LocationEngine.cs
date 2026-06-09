// Döner Empire 3D — LocationEngine
// Port aus lib/services/location_engine.dart.
//
// Liefert Hotspots pro Stadt (als CityMapLocation), Standort-Lookup nach Name,
// und Aggregat-Werte pro Stadt für die Map-Übersicht. Reine Lesefunktionen,
// keine Seiteneffekte — testbar ohne RNG.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class CityMapSummary
    {
        public int ShopCount;
        public int TotalFootTraffic;
        public double WeeklyRent;
        public double AvgReputation;
        public bool HasPresence => ShopCount > 0;
    }

    public static class LocationEngine
    {
        public static List<CityMapLocation> LocationsFor(CityData city)
        {
            if (!GameCatalog.LocationTemplates.TryGetValue(city.Tier, out var templates))
            {
                templates = GameCatalog.LocationTemplates.TryGetValue(CityTier.Klein, out var klein)
                    ? klein
                    : new List<LocationTemplate>();
            }
            var result = new List<CityMapLocation>(templates.Count);
            for (var i = 0; i < templates.Count; i++)
            {
                var t = templates[i];
                var meta = MetaFor(t.Personality, t.Name);
                result.Add(new CityMapLocation
                {
                    Id = $"{city.Id}_{Slug(t.Name)}",
                    Label = t.Name,
                    Icon = meta.Icon,
                    MapPosition = PositionFor(i, templates.Count),
                    Template = t,
                    Audience = meta.Audience,
                    Risk = meta.Risk,
                    Recommendation = meta.Recommendation,
                });
            }
            return result;
        }

        public static CityMapLocation FindLocation(CityData city, string locationName)
        {
            foreach (var loc in LocationsFor(city))
            {
                if (loc.Template.Name == locationName || loc.Label == locationName)
                    return loc;
            }
            return null;
        }

        public static CityMapSummary Summarize(CityData city, IEnumerable<Shop> allShops)
        {
            var shops = allShops.Where(s => s.CityId == city.Id).ToList();
            var avgRep = shops.Count == 0 ? 0.0 : shops.Sum(s => s.Reputation) / shops.Count;
            return new CityMapSummary
            {
                ShopCount = shops.Count,
                TotalFootTraffic = shops.Sum(s => s.FootTraffic),
                WeeklyRent = shops.Sum(s => s.WeeklyRent),
                AvgReputation = avgRep,
            };
        }

        // ── Private ──────────────────────────────────────────────────────────

        private readonly struct LocationMeta
        {
            public readonly string Icon;
            public readonly string Audience;
            public readonly string Risk;
            public readonly string Recommendation;
            public LocationMeta(string icon, string audience, string risk, string recommendation)
            {
                Icon = icon; Audience = audience; Risk = risk; Recommendation = recommendation;
            }
        }

        private static LocationMeta MetaFor(LocationPersonality personality, string name)
        {
            return personality switch
            {
                LocationPersonality.Business => new LocationMeta(
                    "🏢",
                    "Büroarbeiter & Pendler",
                    "Hohe Mittagsspitzen, Personalengpässe werden teuer.",
                    "Premium-Preis + schnelle Kasse funktionieren hier gut."),
                LocationPersonality.Transit => new LocationMeta(
                    "🚉",
                    "Pendler & Laufkundschaft",
                    "Wenig Loyalität: Wartezeit kostet sofort Kunden.",
                    "Kapazität und günstige Klassiker priorisieren."),
                LocationPersonality.Residential => new LocationMeta(
                    "🏘️",
                    "Stammkunden & Familien",
                    "Weniger Laufkundschaft, Wachstum braucht Reputation.",
                    "Qualität, faire Preise und lokale Flyer stärken Stammkunden."),
                LocationPersonality.University => new LocationMeta(
                    "🎓",
                    "Studierende",
                    "Preissensibel, Rabatte drücken die Marge.",
                    "Combos, Social Media und günstige Dürüm-Angebote testen."),
                LocationPersonality.Nightlife => new LocationMeta(
                    "🌙",
                    "Nachtschwärmer",
                    "Starke Abendspitzen, schwächere Tagesauslastung.",
                    "Späte Öffnung, Getränke und Boxen pushen."),
                LocationPersonality.Touristic => new LocationMeta(
                    name.ToLowerInvariant().Contains("shopping") ? "🛍️" : "📍",
                    "Touristen & gemischte Laufkundschaft",
                    "Teure Lage: Miete muss durch hohen Durchsatz getragen werden.",
                    "Sichtbares Marketing und solide Qualität zahlen sich aus."),
                _ => new LocationMeta("📍", "", "", ""),
            };
        }

        private static readonly MapPosition[] Presets =
        {
            new(0.20, 0.68),
            new(0.42, 0.42),
            new(0.66, 0.62),
            new(0.78, 0.30),
            new(0.28, 0.24),
            new(0.54, 0.78),
        };

        private static MapPosition PositionFor(int index, int count)
        {
            if (index < Presets.Length) return Presets[index];
            var t = count <= 1 ? 0.0 : (double)index / (count - 1);
            return new MapPosition(
                0.18 + 0.64 * t,
                0.25 + 0.45 * ((index % 2) == 0 ? 1.0 : 0.0));
        }

        private static readonly Regex NonAlnum = new(@"[^a-z0-9äöüß]+", RegexOptions.Compiled);
        private static readonly Regex MultiUnderscore = new(@"_+", RegexOptions.Compiled);
        private static readonly Regex TrimUnderscore = new(@"^_|_$", RegexOptions.Compiled);

        private static string Slug(string value)
        {
            var s = value.ToLowerInvariant();
            s = NonAlnum.Replace(s, "_");
            s = MultiUnderscore.Replace(s, "_");
            s = TrimUnderscore.Replace(s, "");
            return s;
        }
    }
}
