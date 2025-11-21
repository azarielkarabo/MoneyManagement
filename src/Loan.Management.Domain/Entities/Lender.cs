using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Loan.Management.Entities
{
    public class Lender : FullAuditedAggregateRoot<Guid>
    {
        // Link to ABP Identity user
        public Guid UserId { get; set; }
        public IdentityUser User { get; set; }

        // Lender-specific operational settings
        public string Position { get; set; } // Loan Officer, Manager, etc.
        public decimal MaxLoanApprovalAmount { get; set; }

        // Statistics (consider moving to a separate LenderStatistics entity if these grow)
        public int TotalLoansCreated { get; set; }
        public int TotalBorrowersManaged { get; set; }

        protected Lender()
        {
        }

        public Lender(
            Guid id,
            Guid userId,
            string position,
            decimal maxLoanApprovalAmount = 0
        ) : base(id)
        {
            UserId = userId;
            Position = position;
            MaxLoanApprovalAmount = maxLoanApprovalAmount;
            TotalLoansCreated = 0;
            TotalBorrowersManaged = 0;
        }
    }
}
