using System;

namespace Loan.Management.Services.Reports.Dtos
{
    public class PaymentSummaryDto
    {
        public Guid PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public Guid CreditId { get; set; }
    }
}
