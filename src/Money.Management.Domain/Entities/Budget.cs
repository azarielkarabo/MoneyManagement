using Money.Management.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Budget : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        public decimal Salary { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public ICollection<BudgetItem> Items { get; set; }

        protected Budget()
        {
            Items = new List<BudgetItem>();
        }

        public Budget(
            Guid id,
            Guid userId,
            decimal salary,
            int month,
            int year
        ) : base(id)
        {
            UserId = userId;
            Salary = salary;
            Month = month;
            Year = year;
            Items = new List<BudgetItem>();
        }

        public decimal GetTotalExpenses()
            => Items.Where(i => i.Type == BudgetItemType.Expense).Sum(i => i.Amount);

        public decimal GetTotalSavings()
            => Items.Where(i => i.Type == BudgetItemType.Saving).Sum(i => i.Amount);

        public decimal GetTotalInvestments()
            => Items.Where(i => i.Type == BudgetItemType.Investment).Sum(i => i.Amount);

        public decimal GetRemainingBalance()
            => Salary - GetTotalExpenses();

        public decimal GetTotalAllocated()
            => Items.Sum(i => i.Amount);

        public bool ExceedsSalary()
            => GetTotalAllocated() > Salary;
    }
}
