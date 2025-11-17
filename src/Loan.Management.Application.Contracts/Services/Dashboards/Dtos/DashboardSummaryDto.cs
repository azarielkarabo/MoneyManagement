namespace Loan.Management.Services.Dashboards.Dtos
{
    public class DashboardSummaryDto
    {
        public decimal TotalLent { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalCollected { get; set; }
        public int ActiveLoansCount { get; set; }
        public int OverdueLoansCount { get; set; }
        public int TotalBorrowersCount { get; set; }
        public int NewBorrowersThisMonth { get; set; }
        public decimal CollectionRate { get; set; } // Percentage
    }
}
