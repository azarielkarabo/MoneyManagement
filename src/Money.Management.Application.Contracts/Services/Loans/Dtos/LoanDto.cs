using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Loans.Dtos
{
    public class LoanDto : FullAuditedEntityDto<Guid>
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }

        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalPaid { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public LoanStatus LoanStatus { get; set; }
        public string Notes { get; set; }

        public decimal RepaymentProgress { get; set; }
    }
}
