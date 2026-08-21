using System;
using Money.Management.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class BorrowerApplication : FullAuditedAggregateRoot<Guid>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string IdNumber { get; private set; }
        public IdentificationType IdType { get; private set; }
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Address { get; private set; }
        public decimal MonthlyIncome { get; private set; }
        public string? Message { get; private set; }

        public BorrowerApplicationStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }
        public Guid? ReviewedByUserId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public Guid? CreatedBorrowerId { get; private set; }

        protected BorrowerApplication() { }

        public BorrowerApplication(
            Guid id,
            string firstName,
            string lastName,
            string idNumber,
            IdentificationType idType,
            string email,
            string phoneNumber,
            string address,
            decimal monthlyIncome,
            string? message
        ) : base(id)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
            if (string.IsNullOrWhiteSpace(idNumber))
                throw new ArgumentException("ID number cannot be empty.", nameof(idNumber));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (monthlyIncome < 0)
                throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income cannot be negative.");

            FirstName = firstName;
            LastName = lastName;
            IdNumber = idNumber;
            IdType = idType;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            MonthlyIncome = monthlyIncome;
            Message = message;
            Status = BorrowerApplicationStatus.Pending;
        }

        public void Approve(Guid reviewerUserId, Guid createdBorrowerId)
        {
            if (Status != BorrowerApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be approved.");

            Status = BorrowerApplicationStatus.Approved;
            ReviewedByUserId = reviewerUserId;
            ReviewedAt = DateTime.UtcNow;
            CreatedBorrowerId = createdBorrowerId;
        }

        public void Reject(Guid reviewerUserId, string reason)
        {
            if (Status != BorrowerApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A rejection reason is required.", nameof(reason));

            Status = BorrowerApplicationStatus.Rejected;
            ReviewedByUserId = reviewerUserId;
            ReviewedAt = DateTime.UtcNow;
            RejectionReason = reason;
        }
    }
}
