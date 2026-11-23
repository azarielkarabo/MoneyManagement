using Money.Management.Enums;
﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.credits.Dtos
{
    public class CreateCreditDto
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
        public DateTime DisbursementDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public RepaymentType RepaymentType { get; set; } = RepaymentType.LongTerm;


        [StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
        public string? Purpose { get; set; }

        // Optional: informal loans have no fixed repayment term (0 = open-ended).
        [Range(0, 600, ErrorMessage = "Term months must be between 0 and 600")]
        public int TermMonths { get; set; }

    }
}
