// Döner Empire 3D — Tagesabschluss-Datensatz
// Port aus lib/models/game_state.dart (DailyRecord).

namespace DoenerEmpire.Models
{
    public sealed class DailyRecord
    {
        public int Day;
        public double Revenue;
        public double Costs;                    // operative Tageskosten gesamt
        public int Customers;
        public double RentCosts;
        public double SalaryCosts;
        public double IngredientCosts;
        public double DeliveryCommissionCosts;
        public double LoanPayments;
        public double Investments;              // einmalige Ausgaben

        public double Profit => Revenue - Costs - LoanPayments - Investments;
        public double OperatingProfit => Revenue - Costs;
    }
}
