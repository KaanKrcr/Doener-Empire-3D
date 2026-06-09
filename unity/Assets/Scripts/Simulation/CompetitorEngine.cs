// Döner Empire 3D — KI-Konkurrenz-Engine
// Port aus lib/services/competitor_engine.dart.
//
// Verantwortlich für:
//   • Spawnen von Konkurrenten bei erstem Stadt-Eintritt (size- und difficulty-abhängig)
//   • Tägliche Aktionen (Expansion, Preisanpassung, Reputations-Drift)
//   • Marktanteil-Neuverteilung pro Stadt
//   • Wettbewerbs-Druck-Faktor für die Kundennachfrage des Spielers
//
// RNG ist überall injizierbar (System.Random), damit Verhalten deterministisch
// testbar ist. Wenn kein RNG übergeben wird, wird ein statischer Default genutzt.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class CompetitorEngine
    {
        private static readonly Random DefaultRng = new();

        /// <summary>
        /// Liefert eine ggf. erweiterte Konkurrenten-Liste — spawnt für die Stadt
        /// 1..n Konkurrenten, wenn dort noch keiner existiert.
        /// Sollte beim ersten Eröffnen einer Filiale in der Stadt aufgerufen werden.
        /// </summary>
        public static List<Models.Competitor> EnsureCompetitorsForCity(
            IReadOnlyList<Models.Competitor> existing,
            string cityId,
            GameDifficulty difficulty,
            Models.CompetitorFactory factory = null,
            Random rng = null)
        {
            var result = new List<Models.Competitor>(existing);
            if (existing.Any(c => c.CityId == cityId)) return result;

            var r = rng ?? DefaultRng;
            var city = GameData.AllCities.FirstOrDefault(c => c.Id == cityId)
                       ?? GameData.AllCities[0];

            int count = city.Tier switch
            {
                CityTier.Klein => 1,
                CityTier.Mittel => 2,
                CityTier.Gross => 3,
                CityTier.Metropole => 3 + r.Next(2),
                _ => 1,
            };
            var spawnBonus = DifficultyData.Get(difficulty)
                .CompetitorAggressivenessMultiplier;
            var extra = Math.Clamp((int)Math.Round((spawnBonus - 1.0) * 2), 0, 2);
            count += extra;

            var f = factory ?? new Models.CompetitorFactory(r);
            var baseStamp = DateTime.UtcNow.Ticks;
            for (var i = 0; i < count; i++)
            {
                result.Add(f.Create(
                    id: $"comp_{cityId}_{baseStamp}_{i}",
                    cityId: cityId));
            }
            return result;
        }

        /// <summary>
        /// Tägliches Update aller Konkurrenten + Marktanteils-Neuverteilung
        /// pro Stadt. Mutiert die Competitor-Objekte direkt.
        /// </summary>
        public static IReadOnlyList<Models.Competitor> ProcessDay(
            Models.GameState state,
            Random rng = null)
        {
            var r = rng ?? DefaultRng;
            var aggressiveness = state.Modifiers
                .CompetitorAggressivenessMultiplier;

            foreach (var c in state.Competitors)
            {
                c.DaysSinceLastAction += 1;
                MaybeAct(c, state, aggressiveness, r);
            }

            var byCity = new Dictionary<string, List<Models.Competitor>>();
            foreach (var c in state.Competitors)
            {
                if (!byCity.TryGetValue(c.CityId, out var list))
                {
                    list = new List<Models.Competitor>();
                    byCity[c.CityId] = list;
                }
                list.Add(c);
            }
            foreach (var kv in byCity)
            {
                RecomputeMarketShares(kv.Value, state, kv.Key);
            }

            return state.Competitors;
        }

        /// <summary>
        /// Wettbewerbs-Druck auf eine Spieler-Filiale in dieser Stadt.
        /// Liefert ~0.55..1.10 — kleiner = mehr Druck.
        /// </summary>
        public static double CompetitionPressure(
            Models.GameState state,
            string cityId,
            double playerShopRep)
        {
            var inCity = state.CompetitorsIn(cityId);
            if (inCity.Count == 0) return 1.0;

            var aggressiveness = state.Modifiers
                .CompetitorAggressivenessMultiplier;

            var avgRivalRep = inCity.Sum(c => c.Reputation) / inCity.Count;
            var repDelta = playerShopRep - avgRivalRep;

            var density = inCity.Sum(c => c.ShopCount) / 3.0;

            var pressure = 1.0 - (density * 0.05 * aggressiveness);
            var defenseFactor = Math.Clamp(1.0 / aggressiveness, 0.6, 1.4);
            pressure += repDelta * 0.04 * defenseFactor;

            return Math.Clamp(pressure, 0.55, 1.10);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private static void MaybeAct(
            Models.Competitor c,
            Models.GameState state,
            double aggressiveness,
            Random rng)
        {
            var minDays = Math.Clamp((int)Math.Round(5.0 / aggressiveness), 2, 9);
            if (c.DaysSinceLastAction < minDays) return;

            var baseActionChance = c.Personality switch
            {
                CompetitorPersonality.Aggressive => 0.40,
                CompetitorPersonality.CheapMass => 0.25,
                CompetitorPersonality.Balanced => 0.18,
                CompetitorPersonality.Premium => 0.15,
                CompetitorPersonality.Traditional => 0.10,
                _ => 0.18,
            };
            var actionChance = Math.Clamp(baseActionChance * aggressiveness, 0.05, 0.90);
            if (rng.NextDouble() > actionChance) return;

            c.DaysSinceLastAction = 0;

            var r = rng.NextDouble();
            var expansionChance = Math.Clamp(0.30 * aggressiveness, 0.15, 0.55);
            var priceChance =
                Math.Clamp(0.30 + (aggressiveness - 1.0) * 0.10, 0.20, 0.50);

            if (r < expansionChance && c.ShopCount < 5)
            {
                c.ShopCount = Math.Clamp(c.ShopCount + 1, 1, 5);
                c.Reputation = Math.Clamp(c.Reputation - 0.05, 1.0, 5.0);
            }
            else if (r < expansionChance + priceChance)
            {
                var hasPlayer = state.HasShopIn(c.CityId);
                if (c.Personality == CompetitorPersonality.Aggressive && hasPlayer)
                {
                    c.PriceLevel = Math.Clamp(c.PriceLevel - 0.05, 0.65, 1.4);
                }
                else if (c.Personality == CompetitorPersonality.Premium)
                {
                    c.PriceLevel = Math.Clamp(c.PriceLevel + 0.04, 0.65, 1.4);
                }
                else if (c.Personality == CompetitorPersonality.CheapMass)
                {
                    c.PriceLevel = Math.Clamp(c.PriceLevel - 0.02, 0.65, 1.4);
                }
                else
                {
                    c.PriceLevel = Math.Clamp(
                        c.PriceLevel + (rng.NextDouble() - 0.5) * 0.04,
                        0.65, 1.4);
                }
            }
            else
            {
                var delta = (rng.NextDouble() - 0.45) * 0.20;
                c.Reputation = Math.Clamp(c.Reputation + delta, 1.0, 5.0);
            }
        }

        /// <summary>
        /// Verteilt Marktanteile auf Konkurrenten basierend auf Reputation,
        /// Preisniveau, Filialdichte und Difficulty. Der Spieler ist implizit
        /// der Rest — Konkurrenten-Anteile addieren sich nicht auf 1.0.
        /// </summary>
        private static void RecomputeMarketShares(
            List<Models.Competitor> competitors,
            Models.GameState state,
            string cityId)
        {
            if (competitors.Count == 0) return;
            var aggressiveness = state.Modifiers
                .CompetitorAggressivenessMultiplier;

            var playerShops = state.Shops.Where(s => s.CityId == cityId);
            var playerPower = playerShops.Sum(s => (s.Reputation / 5.0) * 1.0);

            var compPower = competitors.Sum(c =>
            {
                var priceScore = Math.Clamp(2.0 - c.PriceLevel, 0.5, 1.5);
                return (c.Reputation / 5.0) * priceScore * c.ShopCount * aggressiveness;
            });

            var totalPower = playerPower + compPower;
            if (totalPower <= 0) return;

            foreach (var c in competitors)
            {
                var priceScore = Math.Clamp(2.0 - c.PriceLevel, 0.5, 1.5);
                var p = (c.Reputation / 5.0) * priceScore * c.ShopCount * aggressiveness;
                c.MarketShare = Math.Clamp(p / totalPower, 0.0, 1.0);
            }
        }
    }
}
