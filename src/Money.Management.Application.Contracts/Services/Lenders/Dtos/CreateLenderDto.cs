using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Identity;

namespace Money.Management.Services.Lenders.Dtos
{
    // CreateLenderDto.cs
    public class CreateLenderDto
    {
        public IdentityUserCreateDto UserCreate { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxMoneyApprovalAmount { get; set; } = 0;
    }
}
