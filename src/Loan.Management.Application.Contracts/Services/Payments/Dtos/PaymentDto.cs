using Loan.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Loan.Management.Services.Payments.Dtos
{
    public class PaymentDto : FullAuditedEntityDto<Guid>
    {
        public Guid LoanId { get; set; }
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PreviousOutstanding { get; set; }
        public decimal NewOutstanding { get; set; }
        public string Description { get; set; }
        public PaymentMethod Method { get; set; }
    }
}
