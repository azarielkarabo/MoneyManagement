using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.credits.Dtos
{
    public class OverdueCreditDto : EntityDto<Guid>
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public OverdueSeverity Severity { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public decimal? LastPaymentAmount { get; set; }
        public decimal PaymentProgress { get; set; }
    }
}
