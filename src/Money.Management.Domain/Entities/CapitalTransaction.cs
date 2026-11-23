using System;
using Money.Management.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// Money the lender puts into or takes out of their lending business.
    /// The first <see cref="CapitalTransactionType.OpeningBalance"/> is the balance the business started with;
    /// deposits and withdrawals after that adjust it. Lending and repayments are not recorded here:
    /// they come from <see cref="Credit"/> and <see cref="Payment"/>.
    /// </summary>
    public class CapitalTransaction : FullAuditedEntity<Guid>
    {
        public Guid LenderId { get; private set; }
        public Lender Lender { get; private set; }

        public CapitalTransactionType Type { get; private set; }

        /// <summary>Always positive; <see cref="Type"/> says which way the money moved.</summary>
        public decimal Amount { get; private set; }

        public DateTime TransactionDate { get; private set; }

        public string? Notes { get; private set; }

        protected CapitalTransaction() { }

        public CapitalTransaction(Guid id, Guid lenderId, CapitalTransactionType type, decimal amount, DateTime transactionDate, string? notes)
            : base(id)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

            LenderId = lenderId;
            Type = type;
            Amount = amount;
            TransactionDate = transactionDate.Date;
            Notes = notes;
        }

        /// <summary>Signed effect on the business's cash: + for money in, − for money out.</summary>
        public decimal SignedAmount => Type == CapitalTransactionType.Withdrawal ? -Amount : Amount;
    }
}
