// Döner Empire 3D — Konzernweite Zutaten-Qualität pro Produkt
// Port aus lib/models/quality_model.dart.

namespace DoenerEmpire.Models
{
    public enum IngredientQuality { Budget, Standard, Premium }

    public static class IngredientQualityInfo
    {
        public static string Label(IngredientQuality q) => q switch
        {
            IngredientQuality.Budget => "Günstig",
            IngredientQuality.Standard => "Standard",
            IngredientQuality.Premium => "Premium",
            _ => "",
        };

        public static string Emoji(IngredientQuality q) => q switch
        {
            IngredientQuality.Budget => "🪙",
            IngredientQuality.Standard => "⚖️",
            IngredientQuality.Premium => "✨",
            _ => "",
        };

        /// <summary>Multiplikator auf die Zutatenkosten dieses Produkts.</summary>
        public static double IngredientMult(IngredientQuality q) => q switch
        {
            IngredientQuality.Budget => 0.78,
            IngredientQuality.Standard => 1.0,
            IngredientQuality.Premium => 1.35,
            _ => 1.0,
        };

        /// <summary>Täglicher Reputations-Beitrag des Qualitätsniveaus.</summary>
        public static double ReputationPerDay(IngredientQuality q) => q switch
        {
            IngredientQuality.Budget => -0.004,
            IngredientQuality.Standard => 0.0,
            IngredientQuality.Premium => 0.006,
            _ => 0.0,
        };

        // Dart-`enum.name`-Mapping (budget/standard/premium — alle lowercase)
        public static string ToDart(IngredientQuality q) => q.ToString().ToLowerInvariant();

        public static IngredientQuality FromDart(string name) => name switch
        {
            "budget" => IngredientQuality.Budget,
            "premium" => IngredientQuality.Premium,
            _ => IngredientQuality.Standard,
        };
    }
}
