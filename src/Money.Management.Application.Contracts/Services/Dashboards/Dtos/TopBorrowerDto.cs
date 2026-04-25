using System;
using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Top borrower information
    /// </summary>
    public class TopBorrowerDto
    {
        public Guid BorrowerId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Paid { get; set; }
        public int ActiveMoneys { get; set; }
        public int TotalMoneys { get; set; }
        public decimal PaymentRate { get; set; }
        public int DaysSinceLastPayment { get; set; }
        public string CreditScore { get; set; } // Good, Fair, Poor
    }
}