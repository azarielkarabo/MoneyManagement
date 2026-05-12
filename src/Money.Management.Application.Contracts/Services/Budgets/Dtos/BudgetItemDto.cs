using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Budgets.Dtos
{
    public class BudgetItemDto : FullAuditedEntityDto<Guid>
    {
        public Guid BudgetId { get; set; }
        public string Name { get; set; }

        /// <summary>Planned (budgeted) amount.</summary>
        public decimal Amount { get; set; }

        /// <summary>Actual amount spent. Null when not yet recorded.</summary>
        public decimal? ActualAmount { get; set; }

        /// <summary>Planned minus actual. Null when actual is not yet recorded.</summary>
        public decimal? Variance { get; set; }

        public bool IsOverBudget { get; set; }
        public BudgetItemType Type { get; set; }
    }
}
