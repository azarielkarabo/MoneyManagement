using Money.Management.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Borrower : FullAuditedAggregateRoot<Guid>
    {
        public Guid LenderId { get; set; }
        public Lender Lender { get; set; }

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

        // Navigation properties
        public ICollection<Credit> Credits { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Loan> Loans { get; set; }

        protected Borrower()
        {
            Credits = new List<Credit>();
            Payments = new List<Payment>();
            Loans = new List<Loan>();
            IsKycVerified = false;
            Status = BorrowerStatus.New;
        }

        public Borrower(
            Guid id,
            Guid lenderId,
            string firstName,
            string lastName,
            string idNumber,
            IdentificationType idType,
            string email,
            string phoneNumber,
            string streetAddress,
            string city,
            string province,
            string postalCode,
            string country,
            DateTime dateOfBirth,
            EmploymentStatus employmentStatus,
            decimal monthlyIncome
        ) : base(id)
        {
            LenderId = lenderId;
            FirstName = firstName;
            LastName = lastName;
            FullName = $"{firstName} {lastName}";
            IdNumber = idNumber;
            IdType = idType;
            EmailAddress = email;
            PhoneNumber = phoneNumber;
            StreetAddress = streetAddress;
            City = city;
            Province = province;
            PostalCode = postalCode;
            Country = country;
            DateOfBirth = dateOfBirth;
            EmploymentStatus = employmentStatus;
            MonthlyIncome = monthlyIncome;

            Status = BorrowerStatus.New;
            IsKycVerified = false;
            TotalBorrowed = 0;
            Outstanding = 0;
            Paid = 0;
            DefaultAmount = 0;
            TotalMoneys = 0;
            DefaultedMoneys = 0;

            Credits = new List<Credit>();
            Payments = new List<Payment>();
            Loans = new List<Loan>();
        }

        public void UpdateFinancialSummary(decimal totalBorrowed, decimal outstanding, decimal paid, decimal defaultAmount, int totalMoneys, int defaultedMoneys)
        {
            TotalBorrowed = totalBorrowed;
            Outstanding = outstanding;
            Paid = paid;
            DefaultAmount = defaultAmount;
            TotalMoneys = totalMoneys;
            DefaultedMoneys = defaultedMoneys;
            LastActivityDate = DateTime.UtcNow;

            if (Outstanding == 0 && TotalBorrowed > 0)
            {
                Status = BorrowerStatus.PaidOff;
            }
            else if (Outstanding > 0)
            {
                Status = BorrowerStatus.Active;
            }
        }

        public void UpdateKycStatus(bool isVerified)
        {
            IsKycVerified = isVerified;
            KycVerificationDate = DateTime.UtcNow;
        }
    }
}