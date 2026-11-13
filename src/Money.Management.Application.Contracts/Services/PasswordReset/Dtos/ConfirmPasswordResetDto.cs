using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.PasswordReset.Dtos
{
    public class ConfirmPasswordResetDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
