using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// One top-up on a <see cref="Credit"/>: extra money lent on an existing loan, with the
    /// interest charged on it. Kept so the loan's history shows how the balance was built up;
    /// the loan's own totals already include every top-up.
    /// </summary>
    public class CreditTopUp : CreationAuditedEntity<Guid>
    {
        public Guid CreditId { get; private set; }

        /// <summary>Extra amount lent.</summary>
        public decimal Amount { get; private set; }

        /// <summary>Rate (%) charged on this top-up only.</summary>
        public decimal InterestRate { get; private set; }

        /// <summary>Interest added to the loan for this top-up.</summary>
        public decimal InterestAmount { get; private set; }

        public DateTime TopUpDate { get; private set; }

        /// <summary>Balance owed on the loan straight after this top-up (null for back-filled entries).</summary>
        public decimal? OutstandingAfter { get; private set; }

        public string? Note { get; private set; }

        protected CreditTopUp() { }

        public CreditTopUp(
            Guid id,
            Guid creditId,
            decimal amount,
            decimal interestRate,
            decimal interestAmount,
            DateTime topUpDate,
            decimal? outstandingAfter,
            string? note)
            : base(id)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Top-up amount must be greater than 0.");

            CreditId = creditId;
            Amount = amount;
            InterestRate = interestRate;
            InterestAmount = interestAmount;
            TopUpDate = topUpDate;
            OutstandingAfter = outstandingAfter;
            Note = note;
        }
    }
}
