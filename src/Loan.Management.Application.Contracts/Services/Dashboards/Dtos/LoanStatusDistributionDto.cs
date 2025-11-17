namespace Loan.Management.Services.Dashboards.Dtos
{
    public class LoanStatusDistributionDto
    {
        public int ActiveCount { get; set; }
        public int OverdueCount { get; set; }
        public int PaidOffCount { get; set; }
        public decimal ActiveAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PaidOffAmount { get; set; }
    }
}
