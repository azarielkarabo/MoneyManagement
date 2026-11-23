using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Registration
{
    // Self sign-up. Everyone starts as an account holder; lenders apply from inside the app.
    public class RegisterAccountHolderDto
    {
        [Required, StringLength(64)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(64)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(256, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(16)]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(128, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
