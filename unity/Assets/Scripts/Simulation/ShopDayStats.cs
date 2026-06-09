// Döner Empire 3D — Ergebnis einer Tagessimulation für eine Filiale
// Port aus lib/services/game_engine.dart (ShopDayStats).

namespace DoenerEmpire.Simulation
{
    public readonly struct ShopDayStats
    {
        public readonly double ActualRevenue;
        public readonly double PotentialRevenue;
        public readonly int ActualCustomers;
        public readonly int PotentialCustomers;
        public readonly int Capacity;
        public readonly double AvgOrderValue;

        public ShopDayStats(double actualRevenue, double potentialRevenue,
                            int actualCustomers, int potentialCustomers,
                            int capacity, double avgOrderValue)
        {
            ActualRevenue = actualRevenue;
            PotentialRevenue = potentialRevenue;
            ActualCustomers = actualCustomers;
            PotentialCustomers = potentialCustomers;
            Capacity = capacity;
            AvgOrderValue = avgOrderValue;
        }

        public static ShopDayStats Zero() => new(0, 0, 0, 0, 0, 0);
    }

    /// <summary>
    /// Aufschlüsselung der Tageskosten einer Filiale. Delivery-Provision wird
    /// separat ausgewiesen, damit Umsatz nie durch diesen Posten negativ wird.
    /// Port aus lib/services/game_engine.dart (ShopCostBreakdown).
    /// </summary>
    public readonly struct ShopCostBreakdown
    {
        public readonly double Rent;
        public readonly double Salaries;
        public readonly double Ingredients;
        public readonly double Upgrades;
        public readonly double DeliveryCommission;

        public ShopCostBreakdown(double rent, double salaries, double ingredients,
                                 double upgrades = 0, double deliveryCommission = 0)
        {
            Rent = rent;
            Salaries = salaries;
            Ingredients = ingredients;
            Upgrades = upgrades;
            DeliveryCommission = deliveryCommission;
        }

        public double Total => Rent + Salaries + Ingredients + Upgrades + DeliveryCommission;
    }
}
