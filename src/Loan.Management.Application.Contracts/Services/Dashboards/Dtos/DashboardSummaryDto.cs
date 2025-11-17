using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Summary statistics for dashboard overview
    /// </summary>
    public class DashboardSummaryDto
    {
        public decimal TotalLent { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalInterestEarned { get; set; }

        public int ActiveLoansCount { get; set; }
        public int OverdueLoansCount { get; set; }
        public int PaidOffLoansCount { get; set; }

        public int TotalBorrowersCount { get; set; }
        public int ActiveBorrowersCount { get; set; }
        public int NewBorrowersThisMonth { get; set; }

        public decimal CollectionRate { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal AverageLoanSize { get; set; }
        public decimal AverageInterestRate { get; set; }

        // Trends (compared to previous period)
        public decimal TotalLentGrowth { get; set; }
        public decimal CollectionRateChange { get; set; }
        public int NewLoansThisMonth { get; set; }
        public int NewPaymentsThisMonth { get; set; }
    }
}