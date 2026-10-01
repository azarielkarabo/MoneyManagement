using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Accounts.Dtos
{
    public class UpdateFinancialAccountDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; }
    }
}
