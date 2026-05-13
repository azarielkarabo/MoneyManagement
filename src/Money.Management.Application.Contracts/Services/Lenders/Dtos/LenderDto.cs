using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace Money.Management.Services.Lenders.Dtos
{
    public class LenderDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public IdentityUserDto User { get; set; }

        // Professional profile
        public string Position { get; set; }
        public string Department { get; set; }
        public string Branch { get; set; }
        public string EmployeeId { get; set; }
        public string DirectPhone { get; set; }
        public string OfficeLocation { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }

        // Operational settings
        public decimal MaxMoneyApprovalAmount { get; set; }

        // Performance metrics
        public decimal CollectionRate { get; set; }
        public decimal ApprovalRate { get; set; }

        // Statistics
        public int TotalMoneysCreated { get; set; }
        public int TotalBorrowersManaged { get; set; }
    }
}
