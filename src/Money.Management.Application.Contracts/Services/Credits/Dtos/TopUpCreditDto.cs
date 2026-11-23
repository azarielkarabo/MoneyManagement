using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.credits.Dtos
{
    // Lends more money on an existing loan. Interest is charged on the extra
    // amount at its own InterestRate, which the lender must enter for every top-up.
    public class TopUpCreditDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Top-up amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Interest rate is required for every top-up")]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal? InterestRate { get; set; }

        // Optional new due date; the current one is kept when omitted.
        public DateTime? DueDate { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }
    }
}
