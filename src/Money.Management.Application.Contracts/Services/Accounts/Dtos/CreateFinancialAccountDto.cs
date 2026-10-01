using Money.Management.Enums;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Accounts.Dtos
{
    public class CreateFinancialAccountDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public FinancialAccountType AccountType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Opening balance cannot be negative")]
        public decimal OpeningBalance { get; set; }
    }
}
