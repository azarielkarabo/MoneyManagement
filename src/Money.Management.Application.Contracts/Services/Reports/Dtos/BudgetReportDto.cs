using System.Collections.Generic;

namespace Money.Management.Services.Reports.Dtos
{
    public class BudgetReportDto
    {
        public int Year { get; set; }

        public List<MonthlySpendingDto> MonthlyBreakdown { get; set; } = new();
        public List<CategoryExpenseDto> CategoryBreakdown { get; set; } = new();
        public List<MonthlyTrendDto> SavingsTrend { get; set; } = new();

        public class MonthlySpendingDto
        {
            public int Month { get; set; }
            public string MonthLabel { get; set; }
            public decimal Salary { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal TotalSavings { get; set; }
            public decimal RemainingBalance { get; set; }
        }

        public class CategoryExpenseDto
        {
            public string CategoryName { get; set; }
            public decimal TotalAmount { get; set; }
            public int Count { get; set; }
        }
    }
}
