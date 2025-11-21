using Loan.Management.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Loan.Management.Services.Borrowers.Dtos
{
    public class UpdateBorrowerDto
    {
        // Personal Information
        [Required]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        // Identification
        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "ID Number must be 13 digits")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be 13 digits")]
        public string IdNumber { get; set; }

        [Required]
        public IdentificationType IdType { get; set; }

        public DateTime IdExpiryDate { get; set; }

        // Contact Information
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Phone number must contain at least 10 digits")]
        public string PhoneNumber { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string AlternativePhoneNumber { get; set; }

        // Address Information
        [Required]
        [StringLength(500, ErrorMessage = "Street address cannot exceed 500 characters")]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
        public string Province { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; }

        // Employment & Income
        [Required]
        public EmploymentStatus EmploymentStatus { get; set; }

        [StringLength(200, ErrorMessage = "Employer cannot exceed 200 characters")]
        public string Employer { get; set; }

        [StringLength(200, ErrorMessage = "Job title cannot exceed 200 characters")]
        public string JobTitle { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Monthly income must be a positive number")]
        public decimal MonthlyIncome { get; set; }

        public DateTime EmploymentStartDate { get; set; }

        // Personal Details
        public MaritalStatus MaritalStatus { get; set; }

        [Range(0, 20, ErrorMessage = "Number of dependents must be between 0 and 20")]
        public int? NumberOfDependents { get; set; }

        // Emergency Contact
        [StringLength(200, ErrorMessage = "Emergency contact name cannot exceed 200 characters")]
        public string EmergencyContactName { get; set; }

        [StringLength(20, ErrorMessage = "Emergency contact phone cannot exceed 20 characters")]
        public string EmergencyContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "Relationship cannot exceed 100 characters")]
        public string EmergencyContactRelationship { get; set; }

        // Notes
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }
    }
}