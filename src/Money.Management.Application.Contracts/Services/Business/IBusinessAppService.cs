using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Money.Management.Enums;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Business
{
    /// <summary>
    /// The current lender's business: its balance (capital), growth and this month's forecast.
    /// Exposed as /api/app/business/...
    /// </summary>
    public interface IBusinessAppService : IApplicationService
    {
        /// <summary>GET /api/app/business/summary?months=6</summary>
        Task<BusinessSummaryDto> GetSummaryAsync(int months = 6);

        /// <summary>GET /api/app/business/capital-transactions</summary>
        Task<List<CapitalTransactionDto>> GetCapitalTransactionsAsync();

        /// <summary>POST /api/app/business/capital-transaction</summary>
        Task<CapitalTransactionDto> CreateCapitalTransactionAsync(CreateCapitalTransactionDto input);

        /// <summary>DELETE /api/app/business/{id}/capital-transaction</summary>
        Task DeleteCapitalTransactionAsync(Guid id);
    }

    public class CapitalTransactionDto
    {
        public Guid Id { get; set; }
        public CapitalTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateCapitalTransactionDto
    {
        public CapitalTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }
    }

    public class BusinessSummaryDto
    {
        /// <summary>False until the lender sets their opening balance; the other figures are then incomplete.</summary>
        public bool HasOpeningBalance { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime? OpeningDate { get; set; }

        public decimal CapitalAdded { get; set; }
        public decimal CapitalWithdrawn { get; set; }
        /// <summary>Opening balance + added − withdrawn: the lender's own money in the business.</summary>
        public decimal NetCapital { get; set; }

        public decimal TotalLent { get; set; }
        public decimal TotalRepaid { get; set; }

        /// <summary>Net capital − lent + repaid.</summary>
        public decimal CashOnHand { get; set; }
        /// <summary>Still owed by borrowers on open loans.</summary>
        public decimal OutstandingOwed { get; set; }
        /// <summary>Cash on hand + owed.</summary>
        public decimal BusinessValue { get; set; }

        /// <summary>Business value − net capital.</summary>
        public decimal GrowthAmount { get; set; }
        public decimal GrowthPercent { get; set; }

        public List<BusinessMonthDto> Months { get; set; } = new();
        public BusinessForecastDto Forecast { get; set; } = new();
    }

    /// <summary>Position at the end of a month (the current month: as of today).</summary>
    public class BusinessMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal CashOnHand { get; set; }
        public decimal OutstandingOwed { get; set; }
        public decimal BusinessValue { get; set; }
        public decimal LentInMonth { get; set; }
        public decimal RepaidInMonth { get; set; }
    }

    /// <summary>What the lender can expect this calendar month.</summary>
    public class BusinessForecastDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        /// <summary>Received this month + still owed on loans due this month.</summary>
        public decimal ExpectedCollections { get; set; }
        public decimal ReceivedThisMonth { get; set; }
        public decimal RemainingToCollect { get; set; }

        /// <summary>The interest share of <see cref="ExpectedCollections"/>.</summary>
        public decimal ExpectedProfit { get; set; }
        public decimal EarnedThisMonth { get; set; }

        public int LoansDueCount { get; set; }

        /// <summary>Still owed on loans that were due before this month (not counted above).</summary>
        public decimal OverdueOutstanding { get; set; }
        public int OverdueLoansCount { get; set; }
    }
}
