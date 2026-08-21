using System;
using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class BorrowerApplicationDto : FullAuditedEntityDto<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdNumber { get; set; }
        public IdentificationType IdType { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string? Message { get; set; }

        public BorrowerApplicationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? CreatedBorrowerId { get; set; }
    }
}
