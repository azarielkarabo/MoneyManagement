using System.Collections.Generic;

namespace Money.Management.Services.Reports.Dtos
{
    public class ReportSummaryDto
    {
        // Loan summary
        public int TotalLoans { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalRepaid { get; set; }

        // Budget summary
        public decimal AverageMonthlySalary { get; set; }
        public decimal AverageMonthlyExpenses { get; set; }
        public decimal AverageMonthlySavings { get; set; }

        public List<MonthlyTrendDto> LoanRepaymentTrend { get; set; } = new();
        public List<MonthlyTrendDto> BudgetExpenseTrend { get; set; } = new();
    }
}
