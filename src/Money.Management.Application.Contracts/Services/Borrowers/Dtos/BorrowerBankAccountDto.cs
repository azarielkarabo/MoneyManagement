using Money.Management.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class BorrowerBankAccountDto : FullAuditedEntityDto<Guid>
    {
        public Guid BorrowerId { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public BankAccountType AccountType { get; set; }
        public string AccountHolderName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class AddBorrowerBankAccountDto
    {
        [Required]
        [StringLength(200)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string BranchCode { get; set; }

        [Required]
        [StringLength(30)]
        public string AccountNumber { get; set; }

        [Required]
        public BankAccountType AccountType { get; set; }

        [StringLength(200)]
        public string AccountHolderName { get; set; }

        public bool IsPrimary { get; set; }
    }
}
