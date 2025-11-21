using System;
using System.ComponentModel.DataAnnotations;

namespace Loan.Management.Services.credits.Dtos
{
    public class CreditCalculationInput
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(1, 600, ErrorMessage = "Term months must be between 1 and 600")]
        public int TermMonths { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}