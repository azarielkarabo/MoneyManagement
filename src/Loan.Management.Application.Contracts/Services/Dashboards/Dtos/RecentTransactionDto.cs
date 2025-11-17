using System;

namespace Loan.Management.Services.Dashboards.Dtos
{
    public class RecentTransactionDto
    {
        public Guid Id { get; set; }
        public string BorrowerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Loan" or "Payment"
        public string Description { get; set; }
    }
}
