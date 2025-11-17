using Loan.Management.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Loan.Management.Entities
{
    public class Loan : FullAuditedAggregateRoot<Guid>
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }

        // Financial Details
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; } // Percentage
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; } // Principal + Interest
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Dates
        public DateTime DisbursementDate { get; set; }
        public DateTime DueDate { get; set; }

        // Status and Details
        public LoanStatus Status { get; set; }
        public string Description { get; set; }

        // Calculated field
        public int DaysOverdue
        {
            get
            {
                if (Status != LoanStatus.Overdue) return 0;
                return (DateTime.UtcNow.Date - DueDate.Date).Days;
            }
        }

        public OverdueSeverity OverdueSeverity
        {
            get
            {
                var days = DaysOverdue;
                if (days >= 60) return OverdueSeverity.Critical;
                if (days >= 30) return OverdueSeverity.High;
                if (days > 0) return OverdueSeverity.Medium;
                return OverdueSeverity.None;
            }
        }

        // Navigation properties
        public Borrower Borrower { get; set; }
        public ICollection<Payment> Payments { get; set; }

        protected Loan()
        {
            Payments = new List<Payment>();
        }

        public Loan(
            Guid id,
            Guid borrowerId,
            string borrowerName,
            decimal principalAmount,
            decimal interestRate,
            DateTime disbursementDate,
            DateTime dueDate,
            string description = null
        ) : base(id)
        {
            BorrowerId = borrowerId;
            BorrowerName = borrowerName;
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            DisbursementDate = disbursementDate;
            DueDate = dueDate;
            Description = description;
            Status = LoanStatus.Active;
            Payments = new List<Payment>();

            CalculateLoanAmounts();
        }

        public void CalculateLoanAmounts()
        {
            InterestAmount = PrincipalAmount * (InterestRate / 100);
            TotalAmount = PrincipalAmount + InterestAmount;
            OutstandingAmount = TotalAmount - AmountPaid;
        }

        public void RecordPayment(decimal paymentAmount)
        {
            AmountPaid += paymentAmount;
            OutstandingAmount = TotalAmount - AmountPaid;

            // Update status
            if (OutstandingAmount <= 0)
            {
                Status = LoanStatus.PaidOff;
                OutstandingAmount = 0;
            }
            else if (DateTime.UtcNow > DueDate)
            {
                Status = LoanStatus.Overdue;
            }
            else
            {
                Status = LoanStatus.Active;
            }
        }

        public void CheckOverdueStatus()
        {
            if (OutstandingAmount > 0 && DateTime.UtcNow.Date > DueDate.Date)
            {
                Status = LoanStatus.Overdue;
            }
        }
    }
}
