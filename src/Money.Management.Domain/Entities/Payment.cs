using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Payment : FullAuditedAggregateRoot<Guid>
    {
        public Guid LenderId { get; set; }
        public Guid CreditId { get; set; }
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public decimal PreviousOutstanding { get; set; }
        public decimal NewOutstanding { get; set; }

        public string? Description { get; set; }
        public PaymentMethod Method { get; set; }

        // Navigation properties
        public Lender Lender { get; set; }
        public Credit Credit { get; set; }
        public Borrower Borrower { get; set; }

        protected Payment()
        {
        }

        public Payment(
            Guid id,
            Guid lenderId,
            Guid creditId,
            Guid borrowerId,
            string borrowerName,
            decimal amount,
            DateTime paymentDate,
            decimal previousOutstanding,
            PaymentMethod method,
            string? description = null
        ) : base(id)
        {
            LenderId = lenderId;
            CreditId = creditId;
            BorrowerId = borrowerId;
            BorrowerName = borrowerName;
            Amount = amount;
            PaymentDate = paymentDate;
            PreviousOutstanding = previousOutstanding;
            NewOutstanding = previousOutstanding - amount;
            Method = method;
            Description = description;
        }
    }
}

