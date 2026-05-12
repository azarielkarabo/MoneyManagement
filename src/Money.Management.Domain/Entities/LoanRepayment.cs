using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// An immutable record of a single repayment made against a <see cref="Loan"/>.
    /// Created exclusively through <see cref="Loan.MakeRepayment"/>.
    /// </summary>
    public class LoanRepayment : FullAuditedEntity<Guid>
    {
        public Guid LoanId { get; private set; }
        public Loan Loan { get; private set; }

        public decimal AmountPaid { get; private set; }
        public DateTime PaymentDate { get; private set; }
        public string Notes { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected LoanRepayment() { }

        // ── Created by Loan.MakeRepayment ─────────────────────────────────────────
        internal LoanRepayment(
            Guid id,
            Guid loanId,
            decimal amountPaid,
            DateTime paymentDate,
            string notes = null
        ) : base(id)
        {
            if (loanId == Guid.Empty)
                throw new ArgumentException("LoanId cannot be empty.", nameof(loanId));

            if (amountPaid <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amountPaid), "Repayment amount must be greater than zero.");

            LoanId = loanId;
            AmountPaid = amountPaid;
            PaymentDate = paymentDate;
            Notes = notes;
        }

        /// <summary>
        /// Allows a correction note to be appended after the fact.
        /// No financial fields can change after recording.
        /// </summary>
        public void UpdateNotes(string notes)
        {
            Notes = notes;
        }
    }
}
