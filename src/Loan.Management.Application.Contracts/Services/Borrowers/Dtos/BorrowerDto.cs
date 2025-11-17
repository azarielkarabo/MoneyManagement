using Loan.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace Loan.Management.Services.Borrowers.Dtos
{
    public class BorrowerDto : FullAuditedEntityDto<Guid>
    {
        public IdentityUserDto User { get; set; }
        public string Address { get; set; }
        public string IdNumber { get; set; }
        public BorrowerStatus Status { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Paid { get; set; }
        public int ActiveLoansCount { get; set; }
        public int TotalLoansCount { get; set; }
    }
}
