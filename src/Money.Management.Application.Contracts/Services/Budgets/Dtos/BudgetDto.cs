using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Budgets.Dtos
{
    public class BudgetDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public decimal Salary { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalInvestments { get; set; }
        public decimal RemainingBalance { get; set; }

        public List<BudgetItemDto> Items { get; set; } = new();
    }
}
