using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// A single dated spend against a <see cref="BudgetItem"/>, optionally withdrawn from one
    /// of the user's own <see cref="FinancialAccount"/>s. Multiple entries can exist per budget
    /// item (e.g. several grocery trips through the month) — <see cref="BudgetItem.ActualAmount"/>
    /// tracks the running total, this table tracks the day-by-day detail behind it.
    /// </summary>
    public class ExpenseEntry : FullAuditedEntity<Guid>
    {
        public Guid BudgetItemId { get; private set; }
        public BudgetItem BudgetItem { get; private set; }

        public Guid UserId { get; private set; }

        public Guid? AccountId { get; private set; }
        public FinancialAccount? Account { get; private set; }

        public decimal Amount { get; private set; }

        public DateTime EntryDate { get; private set; }

        public string? Notes { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected ExpenseEntry() { }

        public ExpenseEntry(
            Guid id,
            Guid budgetItemId,
            Guid userId,
            decimal amount,
            DateTime entryDate,
            Guid? accountId,
            string? notes
        ) : base(id)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

            BudgetItemId = budgetItemId;
            UserId = userId;
            Amount = amount;
            EntryDate = entryDate.Date;
            AccountId = accountId;
            Notes = notes;
        }
    }
}
