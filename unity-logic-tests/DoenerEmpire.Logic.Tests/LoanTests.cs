using Xunit;
using DoenerEmpire.Models;

namespace DoenerEmpire.Logic.Tests
{
    public class LoanTests
    {
        // 20.000 € @ 6% p.a. über 90 Tage (Betriebskredit aus bank_screen).
        private static Loan SampleLoan() => new()
        {
            Id = "l1", Amount = 20000, InterestRate = 0.06, DurationDays = 90, DayTaken = 10,
        };

        [Fact]
        public void TotalRepaymentIncludesInterest()
        {
            var l = SampleLoan();
            // 20000 * (1 + 0.06 * 90/365) = 20295.8904...
            Assert.Equal(20295.89, l.TotalRepayment, 2);
            Assert.Equal(20295.89 / 90, l.DailyPayment, 2);
        }

        [Fact]
        public void EarlyPayoffDiscountsHalfOfFutureInterest()
        {
            var l = SampleLoan();
            // Sofortige Ablösung am Aufnahmetag: 50% der Zinsen (295.8904) erlassen.
            // remaining = 20295.8904 - 147.9452 = 20147.9452
            Assert.Equal(20147.95, l.EarlyPayoffAmount(10), 2);
            // Ablösung ist immer günstiger als die volle Rückzahlung.
            Assert.True(l.EarlyPayoffAmount(10) < l.TotalRepayment);
        }

        [Fact]
        public void EarlyPayoffApproachesRemainingDebtNearEnd()
        {
            var l = SampleLoan();
            // Am Laufzeitende sind kaum Zukunftszinsen übrig → Rabatt ~0.
            var payoffEnd = l.EarlyPayoffAmount(100); // dayTaken+duration
            Assert.Equal(l.RemainingDebt, payoffEnd, 2);
        }

        [Fact]
        public void RemainingDaysCountsDown()
        {
            var l = SampleLoan();
            Assert.Equal(90, l.RemainingDays(10));
            Assert.Equal(45, l.RemainingDays(55));
            Assert.Equal(0, l.RemainingDays(100));
            Assert.Equal(0, l.RemainingDays(120)); // nicht negativ
        }

        [Fact]
        public void ProgressAndPaidOff()
        {
            var l = SampleLoan();
            Assert.Equal(0.0, l.Progress, 3);
            Assert.False(l.IsPaidOff);
            l.AmountPaid = l.TotalRepayment;
            Assert.Equal(1.0, l.Progress, 3);
            Assert.True(l.IsPaidOff);
            Assert.Equal(0.0, l.RemainingDebt, 3);
        }

        [Fact]
        public void DailyRecordProfitBreakdown()
        {
            var r = new DailyRecord
            {
                Day = 3, Revenue = 1000, Costs = 600,
                LoanPayments = 50, Investments = 200,
            };
            Assert.Equal(400, r.OperatingProfit);     // 1000 - 600
            Assert.Equal(150, r.Profit);              // 1000 - 600 - 50 - 200
        }
    }
}
