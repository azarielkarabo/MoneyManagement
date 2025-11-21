using Loan.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Loan.Management.Entities
{
    public class Payment : FullAuditedAggregateRoot<Guid>
    {
        public Guid CreditId { get; set; }
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public decimal PreviousOutstanding { get; set; }
        public decimal NewOutstanding { get; set; }

        public string Description { get; set; }
        public PaymentMethod Method { get; set; }

        // Navigation properties
        public Credit Credit{ get; set; }
        public Borrower Borrower { get; set; }

        protected Payment()
        {
        }

        public Payment(
            Guid id,
            Guid creditId,
            Guid borrowerId,
            string borrowerName,
            decimal amount,
            DateTime paymentDate,
            decimal previousOutstanding,
            PaymentMethod method,
            string description = null
        ) : base(id)
        {
            CreditId= creditId;
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

