// Döner Empire 3D — Marketing-Kampagnen-Katalog
// 1:1-Port aus lib/core/constants.dart (kAllCampaigns, kCityMarketingCampaigns,
// kGlobalMarketingCampaigns, kAllMarketingCampaigns).
// MarketingCampaign-Modell liegt in Models/Marketing.cs.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Models;

namespace DoenerEmpire.Data
{
    public static class MarketingCatalog
    {
        /// <summary>Shop-Kampagnen (entspricht Dart kAllCampaigns).</summary>
        public static readonly IReadOnlyList<MarketingCampaign> ShopCampaigns = new List<MarketingCampaign>
        {
            new() { Id = "flyer_local", Name = "Flyer-Aktion",
                Description = "Bedruckte Flyer in der Nachbarschaft verteilen lassen. Klein aber wirkungsvoll.",
                Emoji = "📄", Cost = 400, DurationDays = 3, Scope = MarketingScope.Shop,
                CustomerBoost = 0.15, Risk = MarketingRisk.Low },
            new() { Id = "lunch_deal", Name = "Mittagsangebot",
                Description = "Spezial-Menü zur Mittagszeit — mehr Kunden, leicht geringere Marge.",
                Emoji = "🍽️", Cost = 0, DurationDays = 7, Scope = MarketingScope.Shop,
                CustomerBoost = 0.25, AvgOrderValueMod = -0.10, Risk = MarketingRisk.Low },
            new() { Id = "social_media", Name = "Social-Media-Boost",
                Description = "Instagram & TikTok Anzeigen. Erreicht jüngeres Publikum, baut Marke auf.",
                Emoji = "📱", Cost = 1500, DurationDays = 5, Scope = MarketingScope.Shop,
                CustomerBoost = 0.30, ReputationBoostPerDay = 0.08, Risk = MarketingRisk.Low },
            new() { Id = "two_for_one", Name = "2-für-1-Aktion",
                Description = "Riesiger Andrang, aber halbe Marge pro Verkauf. Riskanter Kunden-Magnet.",
                Emoji = "🎟️", Cost = 200, DurationDays = 2, Scope = MarketingScope.Shop,
                CustomerBoost = 0.80, AvgOrderValueMod = -0.40, ReputationBoostPerDay = 0.05,
                Risk = MarketingRisk.Medium },
            new() { Id = "food_influencer", Name = "Influencer-Kooperation",
                Description = "Bekannter Food-Blogger besucht deine Filiale. Chance auf viralen Hit, aber teuer.",
                Emoji = "⭐", Cost = 3500, DurationDays = 4, Scope = MarketingScope.Shop,
                CustomerBoost = 0.50, ReputationBoostOnce = 0.3, ReputationBoostPerDay = 0.05,
                ViralChance = 0.20, Risk = MarketingRisk.Medium },
            new() { Id = "radio_spot", Name = "Radio-Spot",
                Description = "Lokaler Radio-Sender bewirbt deinen Imbiss. Breite Reichweite, beständig.",
                Emoji = "📻", Cost = 5000, DurationDays = 7, Scope = MarketingScope.Shop,
                CustomerBoost = 0.35, ReputationBoostPerDay = 0.04, Risk = MarketingRisk.Low },
            new() { Id = "stadtfest_sponsor", Name = "Stadtfest-Sponsoring",
                Description = "Großes Image-Investment: Logo überall sichtbar. Langer Reputations-Schub.",
                Emoji = "🏟️", Cost = 8000, DurationDays = 14, Scope = MarketingScope.Shop,
                CustomerBoost = 0.25, ReputationBoostOnce = 0.5, ReputationBoostPerDay = 0.03,
                Risk = MarketingRisk.Low },
            new() { Id = "happy_hour", Name = "Happy Hour",
                Description = "Rabatt zur Randzeit füllt den Laden — mehr Andrang, etwas weniger Marge.",
                Emoji = "⏰", Cost = 300, DurationDays = 5, Scope = MarketingScope.Shop,
                CustomerBoost = 0.20, AvgOrderValueMod = -0.08, Risk = MarketingRisk.Low },
            new() { Id = "gewinnspiel", Name = "Gewinnspiel",
                Description = "Verlosung eines Gratis-Döner-Jahres. Viral-Potenzial, gut für die Marke.",
                Emoji = "🎁", Cost = 2500, DurationDays = 10, Scope = MarketingScope.Shop,
                CustomerBoost = 0.22, ReputationBoostPerDay = 0.03, BrandAwarenessDelta = 0.4,
                ViralChance = 0.20, Risk = MarketingRisk.Medium },
        };

