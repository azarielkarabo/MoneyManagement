using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.credits.Dtos
{
    public class CreditDto : FullAuditedEntityDto<Guid>
    {
        // Relationships
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public Guid LenderId { get; set; }
        public string? LenderName { get; set; }

        // Money Details
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }

        // Calculated Amounts
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment Tracking
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }

        // Dates
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }

        // Status & Details
        public CreditStatus Status { get; set; }
        public string Purpose { get; set; }
        public string Notes { get; set; }
        public RepaymentType RepaymentType { get; set; }

        // Computed Properties (calculated in AppService)
        public decimal PaymentProgress { get; set; } // Percentage
        public int DaysOverdue { get; set; }
    }
}