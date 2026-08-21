using System;
using Money.Management.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class LenderApplication : FullAuditedAggregateRoot<Guid>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string DesiredUserName { get; private set; }
        public string Position { get; private set; }
        public string? Message { get; private set; }

        public LenderApplicationStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }
        public Guid? ReviewedByUserId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public Guid? CreatedLenderId { get; private set; }

        protected LenderApplication() { }

        public LenderApplication(
            Guid id,
            string firstName,
            string lastName,
            string email,
            string? phoneNumber,
            string desiredUserName,
            string position,
            string? message
        ) : base(id)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(desiredUserName))
                throw new ArgumentException("Desired username cannot be empty.", nameof(desiredUserName));
            if (string.IsNullOrWhiteSpace(position))
                throw new ArgumentException("Position cannot be empty.", nameof(position));

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            DesiredUserName = desiredUserName;
            Position = position;
            Message = message;
            Status = LenderApplicationStatus.Pending;
        }

        public void Approve(Guid reviewerUserId, Guid createdLenderId)
        {
            if (Status != LenderApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be approved.");

            Status = LenderApplicationStatus.Approved;
            ReviewedByUserId = reviewerUserId;
            ReviewedAt = DateTime.UtcNow;
            CreatedLenderId = createdLenderId;
        }

        public void Reject(Guid reviewerUserId, string reason)
        {
            if (Status != LenderApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A rejection reason is required.", nameof(reason));

            Status = LenderApplicationStatus.Rejected;
            ReviewedByUserId = reviewerUserId;
            ReviewedAt = DateTime.UtcNow;
            RejectionReason = reason;
        }
    }
}
