// Döner Empire 3D — Kern-Tagessimulation (calculateShopStats etc.)
// Port aus lib/services/game_engine.dart §"Tageseinnahmen für einen Shop berechnen".
//
// Bewusst minimaler Erst-Port (M5a-Foundation):
//   • Verwendet alle bereits portierten Bausteine (TimeProfile, DemandUtils,
//     CompetitorEngine, BrandStats, Equipment, Employee).
//   • Campaigns/Upgrades/Combos liefern aktuell 0 (Modelle noch nicht portiert).
//     Die Engine-Methoden sind hier als TODO markiert und werden in M5b/M5c
//     ergänzt, ohne die Aufrufsignatur zu ändern.
//   • Global-Spieß-Upgrade-Bonus ist konstantentabelle aus Dart übernommen.

using System;
using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class GameEngineCore
    {
        // ── Globale Spieß-Upgrade-Konstanten ─────────────────────────────────
        // IDs entsprechen kGlobalSpiess*Id aus lib/core/constants.dart.
        public const string GlobalSpiessBasicId = "global_spiess_basic";
        public const string GlobalSpiessStandardId = "global_spiess_standard";
        public const string GlobalSpiessProfiId = "global_spiess_profi";

        private static readonly string[] GlobalSpiessUpgradeOrder =
        {
            GlobalSpiessBasicId, GlobalSpiessStandardId, GlobalSpiessProfiId
        };

        private static readonly Dictionary<string, double> GlobalSpiessQualityBonus = new()
        {
            [GlobalSpiessBasicId] = 0.15,
            [GlobalSpiessStandardId] = 0.40,
            [GlobalSpiessProfiId] = 0.80,
        };

        private static readonly Dictionary<string, int> GlobalSpiessCapacityBonus = new()
        {
            [GlobalSpiessBasicId] = 40,
            [GlobalSpiessStandardId] = 100,
            [GlobalSpiessProfiId] = 200,
        };

        // ──────────────────────────────────────────────────────────────────────
        // Hauptmethode: Vollständige Tages-Statistik in einem Rutsch
        // ──────────────────────────────────────────────────────────────────────

        public static double CalculateDailyRevenue(Shop shop, int? day = null, GameState state = null)
            => CalculateShopStats(shop, day, state).ActualRevenue;

        public static ShopDayStats CalculateShopStats(Shop shop, int? day = null, GameState state = null)
        {
            if (!shop.IsOpen || shop.Menu.Count == 0) return ShopDayStats.Zero();
            var activeMenu = shop.Menu.Where(p => p.IsActive).ToList();
            if (activeMenu.Count == 0) return ShopDayStats.Zero();

            var effectiveDay = day ?? shop.DayOpened;

            var reputationFactor = ReputationFactor(shop.Reputation);
            var baseCustomers = shop.FootTraffic * 0.06 * reputationFactor;
            var eqQuality = EquipmentQualityScore(shop, state);
            var staffMult = StaffQualityScore(shop);
            var capacity = CapacityLimit(shop, state);
            var variation = DailyVariation(shop, effectiveDay);

            var timeProfile = shop.TimeProfile;
            var weekday = ((effectiveDay % 7) + 7) % 7;
            var timeMult = timeProfile.DailyAverage(weekday);

            var brandMult = state?.Brand.CustomerMultiplier(shop.CityId) ?? 1.0;
            var compPressure = state == null
                ? 1.0
                : CompetitorEngine.CompetitionPressure(state, shop.CityId, shop.Reputation);

            // Story-Kampagnen-Perks aus abgeschlossenen Kapiteln (konzernweit).
            var perks = state == null
                ? new CampaignPerk()
                : CampaignData.AggregatePerks(state.CompletedChapterIds);

            // Combos: shop-spezifisch (greift nur wenn Shop alle Produkte führt).
            var comboBoost = state == null ? 0.0 : ComboService.CustomerBoost(shop, state);
            var comboAov = state == null ? 0.0 : ComboService.AvgOrderBoost(shop, state);

            // Permanente Upgrades (Shop-eigen + globale Konzern-Upgrades).
            var upgradeBoost = state == null ? 0.0 : UpgradeService.CustomerBoost(shop, state);
            var upgradeAov = state == null ? 0.0 : UpgradeService.AvgOrderBoost(shop, state);

            // Marketing-Kampagnen (Shop + Stadt + Konzern).
            var campaignBoost = MarketingService.TotalCustomerBoost(shop, effectiveDay, state);
            var campaignAov = MarketingService.ShopCampaignAvgOrderMod(shop, effectiveDay);

            var specialId = DemandUtils.DailySpecialProductId(effectiveDay);
            var season = DemandUtils.SeasonForDay(effectiveDay);

            double totalDemand = 0;
            double totalRevenue = 0;
            foreach (var sp in activeMenu)
            {
                var pd = ProductData(sp.ProductId);
                if (pd == null) continue;
                var demand = DemandUtils.PriceDemandFactor(
                    sp.Price, pd.BasePrice,
                    state?.Difficulty ?? GameDifficulty.Normal);
                if (sp.ProductId == specialId) demand *= DemandUtils.DailySpecialBoost;
                demand *= DemandUtils.SeasonCategoryMultiplier(season, pd.Category);
                totalDemand += demand;
                totalRevenue += demand * sp.Price *
                    (1.0 + campaignAov + upgradeAov + perks.AvgOrderBoost + comboAov);
            }

            // Safety-Guard: activeMenu ist nicht leer (oben geprüft), aber pd kann
            // null sein. Wenn ALLE pd null waren bleibt totalDemand = 0 → avgDemand=0.
            var avgDemand = totalDemand / activeMenu.Count;
            var avgOrderValue = totalDemand > 0 ? totalRevenue / totalDemand : 0;

            var rawCustomers = baseCustomers * eqQuality * staffMult * variation
                             * avgDemand * timeMult * brandMult * compPressure
                             * (1.0 + campaignBoost + upgradeBoost + perks.CustomerBoost + comboBoost);
            var actualCustomers = Math.Clamp(rawCustomers, 0.0, capacity);

            var actualRevenue = Math.Max(0.0, actualCustomers * avgOrderValue);
            var potentialRevenue = Math.Max(0.0, rawCustomers * avgOrderValue);

            return new ShopDayStats(
                actualRevenue: actualRevenue,
                potentialRevenue: potentialRevenue,
                actualCustomers: (int)Math.Round(actualCustomers, MidpointRounding.AwayFromZero),
                potentialCustomers: (int)Math.Round(rawCustomers, MidpointRounding.AwayFromZero),
                capacity: capacity,
                avgOrderValue: avgOrderValue);
        }

        /// <summary>14 Werte (10..23h), summiert auf actualCustomers.</summary>
        public static double[] HourlyCustomerCurve(Shop shop, int day)
        {
            if (!shop.IsOpen) return new double[14];
            var stats = CalculateShopStats(shop, day);
            if (stats.ActualCustomers == 0)
                return new double[shop.TimeProfile.HourlyFactors.Length];

            var profile = shop.TimeProfile;
            var weekday = ((day % 7) + 7) % 7;

            var hours = new double[profile.HourlyFactors.Length];
            double sum = 0;
            for (var h = 0; h < hours.Length; h++)
            {
                hours[h] = profile.Factor(weekday, h);
                sum += hours[h];
            }
            if (sum <= 0) return new double[profile.HourlyFactors.Length];

            for (var h = 0; h < hours.Length; h++)
                hours[h] = hours[h] / sum * stats.ActualCustomers;
            return hours;
        }

        public static int MaxEmployeesForShop(Shop shop)
        {
            var city = GameData.AllCities.FirstOrDefault(c => c.Id == shop.CityId)
                       ?? GameData.AllCities[0];
            return city.Tier switch
            {
                CityTier.Klein => 3,
                CityTier.Mittel => 5,
                CityTier.Gross => 7,
                CityTier.Metropole => 10,
                _ => 3,
            };
        }

        public static bool IsCapacityLimited(Shop shop, int? day = null, GameState state = null)
        {
            var stats = CalculateShopStats(shop, day, state);
            return stats.PotentialRevenue > stats.ActualRevenue * 1.05;
        }

        public static int RecommendedExtraEmployees(Shop shop, int? day = null, GameState state = null)
        {
            var stats = CalculateShopStats(shop, day, state);
            if (!IsCapacityLimited(shop, day, state)) return 0;
            var maxEmp = MaxEmployeesForShop(shop);
            var canAddMax = Math.Clamp(maxEmp - shop.Employees.Count, 0, 10);
            if (canAddMax == 0) return 0;
            var missing = stats.PotentialCustomers - stats.Capacity;
            var needed = Math.Clamp((int)Math.Ceiling(missing / 80.0), 1, canAddMax);
            return needed;
        }

        public static int CalculateDailyCustomers(Shop shop, int? day = null, GameState state = null)
            => CalculateShopStats(shop, day, state).ActualCustomers;

        public static int TotalCustomersToday(GameState state)
            => state.Shops.Sum(s => CalculateDailyCustomers(s, state.CurrentDay, state));

        // ──────────────────────────────────────────────────────────────────────
        // Stündlicher Tick (Live-Cash-Dripping über den Spieltag)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Stündlicher Umsatz = Tagesumsatz / Öffnungsstunden, über alle Filialen.</summary>
        public static double CalculateHourlyRevenue(GameState state)
            => state.Shops.Sum(s =>
                CalculateDailyRevenue(s, state.CurrentDay, state) / GameData.DailyOpenHours);

        /// <summary>
        /// Stündliche Kosten = Tageskosten / Öffnungsstunden (je Filiale) plus
        /// anteilige globale Upgrade- und Combo-Tageskosten.
        /// </summary>
        public static double CalculateHourlyCosts(GameState state)
        {
            var shopCosts = state.Shops.Sum(s =>
                CalculateDailyCosts(s, state.CurrentDay, state) / GameData.DailyOpenHours);
            var pressure = state.Modifiers.EconomicPressureMultiplier;
            var globalCosts = UpgradeService.GlobalUpgradeDailyCost(state) * pressure / GameData.DailyOpenHours;
            var comboCosts = DayProcessing.ActiveComboDailyCost(state) * pressure / GameData.DailyOpenHours;
            return shopCosts + globalCosts + comboCosts;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Tageskosten (Breakdown)
        // ──────────────────────────────────────────────────────────────────────
        //
        // Port aus calculateDailyCostsBreakdown. Berücksichtigt: Miete (×Pressure,
        // −Perk-RentSaving), Gehälter (×Pressure), Zutaten (Umsatz × Ratio ×
        // (1−Saving) × QualityMult × Pressure). Shop-Upgrades, Delivery-Provision
        // und Facility-Saving sind noch nicht portiert (M5c/M6) → aktuell 0.

        public static double CalculateDailyCosts(Shop shop, int? day = null, GameState state = null)
            => CalculateDailyCostsBreakdown(shop, day, state).Total;

        public static ShopCostBreakdown CalculateDailyCostsBreakdown(
            Shop shop, int? day = null, GameState state = null)
        {
            var pressure = state?.Modifiers.EconomicPressureMultiplier ?? 1.0;
            var perks = state == null
                ? new CampaignPerk()
                : CampaignData.AggregatePerks(state.CompletedChapterIds);

            var rent = shop.DailyRent * pressure * (1 - perks.RentSaving);
            var salaries = shop.Employees.Sum(e => e.SalaryPerDay) * pressure;
            // Shop-eigene Upgrade-Tageskosten (globale separat in DayProcessing).
            var upgrades = UpgradeService.ShopUpgradeDailyCost(shop) * pressure;

            if (shop.Menu.Count == 0)
                return new ShopCostBreakdown(rent, salaries, 0, upgrades, 0);

            var revenue = CalculateDailyRevenue(shop, day, state);
            var equipmentSaving = IngredientSavingBonus(shop);
            var facilitySaving = state == null ? 0.0 : FacilityEngine.FacilitySavingForShop(state, shop);
            var ingredientSaving = Math.Clamp(
                equipmentSaving + facilitySaving + perks.IngredientSaving, 0.0, 0.85);

            var activeMenu = shop.Menu.Where(p => p.IsActive).ToList();
            var ingredientRatio = WeightedIngredientRatio(activeMenu);
            var qualityMult = MenuIngredientQualityMult(shop, state);
            var ingredients = revenue * ingredientRatio * (1 - ingredientSaving)
                            * qualityMult * pressure;

            // Liefer-Provision (nie negativ, immer <= Umsatz × Pressure).
            var deliveryCommission = Math.Clamp(
                UpgradeService.DeliveryCommissionCost(shop, revenue, state),
                0.0, revenue * pressure);

            return new ShopCostBreakdown(rent, salaries, ingredients, upgrades, deliveryCommission);
        }

        public static double IngredientSavingBonus(Shop shop)
        {
            double sum = 0;
            foreach (var se in shop.Equipment)
            {
                var eq = GameCatalog.AllEquipment.FirstOrDefault(e => e.Id == se.EquipmentId);
                if (eq != null) sum += eq.IngredientSavingBonus;
            }
            return sum;
        }

        public static double WeightedIngredientRatio(List<ShopProduct> active)
        {
            if (active.Count == 0) return 0.35;
            double total = 0;
            var n = 0;
            foreach (var sp in active)
            {
                var pd = ProductData(sp.ProductId);
                if (pd == null) continue;
                if (sp.Price <= 0) continue;
                total += Math.Clamp(pd.IngredientCostPerUnit / sp.Price, 0.05, 0.9);
                n++;
            }
            return n == 0 ? 0.35 : total / n;
        }

        public static IngredientQuality ProductQualityOf(GameState state, string productId)
        {
            if (state == null) return IngredientQuality.Standard;
            return state.ProductQuality.TryGetValue(productId, out var name)
                ? IngredientQualityInfo.FromDart(name)
                : IngredientQuality.Standard;
        }

        /// <summary>Durchschnittlicher Zutaten-Quality-Multiplikator über aktive Produkte.</summary>
        public static double MenuIngredientQualityMult(Shop shop, GameState state)
        {
            var active = shop.Menu.Where(p => p.IsActive).ToList();
            if (active.Count == 0 || state == null) return 1.0;
            double sum = 0;
            foreach (var sp in active)
                sum += IngredientQualityInfo.IngredientMult(ProductQualityOf(state, sp.ProductId));
            return sum / active.Count;
        }

        /// <summary>Durchschnittlicher Reputations-Beitrag der Qualitätsniveaus.</summary>
        public static double MenuQualityReputation(Shop shop, GameState state)
        {
            var active = shop.Menu.Where(p => p.IsActive).ToList();
            if (active.Count == 0 || state == null) return 0.0;
            double sum = 0;
            foreach (var sp in active)
                sum += IngredientQualityInfo.ReputationPerDay(ProductQualityOf(state, sp.ProductId));
            return sum / active.Count;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Faktoren — public für Tests & spätere Engines
        // ──────────────────────────────────────────────────────────────────────

        public static double ReputationFactor(double rep)
            => Math.Clamp(0.4 + (rep / 5.0) * 1.0, 0.4, 1.4);

        public static double EquipmentQualityScore(Shop shop, GameState state = null)
        {
            var total = shop.Equipment.Count == 0 ? 0.5 : 1.0;
            foreach (var se in shop.Equipment)
            {
                var eq = GameCatalog.AllEquipment.FirstOrDefault(e => e.Id == se.EquipmentId);
                if (eq != null) total += eq.QualityBonus;
            }
            total += GlobalSpiessQualityFor(state);
            return Math.Clamp(total, 0.5, 2.5);
        }

        public static double StaffQualityScore(Shop shop)
        {
            if (shop.Employees.Count == 0) return 0.55;
            double score = 0.7;
            var hasMentor = shop.Employees.Any(e => e.HasTrait(PersonalityTrait.Mentor));
            var teamBonus = hasMentor ? 1.05 : 1.0;
            var hothead = shop.Employees.Any(e => e.HasTrait(PersonalityTrait.Hothead));
            var adjustedTeam = hothead ? teamBonus * 0.95 : teamBonus;

            foreach (var emp in shop.Employees)
            {
                score += 0.18 * emp.QualityFactor * adjustedTeam;
                score += 0.08 * emp.FriendlinessFactor;
            }
            return Math.Clamp(score, 0.55, 2.4);
        }

        public static int CapacityLimit(Shop shop, GameState state = null)
        {
            var cap = 20;
            foreach (var emp in shop.Employees)
            {
                cap += (int)Math.Round(40 + 80 * emp.SpeedFactor);
                cap += (int)Math.Round(20 * emp.ReliabilityFactor);
            }
            foreach (var se in shop.Equipment)
            {
                var eq = GameCatalog.AllEquipment.FirstOrDefault(e => e.Id == se.EquipmentId);
                if (eq != null) cap += eq.CapacityBonus;
            }
            cap += GlobalSpiessCapacityFor(state);
            return cap;
        }

        /// <summary>Deterministischer Tagesschwankungs-Faktor pro Shop+Day.</summary>
        public static double DailyVariation(Shop shop, int day)
        {
            // Dart: shop.id.hashCode ^ (day * 2654435761) — wir nutzen
            // string-stable hashing damit Tests reproduzierbar sind.
            var seed = unchecked(StableHash(shop.Id) ^ (int)(day * 2654435761u));
            var rng = new Random(seed);

            double avgReliability = 0.5;
            if (shop.Employees.Count > 0)
            {
                avgReliability = shop.Employees.Sum(e => e.ReliabilityFactor) / shop.Employees.Count;
            }
            var spread = 0.20 - avgReliability * 0.15;
            return (1.0 - spread) + rng.NextDouble() * (2 * spread);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Reputations-Update pro Tag
        // ──────────────────────────────────────────────────────────────────────
        //
        // Port aus _updateReputation. Berücksichtigt: Preis-Ratio-Scoring (mit
        // Difficulty-Penalty), Personal-Freundlichkeit + Charmer, Combo-Rep,
        // Zutaten-Quality-Rep. Marketing-Kampagnen-Rep und Shop-Upgrade-Rep
        // sind noch nicht portiert (M5c) → fließen aktuell nicht ein.

        public static double UpdateReputation(Shop shop, GameState state)
        {
            if (shop.Menu.Count == 0) return shop.Reputation;

            var penaltyMultiplier = state.Modifiers.ReputationPenaltyMultiplier;
            double sumScore = 0;
            var n = 0;
            foreach (var sp in shop.Menu.Where(p => p.IsActive))
            {
                var pd = ProductData(sp.ProductId);
                if (pd == null) continue;
                var ratio = sp.Price / pd.BasePrice;
                double s;
                if (ratio <= 0.9) s = 0.05;
                else if (ratio <= 1.1) s = 0.02;
                else if (ratio <= 1.3) s = -0.04;
                else if (ratio <= 1.6) s = -0.12;
                else s = -0.25;

                if (s < 0) s *= penaltyMultiplier;
                else if (penaltyMultiplier > 1.0) s /= Math.Sqrt(penaltyMultiplier);

                sumScore += s;
                n++;
            }

            if (shop.Employees.Count > 0)
            {
                var avgFriend = shop.Employees.Sum(e => e.FriendlinessFactor) / shop.Employees.Count;
                sumScore += (avgFriend - 0.3) * 0.10;
                var charmers = shop.Employees.Count(e => e.HasTrait(PersonalityTrait.Charmer));
                sumScore += charmers * 0.03;
            }
            else
            {
                sumScore -= 0.03 * penaltyMultiplier;
            }

            // Marketing-Kampagnen-Rep (shop/city/global).
            sumScore += MarketingService.ReputationPerDay(shop, state, state.CurrentDay);

            sumScore += UpgradeService.ReputationPerDay(shop, state);
            sumScore += ComboService.ReputationPerDay(shop, state);
            sumScore += MenuQualityReputation(shop, state);

            if (n == 0) return shop.Reputation;
            var delta = sumScore / n;
            return Math.Clamp(shop.Reputation + delta, 0.5, 5.0);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private static ProductData ProductData(string productId)
            => GameData.AllProducts.FirstOrDefault(p => p.Id == productId);

        private static string ActiveGlobalSpiessUpgradeId(GameState state)
        {
            if (state == null) return null;
            for (var i = GlobalSpiessUpgradeOrder.Length - 1; i >= 0; i--)
            {
                var id = GlobalSpiessUpgradeOrder[i];
                if (state.GlobalUpgradeIds.Contains(id)) return id;
            }
            return null;
        }

        private static double GlobalSpiessQualityFor(GameState state)
        {
            var id = ActiveGlobalSpiessUpgradeId(state);
            if (id == null) return 0;
            return GlobalSpiessQualityBonus.TryGetValue(id, out var v) ? v : 0;
        }

        private static int GlobalSpiessCapacityFor(GameState state)
        {
            var id = ActiveGlobalSpiessUpgradeId(state);
            if (id == null) return 0;
            return GlobalSpiessCapacityBonus.TryGetValue(id, out var v) ? v : 0;
        }

        /// <summary>
        /// Stabiler 32-Bit-Hash für Strings (FNV-1a). C#'s String.GetHashCode ist
        /// nicht stabil über Prozess-Restarts hinweg, deshalb eigener Algorithmus
        /// für deterministische Day-Simulation.
        /// </summary>
        private static int StableHash(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            unchecked
            {
                uint hash = 2166136261;
                foreach (var c in s) hash = (hash ^ c) * 16777619;
                return (int)hash;
            }
        }
    }
}
