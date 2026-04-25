using System;
using System.Collections.Generic;

namespace Money.Management.Services.Reports.Dtos
{
    public class LoanReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
        public int DefaultedLoans { get; set; }

        public decimal TotalDisbursed { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalRepaid { get; set; }

        public List<MonthlyTrendDto> RepaymentTrend { get; set; } = new();
    }
}
