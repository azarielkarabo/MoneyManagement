using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Budgets.Dtos
{
    public class CreateBudgetDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0")]
        public decimal Salary { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }

        [Required]
        [Range(2000, 9999, ErrorMessage = "Year must be a valid year")]
        public int Year { get; set; }
    }
}
