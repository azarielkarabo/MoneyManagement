using Money.Management.Enums;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Budgets.Dtos
{
    public class AddBudgetItemDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public BudgetItemType Type { get; set; }
    }
}
