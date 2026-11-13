using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.PasswordReset.Dtos
{
    public class RequestPasswordResetDto
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}
