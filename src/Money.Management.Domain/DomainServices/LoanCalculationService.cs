using Money.Management.Entities;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Services;

namespace Money.Management.DomainServices
{
    /// <summary>
    /// Pure-calculation domain service. No repository dependencies — all methods are
    /// deterministic given their inputs so they can be called from the application layer
    /// or unit-tested without infrastructure.
    /// </summary>
    public class LoanCalculationService : DomainService
    {
        // ── Interest ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates simple interest: I = P × R × T
        /// where R is the annual rate as a percentage and T is in years.
        /// </summary>
        public decimal CalculateSimpleInterest(decimal principal, decimal annualRatePct, decimal termYears)
        {
            ValidateInputs(principal, annualRatePct);
            return principal * (annualRatePct / 100m) * termYears;
        }

        /// <summary>
        /// Calculates compound interest: A = P × (1 + R/n)^(n×T) − P
        /// </summary>
        /// <param name="compoundsPerYear">Typically 12 (monthly) or 1 (annual).</param>
        public decimal CalculateCompoundInterest(
            decimal principal,
            decimal annualRatePct,
            decimal termYears,
            int compoundsPerYear = 12)
        {
            ValidateInputs(principal, annualRatePct);

            if (compoundsPerYear <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(compoundsPerYear), "Compounds per year must be greater than 0.");

            var r = (double)(annualRatePct / 100m);
            var n = (double)compoundsPerYear;
            var t = (double)termYears;

            var amount = (double)principal * Math.Pow(1 + r / n, n * t);
            return (decimal)amount - principal;
        }

        /// <summary>
        /// Calculates total loan amount with simple interest.
        /// </summary>
        public decimal CalculateTotalAmountSimple(
            decimal principal, decimal annualRatePct, decimal termYears)
            => principal + CalculateSimpleInterest(principal, annualRatePct, termYears);

        /// <summary>
        /// Calculates total loan amount with compound interest.
        /// </summary>
        public decimal CalculateTotalAmountCompound(
            decimal principal, decimal annualRatePct, decimal termYears,
            int compoundsPerYear = 12)
            => principal + CalculateCompoundInterest(
                principal, annualRatePct, termYears, compoundsPerYear);

        // ── Monthly instalment (EMI) ──────────────────────────────────────────────

        /// <summary>
        /// Equal Monthly Instalment: EMI = P × r × (1+r)^n / ((1+r)^n − 1)
        /// where r = monthly interest rate, n = total months.
        /// Returns the principal amount divided by months when the rate is 0.
        /// </summary>
        public decimal CalculateMonthlyInstalment(
            decimal principal, decimal annualRatePct, int termMonths)
        {
            ValidateInputs(principal, annualRatePct);

            if (termMonths <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(termMonths), "Term in months must be greater than 0.");

            if (annualRatePct == 0m)
                return Math.Round(principal / termMonths, 2, MidpointRounding.AwayFromZero);

            var r = (double)(annualRatePct / 100m / 12m);
            var n = (double)termMonths;
            var p = (double)principal;

            var emi = p * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
            return Math.Round((decimal)emi, 2, MidpointRounding.AwayFromZero);
        }

        // ── Amortization schedule ─────────────────────────────────────────────────

        /// <summary>
        /// Generates a full month-by-month amortization schedule showing exactly how
        /// each EMI is split between principal and interest.
        /// </summary>
        public AmortizationSchedule GenerateAmortizationSchedule(
            decimal principal,
            decimal annualRatePct,
            int termMonths,
            DateTime startDate)
        {
            ValidateInputs(principal, annualRatePct);

            if (termMonths <= 0)
                throw new ArgumentOutOfRangeException(nameof(termMonths));

            var emi = CalculateMonthlyInstalment(principal, annualRatePct, termMonths);
            var monthlyRate = annualRatePct / 100m / 12m;

            var entries = new List<AmortizationEntry>(termMonths);
            var balance = principal;

            for (var month = 1; month <= termMonths; month++)
            {
                var interestPortion = Math.Round(balance * monthlyRate, 2,
                    MidpointRounding.AwayFromZero);

                var principalPortion = emi - interestPortion;

                // Last instalment adjustment to clear floating-point residue
                if (month == termMonths)
                {
                    principalPortion = balance;
                    emi = principalPortion + interestPortion;
                }

                var closingBalance = Math.Max(0m, balance - principalPortion);

                entries.Add(new AmortizationEntry
                {
                    Month = month,
                    DueDate = startDate.AddMonths(month),
                    OpeningBalance = Math.Round(balance, 2),
                    Emi = Math.Round(emi, 2),
                    InterestPortion = interestPortion,
                    PrincipalPortion = Math.Round(principalPortion, 2),
                    ClosingBalance = Math.Round(closingBalance, 2)
                });

                balance = closingBalance;
            }

            return new AmortizationSchedule(principal, annualRatePct, termMonths, emi, entries);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void ValidateInputs(decimal principal, decimal annualRatePct)
        {
            if (principal <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(principal), "Principal must be greater than 0.");

            if (annualRatePct < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(annualRatePct), "Interest rate cannot be negative.");
        }
    }

    // ── Value objects returned by the service ─────────────────────────────────────

    /// <summary>Full amortization schedule for a loan.</summary>
    public sealed class AmortizationSchedule
    {
        public decimal Principal { get; }
        public decimal AnnualRatePct { get; }
        public int TermMonths { get; }
        public decimal MonthlyInstalment { get; }
        public IReadOnlyList<AmortizationEntry> Entries { get; }

        public decimal TotalPayment => MonthlyInstalment * TermMonths;
        public decimal TotalInterest => TotalPayment - Principal;

        internal AmortizationSchedule(
            decimal principal,
            decimal annualRatePct,
            int termMonths,
            decimal monthlyInstalment,
            IReadOnlyList<AmortizationEntry> entries)
        {
            Principal = principal;
            AnnualRatePct = annualRatePct;
            TermMonths = termMonths;
            MonthlyInstalment = monthlyInstalment;
            Entries = entries;
        }
    }

    /// <summary>A single row in the amortization schedule.</summary>
    public sealed class AmortizationEntry
    {
        public int Month { get; init; }
        public DateTime DueDate { get; init; }
        public decimal OpeningBalance { get; init; }
        public decimal Emi { get; init; }
        public decimal InterestPortion { get; init; }
        public decimal PrincipalPortion { get; init; }
        public decimal ClosingBalance { get; init; }
    }
}
