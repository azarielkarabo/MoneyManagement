namespace Money.Management.Services.Budgets.Dtos
{
    public class CashFlowReportDto
    {
        public decimal Salary { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalInvestments { get; set; }
        public decimal RemainingBalance { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
