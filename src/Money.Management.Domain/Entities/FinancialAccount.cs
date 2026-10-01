using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// A personal financial account (cash, debit, credit, savings, investment) owned by a user.
    /// Balance changes only through <see cref="Deposit"/>/<see cref="Withdraw"/> so every change
    /// stays consistent with the account's own validation rules.
    /// </summary>
    public class FinancialAccount : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        public string Name { get; private set; }

        public FinancialAccountType AccountType { get; private set; }

        public decimal Balance { get; private set; }

        public bool IsActive { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected FinancialAccount() { }

        // ── Domain constructor ────────────────────────────────────────────────────
        public FinancialAccount(
            Guid id,
            Guid userId,
            string name,
            FinancialAccountType accountType,
            decimal openingBalance = 0m
        ) : base(id)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Account name cannot be empty.", nameof(name));

            if (openingBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(openingBalance), "Opening balance cannot be negative.");

            UserId = userId;
            Name = name;
            AccountType = accountType;
            Balance = openingBalance;
            IsActive = true;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Account name cannot be empty.", nameof(name));

            Name = name;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be greater than 0.");

            if (!IsActive)
                throw new InvalidOperationException("Cannot deposit into an inactive account.");

            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Withdrawal amount must be greater than 0.");

            if (!IsActive)
                throw new InvalidOperationException("Cannot withdraw from an inactive account.");

            if (amount > Balance)
                throw new InvalidOperationException(
                    $"Insufficient balance. Available: {Balance:C}, requested: {amount:C}.");

            Balance -= amount;
        }

        public void Deactivate() => IsActive = false;

        public void Activate() => IsActive = true;
    }
}
