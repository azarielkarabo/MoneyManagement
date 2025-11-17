using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Payments.Dtos
{
    public class PaymentsByBorrowerDto
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public decimal TotalPaid { get; set; }
        public int PaymentCount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public List<PaymentDto> Payments { get; set; }

        public PaymentsByBorrowerDto()
        {
            Payments = new List<PaymentDto>();
        }
    }
}
