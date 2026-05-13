using Money.Management.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Borrower : FullAuditedAggregateRoot<Guid>
    {
        public Guid LenderId { get; private set; }
        public Lender Lender { get; private set; }

        // ── Personal Information ──────────────────────────────────────────────────
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }
        public DateTime DateOfBirth { get; private set; }

        // ── Identification ────────────────────────────────────────────────────────
        public string IdNumber { get; private set; }
        public IdentificationType IdType { get; private set; }
        public DateTime IdExpiryDate { get; private set; }

        // ── Contact Information ───────────────────────────────────────────────────
        public string EmailAddress { get; private set; }
        public string PhoneNumber { get; private set; }
        public string AlternativePhoneNumber { get; private set; }

        // ── Address ───────────────────────────────────────────────────────────────
        public string StreetAddress { get; private set; }
        public string City { get; private set; }
        public string Province { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }

        // ── Employment & Income ───────────────────────────────────────────────────
        public EmploymentStatus EmploymentStatus { get; private set; }
        public string Employer { get; private set; }
        public string JobTitle { get; private set; }
        public decimal MonthlyIncome { get; private set; }
        public DateTime EmploymentStartDate { get; private set; }

        // ── Personal Details ──────────────────────────────────────────────────────
        public MaritalStatus MaritalStatus { get; private set; }
        public int? NumberOfDependents { get; private set; }

        // ── Emergency Contact ─────────────────────────────────────────────────────
        public string EmergencyContactName { get; private set; }
        public string EmergencyContactPhone { get; private set; }
        public string EmergencyContactRelationship { get; private set; }

        // ── Compliance & Verification ─────────────────────────────────────────────
        public BorrowerStatus Status { get; private set; }
        public bool IsKycVerified { get; private set; }
        public DateTime? KycVerificationDate { get; private set; }
        public string? SuspensionReason { get; private set; }
        public string? BlacklistReason { get; private set; }

        // ── Profile ───────────────────────────────────────────────────────────────
        public string? AvatarUrl { get; private set; }
        public int CreditScore { get; private set; }
        public CreditRating CreditRating { get; private set; }

        // ── Financial Summary ─────────────────────────────────────────────────────
        public decimal TotalBorrowed { get; private set; }
        public decimal Outstanding { get; private set; }
        public decimal Paid { get; private set; }
        public decimal DefaultAmount { get; private set; }
        public int TotalMoneys { get; private set; }
        public int DefaultedMoneys { get; private set; }

        // ── Tracking ──────────────────────────────────────────────────────────────
        public DateTime? LastActivityDate { get; private set; }
        public string Notes { get; private set; }

        // ── Navigation properties ─────────────────────────────────────────────────
        public ICollection<Credit> Credits { get; private set; }
        public ICollection<Payment> Payments { get; private set; }
        public ICollection<Loan> Loans { get; private set; }

        private readonly List<BorrowerBankAccount> _bankAccounts = new();
        public IReadOnlyCollection<BorrowerBankAccount> BankAccounts => _bankAccounts.AsReadOnly();

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected Borrower()
        {
            Credits = new List<Credit>();
            Payments = new List<Payment>();
            Loans = new List<Loan>();
        }

        // ── Domain constructor ────────────────────────────────────────────────────
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
            if (lenderId == Guid.Empty)
                throw new ArgumentException("LenderId cannot be empty.", nameof(lenderId));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
            if (string.IsNullOrWhiteSpace(idNumber))
                throw new ArgumentException("ID number cannot be empty.", nameof(idNumber));
            if (monthlyIncome < 0)
                throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income cannot be negative.");

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
            CreditRating = CreditRating.Unrated;
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

        // ── Domain methods ────────────────────────────────────────────────────────

        public void UpdatePersonalInfo(
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            MaritalStatus maritalStatus,
            int? numberOfDependents,
            string idNumber,
            IdentificationType idType,
            DateTime idExpiryDate)
        {
            if (!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName;
            if (!string.IsNullOrWhiteSpace(lastName)) LastName = lastName;
            FullName = $"{FirstName} {LastName}";
            DateOfBirth = dateOfBirth;
            MaritalStatus = maritalStatus;
            NumberOfDependents = numberOfDependents;
            if (!string.IsNullOrWhiteSpace(idNumber)) IdNumber = idNumber;
            IdType = idType;
            IdExpiryDate = idExpiryDate;
        }

        public void UpdateContactInfo(string email, string phone, string alternativePhone = null)
        {
            if (!string.IsNullOrWhiteSpace(email)) EmailAddress = email;
            if (!string.IsNullOrWhiteSpace(phone)) PhoneNumber = phone;
            AlternativePhoneNumber = alternativePhone;
        }

        public void UpdateAddress(
            string streetAddress,
            string city,
            string province,
            string postalCode,
            string country)
        {
            StreetAddress = streetAddress;
            City = city;
            Province = province;
            PostalCode = postalCode;
            Country = country;
        }

        public void UpdateEmploymentInfo(
            EmploymentStatus employmentStatus,
            string employer,
            string jobTitle,
            decimal monthlyIncome,
            DateTime employmentStartDate)
        {
            if (monthlyIncome < 0)
                throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income cannot be negative.");
            EmploymentStatus = employmentStatus;
            Employer = employer;
            JobTitle = jobTitle;
            MonthlyIncome = monthlyIncome;
            EmploymentStartDate = employmentStartDate;
        }

        public void UpdateEmergencyContact(string name, string phone, string relationship)
        {
            EmergencyContactName = name;
            EmergencyContactPhone = phone;
            EmergencyContactRelationship = relationship;
        }

        public void UpdateNotes(string notes) => Notes = notes;

        public void Suspend(string reason)
        {
            if (Status == BorrowerStatus.Suspended)
                throw new InvalidOperationException("Borrower is already suspended.");
            Status = BorrowerStatus.Suspended;
            SuspensionReason = reason;
            Notes = reason;
        }

        public void Blacklist(string reason)
        {
            if (Status == BorrowerStatus.Blacklisted)
                throw new InvalidOperationException("Borrower is already blacklisted.");
            Status = BorrowerStatus.Blacklisted;
            BlacklistReason = reason;
            Notes = reason;
        }

        public void SetAvatar(string avatarUrl) => AvatarUrl = avatarUrl;

        public void UpdateCreditScore(int creditScore)
        {
            if (creditScore < 0 || creditScore > 1000)
                throw new ArgumentOutOfRangeException(nameof(creditScore), "Credit score must be between 0 and 1000.");
            CreditScore = creditScore;
            CreditRating = creditScore switch
            {
                >= 750 => CreditRating.Excellent,
                >= 650 => CreditRating.Good,
                >= 550 => CreditRating.Fair,
                >= 450 => CreditRating.Poor,
                > 0   => CreditRating.VeryPoor,
                _     => CreditRating.Unrated
            };
        }

        public void UpdateKycStatus(bool isVerified)
        {
            IsKycVerified = isVerified;
            KycVerificationDate = DateTime.UtcNow;
        }

        public void UpdateFinancialSummary(
            decimal totalBorrowed,
            decimal outstanding,
            decimal paid,
            decimal defaultAmount,
            int totalMoneys,
            int defaultedMoneys)
        {
            TotalBorrowed = totalBorrowed;
            Outstanding = outstanding;
            Paid = paid;
            DefaultAmount = defaultAmount;
            TotalMoneys = totalMoneys;
            DefaultedMoneys = defaultedMoneys;
            LastActivityDate = DateTime.UtcNow;

            if (Outstanding == 0 && TotalBorrowed > 0)
                Status = BorrowerStatus.PaidOff;
            else if (Outstanding > 0)
                Status = BorrowerStatus.Active;
        }

        // ── Bank account management ───────────────────────────────────────────────

        public BorrowerBankAccount AddBankAccount(
            Guid accountId,
            string bankName,
            string branchCode,
            string accountNumber,
            BankAccountType accountType,
            string accountHolderName,
            bool isPrimary = false)
        {
            if (isPrimary)
                foreach (var a in _bankAccounts)
                    a.UnsetPrimary();

            var account = new BorrowerBankAccount(
                accountId, Id, bankName, branchCode, accountNumber, accountType, accountHolderName, isPrimary);
            _bankAccounts.Add(account);
            return account;
        }

        public void SetPrimaryBankAccount(Guid accountId)
        {
            var target = _bankAccounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new InvalidOperationException($"Bank account {accountId} not found on this borrower.");
            foreach (var a in _bankAccounts)
                a.UnsetPrimary();
            target.SetAsPrimary();
        }
    }
}
