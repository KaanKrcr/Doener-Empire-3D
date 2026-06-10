// Döner Empire 3D — Event-Auswahl + -Anwendung
// Port aus lib/providers/game_provider.dart (_rollEvent, applyEventChoice).
// RNG injizierbar für deterministische Tests.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class EventEngine
    {
        private static readonly Random DefaultRng = new();

        /// <summary>Erfüllt der State die Anforderungen des Events?</summary>
        public static bool IsEligible(GameEvent e, GameState state)
        {
            var req = e.Requirements;
            if (req.MinShops > state.Shops.Count) return false;
            if (req.MinDay > state.CurrentDay) return false;
            if (req.MinCash > state.Cash) return false;
            if (req.NeedsMetropolitanShop && !HasMetropolitanShop(state)) return false;
            return true;
        }

        public static bool HasMetropolitanShop(GameState state)
            => state.Shops.Any(s =>
            {
                var c = GameData.AllCities.FirstOrDefault(c => c.Id == s.CityId);
                return c != null && c.Tier == CityTier.Metropole;
            });

        /// <summary>
        /// Zieht ein gewichtetes Event (bevorzugt ungesehene). null, wenn keines
        /// die Anforderungen erfüllt. Gewichte: rare=1, normal=3, common=6.
        /// </summary>
        public static GameEvent RollEvent(GameState state, Random rng = null)
        {
            var r = rng ?? DefaultRng;
            var eligible = EventCatalog.All.Where(e => IsEligible(e, state)).ToList();
            if (eligible.Count == 0) return null;

            var unseen = eligible.Where(e => !state.SeenEventIds.Contains(e.Id)).ToList();
            var pool = unseen.Count > 0 ? unseen : eligible;

            var weights = pool.Select(WeightValue).ToList();
            var total = weights.Sum();
            var pick = r.Next(total);
            for (var i = 0; i < pool.Count; i++)
            {
                pick -= weights[i];
                if (pick < 0) return pool[i];
            }
            return pool[0];
        }

        private static int WeightValue(GameEvent e) => e.Weight switch
        {
            EventWeight.Rare => 1,
            EventWeight.Normal => 3,
            EventWeight.Common => 6,
            _ => 3,
        };

        /// <summary>
        /// Wendet die gewählte Option an: Cash-Delta, Reputation auf alle Filialen
        /// (geclamped 0.5..5.0), Brand-Awareness (0..100), markiert Event als gesehen.
        /// Mutiert den State.
        /// </summary>
        public static GameState ApplyChoice(GameState state, GameEvent gameEvent, EventChoice choice)
        {
            state.Cash += choice.Effect.CashDelta;
            foreach (var shop in state.Shops)
                shop.Reputation = Math.Clamp(shop.Reputation + choice.Effect.ReputationDelta, 0.5, 5.0);
            state.Brand.BrandAwareness = Math.Clamp(
                state.Brand.BrandAwareness + choice.Effect.BrandAwarenessDelta, 0.0, 100.0);
            if (!state.SeenEventIds.Contains(gameEvent.Id))
                state.SeenEventIds.Add(gameEvent.Id);
            return state;
        }
    }
}
