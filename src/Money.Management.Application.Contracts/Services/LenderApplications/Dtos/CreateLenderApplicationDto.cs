using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class CreateLenderApplicationDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(256)]
        public string DesiredUserName { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }
    }
}
