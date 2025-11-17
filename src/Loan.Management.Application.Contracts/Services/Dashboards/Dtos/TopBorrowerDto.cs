using System;

namespace Loan.Management.Services.Dashboards.Dtos
{
    public class TopBorrowerDto
    {
        public Guid BorrowerId { get; set; }
        public string Name { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public int ActiveLoans { get; set; }
        public decimal PaymentRate { get; set; } // Percentage
    }
}
