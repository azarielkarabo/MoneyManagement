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

        /// <summary>
        /// The IdentityUser that logs in as this borrower, if one has been created (via the
        /// self-service BorrowerApplication approval flow). Null for borrowers created directly
        /// by a lender with no linked login account.
        /// </summary>
        public Guid? UserId { get; private set; }

        // ── Personal Information ──────────────────────────────────────────────────
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }

        // ── Identification ────────────────────────────────────────────────────────
        public string IdNumber { get; private set; }
        public IdentificationType IdType { get; private set; }

        // ── Contact Information ───────────────────────────────────────────────────
        public string EmailAddress { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Address { get; private set; }

        // ── Income ────────────────────────────────────────────────────────────────
        public decimal MonthlyIncome { get; private set; }

        // ── Status ────────────────────────────────────────────────────────────────
        public BorrowerStatus Status { get; private set; }

        // ── Financial Summary ─────────────────────────────────────────────────────
        public decimal TotalBorrowed { get; private set; }
        public decimal Outstanding { get; private set; }
        public decimal Paid { get; private set; }
        public decimal DefaultAmount { get; private set; }
        public int TotalMoneys { get; private set; }
        public int DefaultedMoneys { get; private set; }

        public string? Notes { get; private set; }

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
            string address,
            decimal monthlyIncome
        ) : base(id)
        {
            if (lenderId == Guid.Empty)
                throw new ArgumentException("LenderId cannot be empty.", nameof(lenderId));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
            if (monthlyIncome < 0)
                throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income cannot be negative.");

            LenderId = lenderId;
            FirstName = firstName;
            LastName = lastName;
            FullName = $"{firstName} {lastName}";
            IdNumber = idNumber ?? string.Empty;
            IdType = idType;
            EmailAddress = email;
            PhoneNumber = phoneNumber;
            Address = address;
            MonthlyIncome = monthlyIncome;

            Status = BorrowerStatus.New;
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

        public void UpdatePersonalInfo(string firstName, string lastName, string idNumber, IdentificationType idType)
        {
            if (!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName;
            if (!string.IsNullOrWhiteSpace(lastName)) LastName = lastName;
            FullName = $"{FirstName} {LastName}";
            if (!string.IsNullOrWhiteSpace(idNumber)) IdNumber = idNumber;
            IdType = idType;
        }

        public void UpdateContactInfo(string email, string phone)
        {
            if (!string.IsNullOrWhiteSpace(email)) EmailAddress = email;
            if (!string.IsNullOrWhiteSpace(phone)) PhoneNumber = phone;
        }

        public void UpdateAddress(string address)
        {
            Address = address;
        }

        public void UpdateMonthlyIncome(decimal monthlyIncome)
        {
            if (monthlyIncome < 0)
                throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income cannot be negative.");
            MonthlyIncome = monthlyIncome;
        }

        public void UpdateNotes(string? notes) => Notes = notes;

        public void AssignUser(Guid userId)
        {
            if (UserId.HasValue)
                throw new InvalidOperationException("This borrower is already linked to a login account.");
            UserId = userId;
        }

        public void Suspend(string reason)
        {
            if (Status == BorrowerStatus.Suspended)
                throw new InvalidOperationException("Borrower is already suspended.");
            Status = BorrowerStatus.Suspended;
            Notes = reason;
        }

        public void Blacklist(string reason)
        {
            if (Status == BorrowerStatus.Blacklisted)
                throw new InvalidOperationException("Borrower is already blacklisted.");
            Status = BorrowerStatus.Blacklisted;
            Notes = reason;
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
