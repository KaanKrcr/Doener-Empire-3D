// Döner Empire 3D — Kredit
// Port aus lib/models/game_state.dart (Loan).

using System;

namespace DoenerEmpire.Models
{
    public sealed class Loan
    {
        public string Id;
        public double Amount;
        public double InterestRate;   // z.B. 0.06 = 6% p.a.
        public int DurationDays;
        public int DayTaken;
        public double AmountPaid = 0;

        /// <summary>Kredit + Zinsen, die ursprünglich zurückzuzahlen sind.</summary>
        public double TotalRepayment => Amount * (1 + InterestRate * (DurationDays / 365.0));

        public double DailyPayment => TotalRepayment / DurationDays;

        public double RemainingDebt => Math.Max(0.0, TotalRepayment - AmountPaid);

        /// <summary>
        /// Vollständige Ablösung heute: Restschuld minus 50% der noch nicht
        /// angefallenen Zinsen (frühere Tilgung = weniger Zinsen).
        /// </summary>
        public double EarlyPayoffAmount(int currentDay)
        {
            var daysElapsed = Math.Clamp(currentDay - DayTaken, 0, DurationDays);
            var totalInterest = Amount * InterestRate * (DurationDays / 365.0);
            var paidInterestShare = ((double)daysElapsed / DurationDays) * totalInterest;
            var futureInterest = totalInterest - paidInterestShare;
            var discount = futureInterest * 0.5;
            var remaining = TotalRepayment - AmountPaid - discount;
            return Math.Max(0.0, remaining);
        }

        public int RemainingDays(int currentDay)
        {
            var daysElapsed = Math.Clamp(currentDay - DayTaken, 0, DurationDays);
            return DurationDays - daysElapsed;
        }

        public double Progress => Math.Clamp(AmountPaid / TotalRepayment, 0.0, 1.0);

        public bool IsPaidOff => RemainingDebt <= 0.01;

        public Loan Clone() => new()
        {
            Id = Id, Amount = Amount, InterestRate = InterestRate,
            DurationDays = DurationDays, DayTaken = DayTaken, AmountPaid = AmountPaid,
        };
    }
}
