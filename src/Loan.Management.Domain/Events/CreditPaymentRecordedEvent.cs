
using Loan.Management.Entities;
using System;
using Volo.Abp.Domain.Entities.Events;

namespace Loan.Management.Events
{
    public class CreditPaymentRecordedEvent 
    {
        public Credit Credit { get; set; }
        public Guid CreditId { get; set; }
        public Guid BorrowerId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }

        public CreditPaymentRecordedEvent(Credit credit, decimal paymentAmount)
        {
            Credit = credit;
            CreditId = credit.Id;
            BorrowerId = credit.BorrowerId;
            PaymentAmount = paymentAmount;
            PaymentDate = DateTime.UtcNow;
        }
    }
}