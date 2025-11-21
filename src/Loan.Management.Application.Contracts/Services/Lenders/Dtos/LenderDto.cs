using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace Loan.Management.Services.Lenders.Dtos
{
    // LenderDto.cs
    public class LenderDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public IdentityUserDto User { get; set; }
        public string Position { get; set; }
        public decimal MaxLoanApprovalAmount { get; set; }
        public int TotalLoansCreated { get; set; }
        public int TotalBorrowersManaged { get; set; }
    }
}
