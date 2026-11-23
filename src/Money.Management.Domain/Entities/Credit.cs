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

        // Month-end loans are due at the end of the month they are registered in;
        // long-term loans are due after the term. Existing loans default to long-term.
        public RepaymentType RepaymentType { get; set; } = RepaymentType.LongTerm;

        // Calculated Amounts
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment Tracking
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Status
        public CreditStatus Status { get; set; }

        // Optional Fields
        public string? Purpose { get; set; }
        public string? Notes { get; set; }
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
            string? purpose = null
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
            // A loan is live as soon as it is created; no separate approval step.
            Status = CreditStatus.Active;
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
            else
            {
                // The expected payback date never changes the status; it only rates the borrower.
                Status = CreditStatus.Active;
            }
        }

        // The expected payback date is used only to judge how well a borrower pays back:
        // an unpaid loan past that date is "late". It never changes the loan's status.
        public bool IsLate(DateTime today) => OutstandingAmount > 0
            && Status != CreditStatus.PaidOff
            && Status != CreditStatus.Cancelled
            && Status != CreditStatus.Rejected
            && today.Date > DueDate.Date;

        public int DaysLate(DateTime today) => IsLate(today) ? (today.Date - DueDate.Date).Days : 0;

        // Lends more money on this loan at its own interest rate. The interest on the extra
        // amount is added as-is; InterestRate becomes the overall (blended) rate for display.
        // A settled loan becomes active again.
        public void TopUp(decimal amount, decimal interestRate)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Top-up amount must be greater than 0.");
            }
            if (interestRate < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(interestRate), "Interest rate cannot be negative.");
            }

            PrincipalAmount += amount;
            InterestAmount += Math.Round(amount * (interestRate / 100), 2);
            InterestRate = PrincipalAmount > 0 ? Math.Round(InterestAmount / PrincipalAmount * 100, 2) : interestRate;
            TotalAmount = PrincipalAmount + InterestAmount;
            OutstandingAmount = TotalAmount - AmountPaid;

            if (OutstandingAmount > 0 &&
                (Status == CreditStatus.PaidOff || Status == CreditStatus.Approved || Status == CreditStatus.Overdue))
            {
                Status = CreditStatus.Active;
            }
        }

        // Loans are no longer marked Overdue by date (see IsLate). This moves loans that were
        // marked Overdue in the past back to Active; the background job calls it.
        public void CheckOverdueStatus()
        {
            if (Status == CreditStatus.Overdue)
            {
                Status = OutstandingAmount > 0 ? CreditStatus.Active : CreditStatus.PaidOff;
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