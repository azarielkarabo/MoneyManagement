using System;
using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class LenderApplicationDto : FullAuditedEntityDto<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DesiredUserName { get; set; }
        public string Position { get; set; }
        public string? Message { get; set; }

        public LenderApplicationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? CreatedLenderId { get; set; }
    }
}
