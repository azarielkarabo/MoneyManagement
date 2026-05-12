using Money.Management.Enums;
using System;
using System.Collections.Generic;

namespace Money.Management.Services.Budgets.Dtos
{
    public class BudgetVarianceDto
    {
        public Guid BudgetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Salary { get; set; }

        public decimal TotalPlannedExpenses { get; set; }
        public decimal TotalActualExpenses { get; set; }
        public decimal Variance { get; set; }

        public decimal PlannedRemainingBalance { get; set; }
        public decimal ActualRemainingBalance { get; set; }

        public bool IsOverspending { get; set; }
        public decimal OverspendingAmount { get; set; }

        public List<BudgetItemVarianceDto> Items { get; set; } = new();
    }

    public class BudgetItemVarianceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public BudgetItemType Type { get; set; }
        public decimal PlannedAmount { get; set; }
        public decimal? ActualAmount { get; set; }
        public decimal? Variance { get; set; }
        public bool IsOverBudget { get; set; }
    }
}
