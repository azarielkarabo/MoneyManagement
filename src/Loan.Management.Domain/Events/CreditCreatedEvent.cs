
using Loan.Management.Entities;
using System;
using Volo.Abp.Domain.Entities.Events;

namespace Loan.Management.Events
{
    public class CreditCreatedEvent
    {
        public Credit Credit { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid LenderId { get; set; }
        public decimal Amount { get; set; }

        public CreditCreatedEvent(Credit credit)
        {
            Credit = credit;
            BorrowerId = credit.BorrowerId;
            LenderId = credit.LenderId;
            Amount = credit.TotalAmount;
        }
    }
}