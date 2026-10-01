using System;

namespace Money.Management.Services.Budgets.Dtos
{
    public class DailySpendingDto
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
