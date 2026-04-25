using System;
using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
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

        public int ActiveMoneysCount { get; set; }
        public int OverdueMoneysCount { get; set; }
        public int PaidOffMoneysCount { get; set; }

        public int TotalBorrowersCount { get; set; }
        public int ActiveBorrowersCount { get; set; }
        public int NewBorrowersThisMonth { get; set; }

        public decimal CollectionRate { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal AverageMoneySize { get; set; }
        public decimal AverageInterestRate { get; set; }

        // Trends (compared to previous period)
        public decimal TotalLentGrowth { get; set; }
        public decimal CollectionRateChange { get; set; }
        public int NewMoneysThisMonth { get; set; }
        public int NewPaymentsThisMonth { get; set; }
    }
}