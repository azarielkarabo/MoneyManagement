using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Loans.Dtos
{
    public class AmortizationScheduleInput
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal AnnualInterestRate { get; set; }

        [Required]
        [Range(1, 600)]
        public int TermMonths { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
    }

    public class AmortizationScheduleDto
    {
        public decimal Principal { get; set; }
        public decimal AnnualRatePct { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyInstalment { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalInterest { get; set; }

        public List<AmortizationEntryDto> Entries { get; set; } = new();
    }

    public class AmortizationEntryDto
    {
        public int Month { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Emi { get; set; }
        public decimal InterestPortion { get; set; }
        public decimal PrincipalPortion { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
