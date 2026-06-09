// Döner Empire 3D — Standort-Hotspot auf der City-Map
// Port aus lib/models/city_map_model.dart.
//
// Reine Logic-Schicht (keine UnityEngine-Abhängigkeit) — die Position ist
// als normalisierte Map-Koordinate (0..1) als POCO modelliert. Die Unity-
// View-Schicht (View3D) wandelt das in Vector2 bzw. Welt-Koordinaten um.

using System;
using DoenerEmpire.Core;
using DoenerEmpire.Data;

namespace DoenerEmpire.Models
{
    /// <summary>Normalisierte Map-Position 0..1 (X = links→rechts, Y = oben→unten).</summary>
    public readonly struct MapPosition
    {
        public readonly double X;
        public readonly double Y;
        public MapPosition(double x, double y) { X = x; Y = y; }
    }

    /// <summary>
    /// Visuelle und wirtschaftliche Beschreibung eines Standort-Hotspots auf der
    /// City-Map. Die Simulation bleibt bei <see cref="LocationTemplate"/>;
    /// diese Klasse ergänzt Karte, Position und Spielbarkeit.
    /// </summary>
    public sealed class CityMapLocation
    {
        public string Id;
        public string Label;
        public string Icon;
        public MapPosition MapPosition;
        public LocationTemplate Template;
        public string Audience;
        public string Risk;
        public string Recommendation;

        public double FootTrafficFactor => Template.FootTrafficFactor;
        public double RentFactor => Template.RentFactor;
        public LocationPersonality Personality => Template.Personality;

        public int FootTrafficFor(CityData city)
            => (int)Math.Round(city.FootTrafficBase * Template.FootTrafficFactor);

        public double WeeklyRentFor(CityData city)
            => city.RentBase * Template.RentFactor;

        public double DepositFor(CityData city) => WeeklyRentFor(city) * 2;

        /// <summary>
        /// Kompakter Score für schnelle Standort-Vergleiche auf der Karte.
        /// Höherer Laufkundschaftsfaktor ist gut, höherer Mietfaktor drückt.
        /// </summary>
        public double AttractivenessScore(CityData city)
        {
            var traffic = (double)FootTrafficFor(city) / city.FootTrafficBase;
            var rent = WeeklyRentFor(city) / city.RentBase;
            return Math.Clamp((traffic * 70.0) + (Math.Clamp(2.2 - rent, 0.0, 2.2) * 20.0),
                              0.0, 100.0);
        }
    }
}
