using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class BudgetItem : FullAuditedEntity<Guid>
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; }

        public string Name { get; set; }
        public decimal Amount { get; set; }
        public BudgetItemType Type { get; set; }

        protected BudgetItem()
        {
        }

        public BudgetItem(
            Guid id,
            Guid budgetId,
            string name,
            decimal amount,
            BudgetItemType type
        ) : base(id)
        {
            BudgetId = budgetId;
            Name = name;
            Amount = amount;
            Type = type;
        }
    }
}
