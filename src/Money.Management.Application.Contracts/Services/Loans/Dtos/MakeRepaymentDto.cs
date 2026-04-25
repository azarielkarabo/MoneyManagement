using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Loans.Dtos
{
    public class MakeRepaymentDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than 0")]
        public decimal AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
