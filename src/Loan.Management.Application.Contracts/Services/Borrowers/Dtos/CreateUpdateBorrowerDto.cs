using System.ComponentModel.DataAnnotations;
using Volo.Abp.Identity;

namespace Loan.Management.Services.Borrowers.Dtos
{
    public class CreateBorrowerDto
    {
        public IdentityUserCreateDto UserCreate { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be 13 digits")]
        public string IdNumber { get; set; }
    }

    public class UpdateBorrowerDto
    {
        public IdentityUserUpdateDto UserUpdate { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be 13 digits")]
        public string IdNumber { get; set; }
    }
}
