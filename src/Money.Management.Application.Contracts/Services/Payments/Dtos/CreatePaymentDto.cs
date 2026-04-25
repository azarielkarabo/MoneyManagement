using Money.Management.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Payments.Dtos
{
    public class CreatePaymentDto
    {
        [Required]
        public Guid CreditId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
