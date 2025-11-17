namespace Loan.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Loan aging analysis
    /// </summary>
    public class LoanAgingDto
    {
        public int Current { get; set; } // 0-30 days
        public int Days30_60 { get; set; }
        public int Days60_90 { get; set; }
        public int Days90Plus { get; set; }

        public decimal CurrentAmount { get; set; }
        public decimal Days30_60Amount { get; set; }
        public decimal Days60_90Amount { get; set; }
        public decimal Days90PlusAmount { get; set; }
    }
}