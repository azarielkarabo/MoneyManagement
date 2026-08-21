using System.ComponentModel.DataAnnotations;
using Money.Management.Enums;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class CreateBorrowerApplicationDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(20)]
        public string IdNumber { get; set; }

        public IdentificationType IdType { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }
    }
}
