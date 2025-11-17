using Loan.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Loan.Management.Services.Loans.Dtos
{
    public class LoanDto : FullAuditedEntityDto<Guid>
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime DueDate { get; set; }
        public LoanStatus Status { get; set; }
        public string Description { get; set; }
        public int DaysOverdue { get; set; }
        public OverdueSeverity OverdueSeverity { get; set; }
        public decimal PaymentProgress { get; set; } // Percentage
    }
}
