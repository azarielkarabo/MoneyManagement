using Money.Management.Enums;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class CreateBorrowerDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [StringLength(13, ErrorMessage = "ID Number cannot exceed 13 characters")]
        public string IdNumber { get; set; }

        [Required]
        public IdentificationType IdType { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Phone number must contain at least 10 digits")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly income must be a positive number")]
        public decimal MonthlyIncome { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }
    }
}