        /// <summary>Stadtweite Kampagnen (entspricht Dart kCityMarketingCampaigns).</summary>
        public static readonly IReadOnlyList<MarketingCampaign> CityCampaigns = new List<MarketingCampaign>
        {
            new() { Id = "city_plakat", Name = "Stadtweite Plakate",
                Description = "Plakatwände in der ganzen Stadt — alle Filialen profitieren.",
                Emoji = "🪧", Cost = 3000, DurationDays = 7, Scope = MarketingScope.City,
                CustomerBoost = 0.15, ReputationBoostPerDay = 0.02, Risk = MarketingRisk.Low },
            new() { Id = "city_social", Name = "Stadtweite Social-Media-Kampagne",
                Description = "TikTok & Instagram, geotargeted auf die ganze Stadt.",
                Emoji = "📲", Cost = 6000, DurationDays = 10, Scope = MarketingScope.City,
                CustomerBoost = 0.25, ReputationBoostPerDay = 0.05, BrandAwarenessDelta = 0.3,
                Risk = MarketingRisk.Low },
            new() { Id = "city_event", Name = "Stadt-Event (Pop-Up Stand)",
                Description = "Eigener Stand auf einem Stadtfest oder Markt — starker Reputations-Boost.",
                Emoji = "🎪", Cost = 5000, DurationDays = 3, Scope = MarketingScope.City,
                CustomerBoost = 0.40, ReputationBoostOnce = 0.4, ReputationBoostPerDay = 0.06,
                BrandAwarenessDelta = 0.5, Risk = MarketingRisk.Medium },
            new() { Id = "city_radio", Name = "Stadtweiter Radio-Spot",
                Description = "Lokaler Radiosender — breite Hörerschaft in der ganzen Stadt.",
                Emoji = "📻", Cost = 8000, DurationDays = 14, Scope = MarketingScope.City,
                CustomerBoost = 0.20, ReputationBoostPerDay = 0.03, BrandAwarenessDelta = 0.4,
                Risk = MarketingRisk.Low },
        };

        /// <summary>Konzernweite Kampagnen (entspricht Dart kGlobalMarketingCampaigns).</summary>
        public static readonly IReadOnlyList<MarketingCampaign> GlobalCampaigns = new List<MarketingCampaign>
        {
            new() { Id = "tv_werbung", Name = "TV-Werbung (national)",
                Description = "Nationaler TV-Spot. Massiver Marken-Boost — alle Filialen profitieren.",
                Emoji = "📺", Cost = 25000, DurationDays = 14, Scope = MarketingScope.Global,
                CustomerBoost = 0.20, ReputationBoostPerDay = 0.03, BrandAwarenessDelta = 1.5,
                Risk = MarketingRisk.Low },
            new() { Id = "influencer_national", Name = "Influencer-Kampagne (national)",
                Description = "Bekannter Food-Influencer mit 500k+ Followern. Viral-Chance hoch.",
                Emoji = "⭐", Cost = 15000, DurationDays = 7, Scope = MarketingScope.Global,
                CustomerBoost = 0.30, ReputationBoostOnce = 0.5, ReputationBoostPerDay = 0.06,
                BrandAwarenessDelta = 2.0, ViralChance = 0.25, Risk = MarketingRisk.Medium },
            new() { Id = "brand_launch", Name = "Marken-Relaunch",
                Description = "Neues Logo, neues Erscheinungsbild — Premium-Image für alle Filialen.",
                Emoji = "🏷️", Cost = 20000, DurationDays = 30, Scope = MarketingScope.Global,
                CustomerBoost = 0.10, ReputationBoostPerDay = 0.04, BrandAwarenessDelta = 0.8,
                Risk = MarketingRisk.Low },
            new() { Id = "treue_programm", Name = "Treue-Programm-Kampagne",
                Description = "Bundesweite Stempelkarten-Aktion. Stammkunden +++ besonders lohnend "
                            + "wenn Stammkunden-App schon aktiv ist.",
                Emoji = "💳", Cost = 10000, DurationDays = 21, Scope = MarketingScope.Global,
                CustomerBoost = 0.12, ReputationBoostPerDay = 0.05, BrandAwarenessDelta = 0.5,
                Risk = MarketingRisk.Low },
        };

        /// <summary>Alle Kampagnen kombiniert (für Lookup nach ID).</summary>
        public static readonly IReadOnlyList<MarketingCampaign> All =
            ShopCampaigns.Concat(CityCampaigns).Concat(GlobalCampaigns).ToList();

        public static MarketingCampaign ById(string id) => All.FirstOrDefault(c => c.Id == id);
    }
}
