using System;

namespace Money.Management.Services.Reports.Dtos
{
    public class MoneySummaryDto
    {
        public Guid CreditId { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Outstanding { get; set; }
        public string Status { get; set; }
    }
}
