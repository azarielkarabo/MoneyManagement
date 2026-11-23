using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    // An account holder asks a lender for a loan. When the lender approves it, the lender sets
    // the interest rate and expected payback date and a Credit (loan) is created.
    public class LoanRequest : FullAuditedAggregateRoot<Guid>
    {
        // Applicant (the signed-in account holder)
        public Guid UserId { get; private set; }
        public string ApplicantName { get; private set; } = string.Empty;
        public string? ApplicantEmail { get; private set; }
        public string? PhoneNumber { get; private set; }

        // Borrower details, used to create the borrower record if the lender approves
        public string? IdNumber { get; private set; }
        public string Address { get; private set; } = string.Empty;
        public decimal MonthlyIncome { get; private set; }

        public Guid LenderId { get; private set; }
        public decimal Amount { get; private set; }
        public string Reason { get; private set; } = string.Empty;

        public LoanRequestStatus Status { get; private set; }

        // Set when reviewed
        public decimal? InterestRate { get; private set; }
        public DateTime? ExpectedPaybackDate { get; private set; }
        public string? ReviewNote { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public Guid? CreatedCreditId { get; private set; }

        protected LoanRequest() { }

        public LoanRequest(
            Guid id,
            Guid userId,
            string applicantName,
            string? applicantEmail,
            string? phoneNumber,
            string? idNumber,
            string address,
            decimal monthlyIncome,
            Guid lenderId,
            decimal amount,
            string reason) : base(id)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Requested amount must be greater than 0.");

            UserId = userId;
            ApplicantName = applicantName;
            ApplicantEmail = applicantEmail;
            PhoneNumber = phoneNumber;
            IdNumber = idNumber;
            Address = address;
            MonthlyIncome = monthlyIncome;
            LenderId = lenderId;
            Amount = amount;
            Reason = reason;
            Status = LoanRequestStatus.Pending;
        }

        public void Approve(Guid creditId, decimal interestRate, DateTime expectedPaybackDate, string? note, DateTime now)
        {
            EnsurePending();
            Status = LoanRequestStatus.Approved;
            CreatedCreditId = creditId;
            InterestRate = interestRate;
            ExpectedPaybackDate = expectedPaybackDate;
            ReviewNote = note;
            ReviewedAt = now;
        }

        public void Reject(string? reason, DateTime now)
        {
            EnsurePending();
            Status = LoanRequestStatus.Rejected;
            ReviewNote = reason;
            ReviewedAt = now;
        }

        public void Cancel()
        {
            EnsurePending();
            Status = LoanRequestStatus.Cancelled;
        }

        private void EnsurePending()
        {
            if (Status != LoanRequestStatus.Pending)
                throw new InvalidOperationException("This loan request has already been dealt with.");
        }
    }
}
