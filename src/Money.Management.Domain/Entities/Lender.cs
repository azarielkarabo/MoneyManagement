using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Money.Management.Entities
{
    public class Lender : FullAuditedAggregateRoot<Guid>
    {
        // Link to ABP Identity user
        public Guid UserId { get; set; }
        public IdentityUser User { get; set; }

        // Lender-specific operational settings
        public string Position { get; set; } // Money Officer, Manager, etc.
        public decimal MaxMoneyApprovalAmount { get; set; }

        // Statistics (consider moving to a separate LenderStatistics entity if these grow)
        public int TotalMoneysCreated { get; set; }
        public int TotalBorrowersManaged { get; set; }

        protected Lender()
        {
        }

        public Lender(
            Guid id,
            Guid userId,
            string position,
            decimal maxMoneyApprovalAmount = 0
        ) : base(id)
        {
            UserId = userId;
            Position = position;
            MaxMoneyApprovalAmount = maxMoneyApprovalAmount;
            TotalMoneysCreated = 0;
            TotalBorrowersManaged = 0;
        }
    }
}
