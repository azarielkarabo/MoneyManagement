using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// A bank account belonging to a <see cref="Borrower"/>.
    /// Created exclusively through <see cref="Borrower.AddBankAccount"/>.
    /// </summary>
    public class BorrowerBankAccount : FullAuditedEntity<Guid>
    {
        public Guid BorrowerId { get; private set; }
        public Borrower Borrower { get; private set; }

        public string BankName { get; private set; }
        public string? BranchCode { get; private set; }
        public string AccountNumber { get; private set; }
        public BankAccountType AccountType { get; private set; }
        public string? AccountHolderName { get; private set; }

        public bool IsPrimary { get; private set; }
        public bool IsVerified { get; private set; }
        public DateTime? VerifiedAt { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected BorrowerBankAccount() { }

        // ── Created by Borrower.AddBankAccount ────────────────────────────────────
        internal BorrowerBankAccount(
            Guid id,
            Guid borrowerId,
            string bankName,
            string branchCode,
            string accountNumber,
            BankAccountType accountType,
            string accountHolderName,
            bool isPrimary = false
        ) : base(id)
        {
            if (borrowerId == Guid.Empty)
                throw new ArgumentException("BorrowerId cannot be empty.", nameof(borrowerId));
            if (string.IsNullOrWhiteSpace(bankName))
                throw new ArgumentException("Bank name cannot be empty.", nameof(bankName));
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty.", nameof(accountNumber));

            BorrowerId = borrowerId;
            BankName = bankName;
            BranchCode = branchCode;
            AccountNumber = accountNumber;
            AccountType = accountType;
            AccountHolderName = accountHolderName;
            IsPrimary = isPrimary;
            IsVerified = false;
        }

        public void SetAsPrimary() => IsPrimary = true;

        internal void UnsetPrimary() => IsPrimary = false;

        public void Verify()
        {
            IsVerified = true;
            VerifiedAt = DateTime.UtcNow;
        }

        public void Unverify()
        {
            IsVerified = false;
            VerifiedAt = null;
        }
    }
}
