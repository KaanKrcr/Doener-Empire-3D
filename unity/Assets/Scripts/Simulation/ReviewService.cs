// Döner Empire 3D — Prozedurale Kundenbewertungen
// Port aus lib/services/review_util.dart.
//
// Deterministisch pro (Tag, Filialanzahl). HINWEIS: C#-Random erzeugt eine
// andere Zahlenfolge als Dart-Random, daher sind die konkreten Texte/Autoren
// nicht byte-gleich zur Flutter-Version — der Verhaltensvertrag (Sterne 1..5,
// Reputation/Preis/Qualität-Tendenz, Stabilität pro Tag) ist identisch.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public sealed class CustomerReview
    {
        public int Stars;       // 1..5
        public string Author;
        public string Text;
        public string ShopName;
    }

    public static class ReviewService
    {
        private static readonly string[] Positive =
        {
            "Bester Döner der Stadt! Komme immer wieder.",
            "Mega lecker und das Fleisch frisch — top!",
            "Freundlicher Service, schnelle Bedienung. 👌",
            "Hier stimmt einfach alles. Klare Empfehlung!",
        };
        private static readonly string[] Neutral =
        {
            "Solider Döner, nichts Besonderes, aber okay.",
            "Ganz gut, beim nächsten Mal vielleicht mehr Soße.",
            "Durchschnitt — sättigt, aber haut nicht um.",
        };
        private static readonly string[] Negative =
        {
            "Lange Wartezeit und lauwarm. Schade.",
            "War schon mal besser hier. Eher mau.",
            "Nicht mein Favorit, da gibt es Bessere.",
        };
        private static readonly string[] TooExpensive =
        {
            "Für den Preis erwarte ich ehrlich mehr.",
            "Lecker, aber ganz schön teuer geworden.",
        };
        private static readonly string[] CheapGood =
        {
            "Top Preis-Leistung — günstig und gut!",
            "Für das Geld wirklich in Ordnung.",
        };
        private static readonly string[] PremiumPraise =
        {
            "Man schmeckt die guten Zutaten. Premium!",
            "Qualität merkt man — frische Zutaten.",
        };

        public static List<CustomerReview> GenerateReviews(GameState state, int count = 4)
        {
            if (state.Shops.Count == 0) return new List<CustomerReview>();
            var rng = new Random(state.CurrentDay * 7919 + state.Shops.Count);
            var names = EmployeeNames.Male.Concat(EmployeeNames.Female).ToList();

            var output = new List<CustomerReview>(count);
            for (var i = 0; i < count; i++)
            {
                var shop = state.Shops[rng.Next(state.Shops.Count)];
                var ratio = AvgPriceRatio(shop);
                var premium = AvgPremium(shop, state); // -1..1

                var stars = shop.Reputation;
                if (ratio > 1.2) stars -= 1.0;
                else if (ratio < 0.95) stars += 0.3;
                stars += premium * 0.4;
                stars += rng.NextDouble() * 1.5 - 0.75;
                var s = (int)Math.Round(Math.Clamp(stars, 1.0, 5.0), MidpointRounding.AwayFromZero);

                output.Add(new CustomerReview
                {
                    Stars = s,
                    Author = names[rng.Next(names.Count)],
                    Text = PickText(s, ratio, premium, rng),
                    ShopName = shop.DisplayName,
                });
            }
            return output;
        }

        private static double AvgPriceRatio(Shop shop)
        {
            var active = shop.Menu.Where(p => p.IsActive).ToList();
            if (active.Count == 0) return 1.0;
            double sum = 0;
            var n = 0;
            foreach (var sp in active)
            {
                var pd = GameData.AllProducts.FirstOrDefault(p => p.Id == sp.ProductId);
                if (pd == null || pd.BasePrice <= 0) continue;
                sum += sp.Price / pd.BasePrice;
                n++;
            }
            return n == 0 ? 1.0 : sum / n;
        }

        /// <summary>−1 (überwiegend günstig) .. +1 (überwiegend premium).</summary>
        private static double AvgPremium(Shop shop, GameState state)
        {
            var active = shop.Menu.Where(p => p.IsActive).ToList();
            if (active.Count == 0) return 0;
            double sum = 0;
            foreach (var sp in active)
            {
                var q = GameEngineCore.ProductQualityOf(state, sp.ProductId);
                sum += q switch
                {
                    IngredientQuality.Budget => -1.0,
                    IngredientQuality.Premium => 1.0,
                    _ => 0.0,
                };
            }
            return sum / active.Count;
        }

        private static string PickText(int stars, double ratio, double premium, Random rng)
        {
            if (ratio > 1.2 && stars <= 3 && rng.Next(2) == 0)
                return TooExpensive[rng.Next(TooExpensive.Length)];
            if (ratio < 0.95 && stars >= 3 && rng.Next(2) == 0)
                return CheapGood[rng.Next(CheapGood.Length)];
            if (premium > 0.4 && stars >= 4 && rng.Next(2) == 0)
                return PremiumPraise[rng.Next(PremiumPraise.Length)];
            if (stars >= 4) return Positive[rng.Next(Positive.Length)];
            if (stars == 3) return Neutral[rng.Next(Neutral.Length)];
            return Negative[rng.Next(Negative.Length)];
        }
    }
}
