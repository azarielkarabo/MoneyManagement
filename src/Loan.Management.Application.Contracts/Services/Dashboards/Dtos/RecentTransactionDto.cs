using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Recent transaction (loan or payment)
    /// </summary>
    public class RecentTransactionDto
    {
        public Guid Id { get; set; }
        public string BorrowerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Loan" or "Payment"
        public string Description { get; set; }
        public string Status { get; set; }
    }
}