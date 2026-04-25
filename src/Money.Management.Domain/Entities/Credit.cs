// Money.Management/Entities/Credit.cs
using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Credit : FullAuditedAggregateRoot<Guid>
    {
        public Guid BorrowerId { get; set; }
        public Borrower Borrower { get; set; }

        public Guid LenderId { get; set; }
        public Lender Lender { get; set; }

        // Money Details
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }

        // Calculated Amounts
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment Tracking
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Status
        public CreditStatus Status { get; set; }

        // Optional Fields
        public string Purpose { get; set; }
        public string Notes { get; set; }
        public DateTime? LastPaymentDate { get; set; }

        protected Credit()
        {
        }

        public Credit(
            Guid id,
            Guid borrowerId,
            Guid lenderId,
            decimal principalAmount,
            decimal interestRate,
            int termMonths,
            DateTime dueDate,
            string purpose = null
        ) : base(id)
        {
            BorrowerId = borrowerId;
            LenderId = lenderId;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            TermMonths = termMonths;
            IssuedDate = DateTime.UtcNow;
            DueDate = dueDate;
            Purpose = purpose;
            Status = CreditStatus.Pending;
            AmountPaid = 0;

            CalculateMoneyAmounts();
        }

        public void CalculateMoneyAmounts()
        {
            InterestAmount = PrincipalAmount * (InterestRate / 100);
            TotalAmount = PrincipalAmount + InterestAmount;
            OutstandingAmount = TotalAmount - AmountPaid;
        }

        public void RecordPayment(decimal paymentAmount)
        {
            AmountPaid += paymentAmount;
            OutstandingAmount = TotalAmount - AmountPaid;
            LastPaymentDate = DateTime.UtcNow;

            // Update status
            if (OutstandingAmount <= 0)
            {
                Status = CreditStatus.PaidOff;
                OutstandingAmount = 0;
            }
            else if (DateTime.UtcNow > DueDate)
            {
                Status = CreditStatus.Overdue;
            }
            else
            {
                Status = CreditStatus.Active;
            }
        }

        public void CheckOverdueStatus()
        {
            if (OutstandingAmount > 0 && DateTime.UtcNow.Date > DueDate.Date)
            {
                Status = CreditStatus.Overdue;
            }
        }

        public void ApproveCredit()
        {
            Status = CreditStatus.Approved;
        }

        public void ActivateCredit()
        {
            Status = CreditStatus.Active;
        }
    }
}