using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Accounts.Dtos
{
    public class AccountTransferDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public Guid FromAccountId { get; set; }
        public string FromAccountName { get; set; }
        public Guid ToAccountId { get; set; }
        public string ToAccountName { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
