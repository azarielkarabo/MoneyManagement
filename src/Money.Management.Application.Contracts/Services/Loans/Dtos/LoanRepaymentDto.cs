using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Loans.Dtos
{
    public class LoanRepaymentDto : FullAuditedEntityDto<Guid>
    {
        public Guid LoanId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; }
    }
}
