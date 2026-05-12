using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Loans.Dtos
{
    public class LoanCalculationInput
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
    }

    public class LoanCalculationDto
    {
        public decimal PrincipalAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TermMonths { get; set; }

        public decimal SimpleInterest { get; set; }
        public decimal TotalAmountSimple { get; set; }

        public decimal CompoundInterest { get; set; }
        public decimal TotalAmountCompound { get; set; }

        public decimal MonthlyInstalment { get; set; }
    }
}
