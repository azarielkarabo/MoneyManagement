using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// A single line in a <see cref="Budget"/>.
    /// Tracks both the planned amount and, optionally, the actual amount spent.
    /// Created exclusively through <see cref="Budget.AddItem"/>.
    /// </summary>
    public class BudgetItem : FullAuditedEntity<Guid>
    {
        public Guid BudgetId { get; private set; }
        public Budget Budget { get; private set; }

        public string Name { get; private set; }

        /// <summary>Planned (budgeted) amount.</summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Running total actually spent against this item, accumulated one
        /// <see cref="ExpenseEntry"/> at a time via <see cref="Budget.AddExpenseEntry"/>.
        /// Null means nothing has been spent yet.
        /// </summary>
        public decimal? ActualAmount { get; private set; }

        public BudgetItemType Type { get; private set; }

        /// <summary>
        /// Positive = under budget; negative = over budget.
        /// Null when no actual spending has been recorded.
        /// </summary>
        public decimal? Variance => ActualAmount.HasValue
            ? Amount - ActualAmount.Value
            : null;

        /// <summary>True when actual spending exceeds the planned amount.</summary>
        public bool IsOverBudget => ActualAmount.HasValue && ActualAmount.Value > Amount;

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected BudgetItem() { }

        // ── Created by Budget.AddItem ────────────────────────────────────────────
        internal BudgetItem(
            Guid id,
            Guid budgetId,
            string name,
            decimal amount,
            BudgetItemType type
        ) : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Budget item name cannot be empty.", nameof(name));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

            BudgetId = budgetId;
            Name = name;
            Amount = amount;
            Type = type;
        }

        /// <summary>Adds to the running actual-spend total for this item.</summary>
        internal void AddActualSpending(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount), "Amount must be greater than 0.");

            ActualAmount = (ActualAmount ?? 0m) + amount;
        }

        /// <summary>
        /// Adjusts the planned amount. Only allowed before any actual spending
        /// is recorded (to prevent retroactive budget manipulation).
        /// </summary>
        public void UpdatePlannedAmount(decimal newAmount)
        {
            if (ActualAmount.HasValue)
                throw new InvalidOperationException(
                    "Cannot change the planned amount once actual spending has been recorded.");

            if (newAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(newAmount));

            Amount = newAmount;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            Name = name;
        }
    }
}
