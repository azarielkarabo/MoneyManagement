using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Loans.Dtos
{
    public class CreateLoanDto
    {
        [Required]
        public Guid BorrowerId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal amount must be greater than 0")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
