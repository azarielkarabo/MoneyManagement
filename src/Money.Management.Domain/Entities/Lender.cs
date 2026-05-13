using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Money.Management.Entities
{
    public class Lender : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public IdentityUser User { get; private set; }

        // ── Professional profile ──────────────────────────────────────────────────
        public string Position { get; private set; }
        public string? Department { get; private set; }
        public string? Branch { get; private set; }
        public string? EmployeeId { get; private set; }
        public string? DirectPhone { get; private set; }
        public string? OfficeLocation { get; private set; }
        public string? Bio { get; private set; }
        public string? AvatarUrl { get; private set; }

        // ── Operational settings ──────────────────────────────────────────────────
        public decimal MaxMoneyApprovalAmount { get; private set; }

        // ── Performance metrics ───────────────────────────────────────────────────
        /// <summary>Percentage of outstanding balances collected (0–100).</summary>
        public decimal CollectionRate { get; private set; }

        /// <summary>Percentage of loan applications approved (0–100).</summary>
        public decimal ApprovalRate { get; private set; }

        // ── Statistics ────────────────────────────────────────────────────────────
        public int TotalMoneysCreated { get; private set; }
        public int TotalBorrowersManaged { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected Lender() { }

        // ── Domain constructor ────────────────────────────────────────────────────
        public Lender(
            Guid id,
            Guid userId,
            string position,
            decimal maxMoneyApprovalAmount = 0
        ) : base(id)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(position))
                throw new ArgumentException("Position cannot be empty.", nameof(position));
            if (maxMoneyApprovalAmount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maxMoneyApprovalAmount), "Max approval amount cannot be negative.");

            UserId = userId;
            Position = position;
            MaxMoneyApprovalAmount = maxMoneyApprovalAmount;
            TotalMoneysCreated = 0;
            TotalBorrowersManaged = 0;
        }

        // ── Domain methods ────────────────────────────────────────────────────────

        public void UpdateProfile(
            string position = null,
            string department = null,
            string branch = null,
            string employeeId = null,
            string directPhone = null,
            string officeLocation = null,
            string bio = null)
        {
            if (!string.IsNullOrWhiteSpace(position)) Position = position;
            if (department != null) Department = department;
            if (branch != null) Branch = branch;
            if (employeeId != null) EmployeeId = employeeId;
            if (directPhone != null) DirectPhone = directPhone;
            if (officeLocation != null) OfficeLocation = officeLocation;
            if (bio != null) Bio = bio;
        }

        public void SetAvatar(string avatarUrl) => AvatarUrl = avatarUrl;

        public void UpdateApprovalLimit(decimal maxMoneyApprovalAmount)
        {
            if (maxMoneyApprovalAmount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maxMoneyApprovalAmount), "Max approval amount cannot be negative.");
            MaxMoneyApprovalAmount = maxMoneyApprovalAmount;
        }

        public void UpdatePerformanceMetrics(decimal collectionRate, decimal approvalRate)
        {
            CollectionRate = collectionRate;
            ApprovalRate = approvalRate;
        }

        public void IncrementMoneysCreated() => TotalMoneysCreated++;
        public void IncrementBorrowersManaged() => TotalBorrowersManaged++;
    }
}
