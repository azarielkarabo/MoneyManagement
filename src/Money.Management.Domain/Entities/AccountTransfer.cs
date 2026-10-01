using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// An audit record of a completed transfer between two of a user's own
    /// <see cref="FinancialAccount"/>s. Created only by <see cref="Money.Management.DomainServices.FinancialAccountManager"/>
    /// once both balance updates have succeeded.
    /// </summary>
    public class AccountTransfer : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        public Guid FromAccountId { get; private set; }
        public Guid ToAccountId { get; private set; }

        public decimal Amount { get; private set; }

        public string? Notes { get; private set; }

        public DateTime TransferDate { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected AccountTransfer() { }

        public AccountTransfer(
            Guid id,
            Guid userId,
            Guid fromAccountId,
            Guid toAccountId,
            decimal amount,
            string? notes
        ) : base(id)
        {
            if (fromAccountId == toAccountId)
                throw new ArgumentException("Cannot transfer an account into itself.", nameof(toAccountId));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Transfer amount must be greater than 0.");

            UserId = userId;
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Amount = amount;
            Notes = notes;
            TransferDate = DateTime.UtcNow;
        }
    }
}
