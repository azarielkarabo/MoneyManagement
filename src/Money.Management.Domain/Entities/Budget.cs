using Money.Management.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// Monthly budget aggregate root. Tracks both planned allocations and actual
    /// spending per <see cref="BudgetItem"/>. All mutations go through methods.
    /// </summary>
    public class Budget : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        /// <summary>Total income for this month.</summary>
        public decimal Salary { get; private set; }

        public int Month { get; private set; }
        public int Year { get; private set; }

        private readonly List<BudgetItem> _items = new();
        public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected Budget() { }

        // ── Domain constructor ────────────────────────────────────────────────────
        public Budget(Guid id, Guid userId, decimal salary, int month, int year) : base(id)
        {
            if (salary <= 0)
                throw new ArgumentOutOfRangeException(nameof(salary), "Salary must be greater than 0.");

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

            if (year < 2000)
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be 2000 or later.");

            UserId = userId;
            Salary = salary;
            Month = month;
            Year = year;
        }

        // ── Income ────────────────────────────────────────────────────────────────

        /// <summary>Updates the income (salary) for this budget period.</summary>
        public void UpdateSalary(decimal newSalary)
        {
            if (newSalary <= 0)
                throw new ArgumentOutOfRangeException(nameof(newSalary), "Salary must be greater than 0.");

            Salary = newSalary;
        }

        // ── Item management ───────────────────────────────────────────────────────

        /// <summary>
        /// Adds a planned budget item. Throws if the total planned amount would exceed
        /// salary (so the caller must fix the budget before adding more items).
        /// </summary>
        public BudgetItem AddItem(Guid itemId, string name, decimal plannedAmount, BudgetItemType type)
        {
            if (plannedAmount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(plannedAmount), "Planned amount must be greater than 0.");

            var newPlannedTotal = GetTotalPlanned() + plannedAmount;
            if (newPlannedTotal > Salary)
                throw new InvalidOperationException(
                    $"Adding this item (planned {plannedAmount:C}) would exceed the salary " +
                    $"({Salary:C}). Current planned total: {GetTotalPlanned():C}.");

            var item = new BudgetItem(itemId, Id, name, plannedAmount, type);
            _items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a dated spend to an existing budget item's running actual total.
        /// Actual amounts ARE allowed to exceed the planned amount —
        /// use <see cref="IsOverspending"/> to detect this.
        /// </summary>
        public void AddExpenseEntry(Guid itemId, decimal amount)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new InvalidOperationException(
                    $"Budget item {itemId} not found in budget {Id}.");

            item.AddActualSpending(amount);
        }

        // ── Planned calculations ──────────────────────────────────────────────────

        public decimal GetTotalPlanned() => _items.Sum(i => i.Amount);

        public decimal GetTotalPlannedExpenses() =>
            _items.Where(i => i.Type == BudgetItemType.Expense).Sum(i => i.Amount);

        public decimal GetTotalPlannedSavings() =>
            _items.Where(i => i.Type == BudgetItemType.Saving).Sum(i => i.Amount);

        public decimal GetTotalPlannedInvestments() =>
            _items.Where(i => i.Type == BudgetItemType.Investment).Sum(i => i.Amount);

        /// <summary>Salary minus total planned expenses (not savings/investments).</summary>
        public decimal GetPlannedRemainingBalance() => Salary - GetTotalPlannedExpenses();

        // ── Actual calculations ───────────────────────────────────────────────────

        public decimal GetTotalActual() => _items.Sum(i => i.ActualAmount ?? 0m);

        public decimal GetTotalActualExpenses() =>
            _items.Where(i => i.Type == BudgetItemType.Expense)
                  .Sum(i => i.ActualAmount ?? 0m);

        public decimal GetTotalActualSavings() =>
            _items.Where(i => i.Type == BudgetItemType.Saving)
                  .Sum(i => i.ActualAmount ?? 0m);

        /// <summary>Salary minus total actual expenses.</summary>
        public decimal GetActualRemainingBalance() => Salary - GetTotalActualExpenses();

        // ── Variance ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Positive value means under-budget; negative means over-budget.
        /// </summary>
        public decimal GetVariance() => GetTotalPlannedExpenses() - GetTotalActualExpenses();

        /// <summary>True when actual expenses exceed the budgeted salary.</summary>
        public bool IsOverspending() => GetTotalActualExpenses() > Salary;

        /// <summary>Amount by which actual expenses exceed salary (0 when within budget).</summary>
        public decimal GetOverspendingAmount() =>
            Math.Max(0m, GetTotalActualExpenses() - Salary);

        // ── Convenience ───────────────────────────────────────────────────────────

        // Kept for backward-compat with earlier code that used these names
        public decimal GetTotalExpenses() => GetTotalPlannedExpenses();
        public decimal GetTotalSavings() => GetTotalPlannedSavings();
        public decimal GetTotalInvestments() => GetTotalPlannedInvestments();
        public decimal GetRemainingBalance() => GetPlannedRemainingBalance();
        public decimal GetTotalAllocated() => GetTotalPlanned();
        public bool ExceedsSalary() => GetTotalPlanned() > Salary;
    }
}
