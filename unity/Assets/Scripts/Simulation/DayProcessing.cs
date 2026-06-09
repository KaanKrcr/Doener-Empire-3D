// Döner Empire 3D — Tagesabschluss-Helfer (City-Unlocks, Combo-Kosten, Brand)
// Port aus lib/services/game_engine.dart (Teile von processDay + _updateBrand
// + _checkCityUnlocks + activeComboDailyCost + globalUpgradeDailyCost).
//
// Der volle processDay() braucht CorporateEngine (Facilities/Stocks/Auto-Hire),
// Loans und den Marketing-Katalog — diese folgen in M6. Hier sind die
// vollständig isolierbaren, deterministischen Bausteine portiert und getestet.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    /// <summary>Ergebnis eines Tagesabschlusses (für UI-Reports).</summary>
    public sealed class DayResult
    {
        public GameState State;
        public DailyRecord Record;
        public List<string> NewlyUnlockedCities = new();
    }

    public static class DayProcessing
    {
        /// <summary>
        /// Kern-Tagesabschluss: führt alle bereits portierten Subsysteme zusammen
        /// (Wettbewerb, Umsatz/Kosten, Reputation, Employee-XP, Kampagnen-Ablauf,
        /// Loans, Brand, City-Unlocks). Mutiert und liefert den State zurück.
        ///
        /// NICHT enthalten (CorporateEngine-Port M7 ausstehend):
        ///   • Facility-Kosten/-B2B-Umsatz
        ///   • Stocks-Tagespreis-Update
        ///   • HR-Manager-XP-Progress
        ///   • Auto-Pricing / Auto-Hire
        /// Diese Posten fließen daher (noch) nicht in Cash/History ein.
        /// </summary>
        public static DayResult ProcessDay(GameState state)
        {
            var today = state.CurrentDay;
            var mods = state.Modifiers;

            // 1) Wettbewerb aktualisieren (mutiert state.Competitors).
            CompetitorEngine.ProcessDay(state);

            double totalRevenue = 0, totalRent = 0, totalSalaries = 0;
            double totalIngredients = 0, totalDeliveryCommission = 0;
            var totalCustomers = 0;

            var trainingGrowth = HrEngine.TrainingGrowthMultiplier(state);

            foreach (var shop in state.Shops)
            {
                var revenue = GameEngineCore.CalculateDailyRevenue(shop, today, state);
                var br = GameEngineCore.CalculateDailyCostsBreakdown(shop, today, state);
                var customers = GameEngineCore.CalculateDailyCustomers(shop, today, state);

                totalRevenue += revenue;
                totalRent += br.Rent;
                totalSalaries += br.Salaries;
                totalIngredients += br.Ingredients;
                totalDeliveryCommission += br.DeliveryCommission;
                totalCustomers += customers;

                shop.Reputation = GameEngineCore.UpdateReputation(shop, state);

                foreach (var emp in shop.Employees)
                {
                    emp.DaysEmployed += 1;
                    var expInterval = HrEngine.XpIntervalDays(
                        state.Difficulty, trainingGrowth, emp.GrowthPotential);
                    if (emp.DaysEmployed % expInterval == 0 && emp.Experience < 10)
                        emp.Experience += 1;
                }

                // Abgelaufene Shop-Kampagnen entfernen (Ablauf am Folgetag).
                shop.ActiveCampaigns.RemoveAll(c => !c.IsActive(today + 1));
            }

            // Abgelaufene Stadt-/Konzern-Kampagnen entfernen.
            foreach (var kv in state.ActiveCityCampaigns)
                kv.Value.RemoveAll(c => !c.IsActive(today + 1));
            state.ActiveGlobalCampaigns.RemoveAll(c => !c.IsActive(today + 1));

            // Konzernweite Tageskosten.
            var pressure = mods.EconomicPressureMultiplier;
            var hrDailySalary = (state.HrManager?.SalaryPerDay ?? 0) * pressure;
            var globalUpgradeCost = UpgradeService.GlobalUpgradeDailyCost(state) * pressure;
            var comboCost = ActiveComboDailyCost(state) * pressure;
            var totalCosts = totalRent + totalSalaries + hrDailySalary
                           + totalIngredients + totalDeliveryCommission
                           + globalUpgradeCost + comboCost;

            // Kreditraten.
            double loanPayments = 0;
            foreach (var loan in state.Loans)
            {
                if (loan.IsPaidOff) continue;
                var payment = loan.DailyPayment;
                loan.AmountPaid += payment;
                loanPayments += payment;
            }
            state.Loans.RemoveAll(l => l.IsPaidOff);

            // Produktionsanlagen: Betriebskosten + B2B-Umsatz (netto separat).
            var facilityCost = FacilityEngine.FacilityDailyCosts(state);
            var facilityRevenue = FacilityEngine.FacilityB2BRevenue(state);
            var facilityNet = facilityRevenue - facilityCost;

            var netCash = totalRevenue - totalCosts - loanPayments;
            state.Cash += netCash + facilityNet;

            var record = new DailyRecord
            {
                Day = today,
                Revenue = totalRevenue,
                Costs = totalCosts,
                Customers = totalCustomers,
                RentCosts = totalRent,
                SalaryCosts = totalSalaries + hrDailySalary,
                IngredientCosts = totalIngredients,
                DeliveryCommissionCosts = totalDeliveryCommission,
                LoanPayments = loanPayments,
                Investments = 0,
            };
            state.History.Add(record);
            if (state.History.Count > 60)
                state.History.RemoveRange(0, state.History.Count - 60);

            // Fortschritt: Gesamtumsatz (inkl. B2B) + City-Unlocks.
            var progressRevenue = (totalRevenue + facilityRevenue) * mods.ProgressSpeedMultiplier;
            state.TotalRevenue += progressRevenue;
            state.TotalProfit += netCash + facilityNet;

            var before = new HashSet<string>(state.UnlockedCityIds);
            state.UnlockedCityIds = CheckCityUnlocks(state.UnlockedCityIds, state.TotalRevenue);
            var newlyUnlocked = state.UnlockedCityIds.Where(c => !before.Contains(c)).ToList();

            // Brand-Update.
            state.Brand = UpdateBrand(state, totalRevenue, totalCustomers, state.Shops);

            // Aktienkurs täglich aktualisieren (nur wenn an der Börse).
            CorporateStocksEngine.UpdateDailyPrice(state);

            // TODO(M7b/c): Facilities-Kosten/B2B, HR-Manager-XP-Progress,
            // Auto-Pricing/Auto-Hire (CorporateEngine-Rest).

            state.CurrentDay = today + 1;

            return new DayResult
            {
                State = state,
                Record = record,
                NewlyUnlockedCities = newlyUnlocked,
            };
        }

        /// <summary>
        /// Schaltet Städte frei, deren UnlockCost durch den Gesamtumsatz erreicht
        /// ist. Liefert eine neue Liste (kostenpflichtige Städte; Startstädte mit
        /// UnlockCost 0 werden hier nicht hinzugefügt).
        /// </summary>
        public static List<string> CheckCityUnlocks(IReadOnlyList<string> current, double totalRevenue)
        {
            var newList = new List<string>(current);
            foreach (var city in GameData.AllCities)
            {
                if (!newList.Contains(city.Id) &&
                    totalRevenue >= city.UnlockCost &&
                    city.UnlockCost > 0)
                {
                    newList.Add(city.Id);
                }
            }
            return newList;
        }

        /// <summary>Konzernweite Tagespauschale aller aktiven Combos.</summary>
        public static double ActiveComboDailyCost(GameState state)
        {
            double cost = 0;
            foreach (var id in state.ActiveComboIds)
            {
                var c = ComboData.ById(id);
                if (c != null) cost += c.DailyCost;
            }
            return cost;
        }

        /// <summary>
        /// Brand-Update: Awareness wächst mit Tagesaktivität (Kunden + Umsatz),
        /// City-Reputation pro aktiver Filiale je Stadt.
        ///
        /// HINWEIS: Marketing-Kampagnen-Deltas und Upgrade-Brand-Boni sind noch
        /// nicht portiert (M5c/M6) und fließen daher (noch) nicht ein.
        /// </summary>
        public static BrandStats UpdateBrand(
            GameState state,
            double dailyRevenue,
            int dailyCustomers,
            IReadOnlyList<Shop> shops)
        {
            var brand = state.Brand;
            var progressSpeed = state.Modifiers.ProgressSpeedMultiplier;

            var newAwareness = brand.BrandAwareness +
                ((dailyCustomers / 100.0) * 0.02 + (dailyRevenue / 1000.0) * 0.005) * progressSpeed;

            // Upgrade-Brand-Boni (Loyalty-App, Social-Media-Team etc.).
            foreach (var shop in shops)
                newAwareness += UpgradeService.BrandPerDay(shop, state);

            // Marketing-Kampagnen-Brand-Deltas (city + global).
            newAwareness += MarketingService.BrandAwarenessDelta(state, state.CurrentDay, shops);

            // Plateau: über 30 wächst es deutlich langsamer.
            if (newAwareness > 30)
            {
                var overshoot = newAwareness - 30;
                newAwareness = 30 + overshoot * 0.4;
            }
            newAwareness = System.Math.Clamp(newAwareness, 0.0, 100.0);

            var newCityRep = new Dictionary<string, double>(brand.CityReputation);
            var shopsByCity = new Dictionary<string, List<Shop>>();
            foreach (var s in shops)
            {
                if (!shopsByCity.TryGetValue(s.CityId, out var list))
                {
                    list = new List<Shop>();
                    shopsByCity[s.CityId] = list;
                }
                list.Add(s);
            }
            foreach (var kv in shopsByCity)
            {
                var current = newCityRep.TryGetValue(kv.Key, out var v) ? v : 0;
                var avgRep = kv.Value.Sum(sh => sh.Reputation) / kv.Value.Count;
                var delta = (avgRep - 2.5) * 0.5;
                delta += kv.Value.Count * 0.2;
                delta *= progressSpeed;
                newCityRep[kv.Key] = System.Math.Clamp(current + delta, 0.0, 100.0);
            }

            return new BrandStats
            {
                BrandAwareness = newAwareness,
                CityReputation = newCityRep,
            };
        }
    }
}
