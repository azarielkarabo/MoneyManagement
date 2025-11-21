using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace Loan.Management.Services.Lenders.Dtos
{
    // UpdateLenderDto.cs
    public class UpdateLenderDto: EntityDto<Guid>
    {
        public IdentityUserUpdateDto UserUpdate { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxLoanApprovalAmount { get; set; }
    }
}
