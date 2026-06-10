// Döner Empire 3D — Tageszeit-Profile pro Standort-Typ
// Port aus lib/models/time_profile_model.dart.
//
// Index 0 = 10 Uhr, Index 13 = 23 Uhr (14 Stunden Betrieb).
// Durchschnitt der hourlyFactors ≈ 1.0, damit Gesamtumsatz unverzerrt bleibt.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class TimeProfile
    {
        public readonly double[] HourlyFactors;    // 14 Werte (10..23h), 0..2.0
        public readonly double[] WeekdayFactors;   // 7 Werte (Mo..So)

        public TimeProfile(double[] hourly, double[] weekday)
        {
            HourlyFactors = hourly;
            WeekdayFactors = weekday;
        }

        public double Factor(int weekday, int hourSlot)
        {
            var h = System.Math.Clamp(hourSlot, 0, HourlyFactors.Length - 1);
            var w = System.Math.Clamp(weekday, 0, WeekdayFactors.Length - 1);
            return HourlyFactors[h] * WeekdayFactors[w];
        }

        public double DailyAverage(int weekday)
        {
            var w = System.Math.Clamp(weekday, 0, WeekdayFactors.Length - 1);
            double sum = 0;
            for (var i = 0; i < HourlyFactors.Length; i++) sum += HourlyFactors[i];
            return (sum / HourlyFactors.Length) * WeekdayFactors[w];
        }
    }

    public static class TimeProfiles
    {
        public static readonly IReadOnlyDictionary<Core.LocationPersonality, TimeProfile> All =
            new Dictionary<Core.LocationPersonality, TimeProfile>
            {
                [Core.LocationPersonality.Business] = new(
                    new[] { 0.4, 0.7, 1.6, 1.9, 1.4, 0.9, 0.8, 0.9, 1.0, 0.8, 0.5, 0.3, 0.2, 0.1 },
                    new[] { 1.10, 1.10, 1.10, 1.10, 1.20, 0.40, 0.30 }),
                [Core.LocationPersonality.University] = new(
                    new[] { 0.3, 0.5, 1.4, 1.5, 1.2, 0.9, 0.8, 0.9, 1.1, 1.3, 1.4, 1.5, 1.4, 1.0 },
                    new[] { 0.85, 0.95, 1.00, 1.30, 1.25, 1.00, 0.70 }),
                [Core.LocationPersonality.Touristic] = new(
                    new[] { 0.7, 1.0, 1.2, 1.3, 1.2, 1.0, 1.0, 1.1, 1.3, 1.4, 1.3, 1.0, 0.8, 0.6 },
                    new[] { 0.90, 0.90, 0.95, 1.00, 1.15, 1.30, 1.25 }),
                [Core.LocationPersonality.Residential] = new(
                    new[] { 0.3, 0.4, 0.8, 0.9, 0.7, 0.6, 0.8, 1.1, 1.5, 1.6, 1.3, 0.9, 0.6, 0.3 },
                    new[] { 0.85, 0.85, 0.90, 0.95, 1.10, 1.30, 1.30 }),
                [Core.LocationPersonality.Nightlife] = new(
                    new[] { 0.2, 0.3, 0.8, 0.9, 0.7, 0.6, 0.7, 0.8, 0.9, 1.0, 1.2, 1.5, 1.9, 2.0 },
                    new[] { 0.50, 0.60, 0.65, 0.80, 1.50, 1.80, 1.10 }),
                [Core.LocationPersonality.Transit] = new(
                    new[] { 0.8, 1.0, 1.2, 1.1, 1.0, 1.0, 1.1, 1.3, 1.2, 1.0, 0.9, 0.9, 0.8, 0.6 },
                    new[] { 1.05, 1.05, 1.05, 1.05, 1.10, 0.90, 0.75 }),
            };

        public static TimeProfile For(Core.LocationPersonality p)
            => All.TryGetValue(p, out var profile) ? profile : All[Core.LocationPersonality.Touristic];
    }

    public static class LocationPersonalityInfo
    {
        public static string Label(Core.LocationPersonality p) => p switch
        {
            Core.LocationPersonality.Business => "Bürogegend",
            Core.LocationPersonality.University => "Uni-Viertel",
            Core.LocationPersonality.Touristic => "Touristisch",
            Core.LocationPersonality.Residential => "Wohngebiet",
            Core.LocationPersonality.Nightlife => "Ausgehviertel",
            Core.LocationPersonality.Transit => "Verkehrsknoten",
            _ => "",
        };

        public static string Emoji(Core.LocationPersonality p) => p switch
        {
            Core.LocationPersonality.Business => "🏢",
            Core.LocationPersonality.University => "🎓",
            Core.LocationPersonality.Touristic => "📸",
            Core.LocationPersonality.Residential => "🏘️",
            Core.LocationPersonality.Nightlife => "🌃",
            Core.LocationPersonality.Transit => "🚉",
            _ => "",
        };

        public static string Description(Core.LocationPersonality p) => p switch
        {
            Core.LocationPersonality.Business => "Starker Mittag-Peak (12-14h). Abends tot.",
            Core.LocationPersonality.University => "Mittag + Abend stark. Studenten-Vibe.",
            Core.LocationPersonality.Touristic => "Konstant hoch ganztags. Stabil.",
            Core.LocationPersonality.Residential => "Familien-Abendessen. Wochenende Top.",
            Core.LocationPersonality.Nightlife => "Nacht-Crowd! Boomt nach 22 Uhr.",
            Core.LocationPersonality.Transit => "Konstanter Strom, kein extremer Peak.",
            _ => "",
        };
    }
}
