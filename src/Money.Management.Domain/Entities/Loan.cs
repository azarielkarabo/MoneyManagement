using Money.Management.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class Loan : FullAuditedAggregateRoot<Guid>
    {
        public Guid BorrowerId { get; set; }
        public Borrower Borrower { get; set; }

        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public LoanStatus LoanStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }

        public string Notes { get; set; }

        public ICollection<LoanRepayment> Repayments { get; set; }

        protected Loan()
        {
            Repayments = new List<LoanRepayment>();
        }

        public Loan(
            Guid id,
            Guid borrowerId,
            decimal principalAmount,
            decimal interestRate,
            DateTime startDate,
            DateTime? endDate = null,
            string notes = null
        ) : base(id)
        {
            BorrowerId = borrowerId;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            StartDate = startDate;
            EndDate = endDate;
            Notes = notes;
            LoanStatus = LoanStatus.Active;

            CalculateTotalAmount();

            Repayments = new List<LoanRepayment>();
        }

        public void CalculateTotalAmount()
        {
            TotalAmount = PrincipalAmount + (PrincipalAmount * InterestRate / 100);
            OutstandingBalance = TotalAmount;
        }

        public void ApplyRepayment(decimal amountPaid)
        {
            OutstandingBalance -= amountPaid;

            if (OutstandingBalance <= 0)
            {
                OutstandingBalance = 0;
                LoanStatus = LoanStatus.Completed;
                EndDate = DateTime.UtcNow;
            }
        }

        public void MarkAsDefaulted()
        {
            LoanStatus = LoanStatus.Defaulted;
        }
    }
}
