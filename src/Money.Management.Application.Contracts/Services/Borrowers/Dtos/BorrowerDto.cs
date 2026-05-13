using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class BorrowerDto : FullAuditedEntityDto<Guid>
    {
        public Guid LenderId { get; set; }

        // Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Identification
        public string IdNumber { get; set; }
        public IdentificationType IdType { get; set; }
        public DateTime IdExpiryDate { get; set; }

        // Contact Information
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }

        // Address Information
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Employment & Income
        public EmploymentStatus EmploymentStatus { get; set; }
        public string Employer { get; set; }
        public string JobTitle { get; set; }
        public decimal MonthlyIncome { get; set; }
        public DateTime EmploymentStartDate { get; set; }

        // Personal Details
        public MaritalStatus MaritalStatus { get; set; }
        public int? NumberOfDependents { get; set; }

        // Emergency Contact
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string EmergencyContactRelationship { get; set; }

        // Compliance & Verification
        public BorrowerStatus Status { get; set; }
        public bool IsKycVerified { get; set; }
        public DateTime? KycVerificationDate { get; set; }
        public string SuspensionReason { get; set; }
        public string BlacklistReason { get; set; }

        // Profile
        public string AvatarUrl { get; set; }
        public int CreditScore { get; set; }
        public CreditRating CreditRating { get; set; }

        // Financial Summary
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Paid { get; set; }
        public decimal DefaultAmount { get; set; }
        public int TotalMoneys { get; set; }
        public int DefaultedMoneys { get; set; }

        // Tracking
        public DateTime? LastActivityDate { get; set; }
        public string Notes { get; set; }

        // Computed (set by service)
        public int ActiveMoneysCount { get; set; }
        public int TotalMoneysCount { get; set; }
    }
}
