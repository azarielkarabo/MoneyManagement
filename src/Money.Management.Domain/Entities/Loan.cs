using Money.Management.CustomExceptions;
using Money.Management.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// Loan aggregate root. All state changes go through public methods — no external writes
    /// to properties. Navigation property <see cref="Repayments"/> is read-only; use
    /// <see cref="MakeRepayment"/> to add entries.
    /// </summary>
    public class Loan : FullAuditedAggregateRoot<Guid>
    {
        // ── Foreign keys ──────────────────────────────────────────────────────────
        public Guid BorrowerId { get; private set; }
        public Borrower Borrower { get; private set; }

        // ── Loan terms (immutable after approval) ────────────────────────────────
        public decimal PrincipalAmount { get; private set; }

        /// <summary>Annual interest rate as a percentage, e.g. 12 = 12 %.</summary>
        public decimal InterestRate { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        // ── Calculated amounts (derived from terms) ───────────────────────────────
        public decimal TotalAmount { get; private set; }
        public decimal OutstandingBalance { get; private set; }

        /// <summary>Total interest = TotalAmount - PrincipalAmount (computed, not stored).</summary>
        public decimal InterestAmount => TotalAmount - PrincipalAmount;

        /// <summary>Total amount repaid to date (computed, not stored).</summary>
        public decimal TotalRepaid => TotalAmount - OutstandingBalance;

        // ── Status / metadata ──────────────────────────────────────────────────────
        public LoanStatus Status { get; private set; }
        public string Notes { get; private set; }

        // ── Repayments collection (private backing field) ─────────────────────────
        // EF Core discovers _repayments by the _propertyName convention and calls .Add()
        // on the existing instance; readonly prevents external reassignment.
        private readonly List<LoanRepayment> _repayments = new();
        public IReadOnlyCollection<LoanRepayment> Repayments => _repayments.AsReadOnly();

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected Loan() { }

        // ── Domain constructor ────────────────────────────────────────────────────
        public Loan(
            Guid id,
            Guid borrowerId,
            decimal principalAmount,
            decimal interestRate,
            DateTime startDate,
            DateTime? endDate = null,
            string notes = null
        ) : base(id)
        {
            if (borrowerId == Guid.Empty)
                throw new ArgumentException("BorrowerId cannot be empty.", nameof(borrowerId));

            if (principalAmount <= 0)
                throw new InvalidPrincipalAmountException(principalAmount);

            if (interestRate < 0 || interestRate > 100)
                throw new InvalidOperationException(
                    $"Interest rate must be between 0 and 100 (got {interestRate}).");

            if (endDate.HasValue && endDate.Value <= startDate)
                throw new InvalidMoneyDatesException();

            BorrowerId = borrowerId;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            StartDate = startDate;
            EndDate = endDate;
            Notes = notes;
            Status = LoanStatus.PendingApproval;

            RecalculateTotalAmount();
        }

        // ── State-transition methods ──────────────────────────────────────────────

        /// <summary>
        /// Transitions the loan from <see cref="LoanStatus.PendingApproval"/> to
        /// <see cref="LoanStatus.Active"/>.
        /// </summary>
        public void Approve()
        {
            if (Status != LoanStatus.PendingApproval)
                throw new CannotApproveLoanException(Id, Status);

            Status = LoanStatus.Active;
        }

        /// <summary>
        /// Records a repayment against this loan. Partial payments are allowed.
        /// If the payment clears the full outstanding balance the loan is automatically
        /// completed. Returns the created <see cref="LoanRepayment"/>.
        /// </summary>
        public LoanRepayment MakeRepayment(
            Guid repaymentId,
            decimal amount,
            DateTime paymentDate,
            string notes = null)
        {
            if (Status == LoanStatus.Completed)
                throw new LoanAlreadyCompletedException(Id);

            if (Status != LoanStatus.Active)
                throw new LoanNotActiveException(Id, Status);

            if (amount <= 0)
                throw new InvalidPaymentAmountException(amount);

            // Cap at outstanding so callers cannot over-pay
            var effectiveAmount = Math.Min(amount, OutstandingBalance);

            var repayment = new LoanRepayment(repaymentId, Id, effectiveAmount, paymentDate, notes);
            _repayments.Add(repayment);

            OutstandingBalance = Math.Max(0m, OutstandingBalance - effectiveAmount);

            if (OutstandingBalance == 0m)
            {
                Status = LoanStatus.Completed;
                EndDate = DateTime.UtcNow;
            }

            return repayment;
        }

        /// <summary>Marks an active loan as defaulted.</summary>
        public void MarkAsDefaulted()
        {
            if (Status != LoanStatus.Active)
                throw new LoanNotActiveException(Id, Status);

            Status = LoanStatus.Defaulted;
        }

        /// <summary>
        /// Updates free-text notes. The only mutable property after creation
        /// because notes carry no business invariants.
        /// </summary>
        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }

        // ── Query helpers ─────────────────────────────────────────────────────────

        /// <summary>Returns the current outstanding balance.</summary>
        public decimal CalculateBalance() => OutstandingBalance;

        /// <summary>Returns repayment progress as a percentage (0–100).</summary>
        public decimal CalculateRepaymentProgress() =>
            TotalAmount > 0 ? (TotalRepaid / TotalAmount) * 100m : 0m;

        /// <summary>
        /// Returns true when the loan is active and past its scheduled end date
        /// without being fully repaid.
        /// </summary>
        public bool IsOverdue(DateTime? asOf = null)
        {
            var date = asOf ?? DateTime.UtcNow;
            return Status == LoanStatus.Active
                && EndDate.HasValue
                && date.Date > EndDate.Value.Date
                && OutstandingBalance > 0;
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void RecalculateTotalAmount()
        {
            TotalAmount = PrincipalAmount + (PrincipalAmount * InterestRate / 100m);
            OutstandingBalance = TotalAmount;
        }
    }
}
