using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Budgets.Dtos
{
    public class ExpenseEntryDto : FullAuditedEntityDto<Guid>
    {
        public Guid BudgetItemId { get; set; }
        public string BudgetItemName { get; set; }
        public Guid? AccountId { get; set; }
        public string? AccountName { get; set; }
        public decimal Amount { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Notes { get; set; }
    }
}
