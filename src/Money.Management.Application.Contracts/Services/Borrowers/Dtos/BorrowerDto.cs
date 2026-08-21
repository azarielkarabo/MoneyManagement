using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class BorrowerDto : FullAuditedEntityDto<Guid>
    {
        public Guid LenderId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        public string IdNumber { get; set; }
        public IdentificationType IdType { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public decimal MonthlyIncome { get; set; }

        public BorrowerStatus Status { get; set; }

        // Financial Summary
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Paid { get; set; }
        public decimal DefaultAmount { get; set; }
        public int TotalMoneys { get; set; }
        public int DefaultedMoneys { get; set; }

        public string Notes { get; set; }

        // Computed (set by service)
        public int ActiveMoneysCount { get; set; }
        public int TotalMoneysCount { get; set; }
    }
}
