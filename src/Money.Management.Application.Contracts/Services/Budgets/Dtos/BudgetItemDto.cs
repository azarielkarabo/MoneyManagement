using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Budgets.Dtos
{
    public class BudgetItemDto : FullAuditedEntityDto<Guid>
    {
        public Guid BudgetId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public BudgetItemType Type { get; set; }
    }
}
