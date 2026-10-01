using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Accounts.Dtos
{
    public class TransferFundsDto
    {
        [Required]
        public Guid FromAccountId { get; set; }

        [Required]
        public Guid ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
