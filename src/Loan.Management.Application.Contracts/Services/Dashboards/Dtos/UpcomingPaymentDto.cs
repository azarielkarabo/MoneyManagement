using System;

namespace Loan.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Upcoming payments/due credits
    /// </summary>
    public class UpcomingPaymentDto
    {
        public Guid CreditId { get; set; }
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerPhone { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public string Priority { get; set; } // "High", "Medium", "Low"
    }
}