using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Accounts.Dtos
{
    public class FinancialAccountDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public FinancialAccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }
}
