using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Budgets.Dtos
{
    public class AddExpenseEntryDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        /// <summary>Optional financial account to withdraw this amount from.</summary>
        public Guid? AccountId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
