using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.LenderApplications.Dtos
{
    // In-app application by a signed-in account holder to become a lender.
    // Name, email and username come from their account.
    public class ApplyAsLenderDto
    {
        [Required, StringLength(128)]
        public string Position { get; set; } = string.Empty;

        [StringLength(16)]
        public string? PhoneNumber { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }
    }
}
