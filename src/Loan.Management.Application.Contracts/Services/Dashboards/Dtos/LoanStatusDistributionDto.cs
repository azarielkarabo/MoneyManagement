using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Loan distribution by status
    /// </summary>
    public class LoanStatusDistributionDto
    {
        public int ActiveCount { get; set; }
        public int OverdueCount { get; set; }
        public int PaidOffCount { get; set; }
        public int CancelledCount { get; set; }
        public int WriteOffCount { get; set; }

        public decimal ActiveAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PaidOffAmount { get; set; }
        public decimal CancelledAmount { get; set; }
        public decimal WriteOffAmount { get; set; }

        public decimal ActivePercentage { get; set; }
        public decimal OverduePercentage { get; set; }
        public decimal PaidOffPercentage { get; set; }
    }
}