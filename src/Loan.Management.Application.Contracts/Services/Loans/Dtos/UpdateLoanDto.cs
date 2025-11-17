using Loan.Management.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Loan.Management.Services.Loans.Dtos
{
    public class UpdateLoanDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime DisbursementDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public LoanStatus Status { get; set; }
    }
}
