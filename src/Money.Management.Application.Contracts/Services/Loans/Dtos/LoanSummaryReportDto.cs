namespace Money.Management.Services.Loans.Dtos
{
    public class LoanSummaryReportDto
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
        public int DefaultedLoans { get; set; }

        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterestExpected { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}
