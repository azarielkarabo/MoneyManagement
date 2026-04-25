using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    public class LoanRepayment : FullAuditedEntity<Guid>
    {
        public Guid LoanId { get; set; }
        public Loan Loan { get; set; }

        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }

        public string Notes { get; set; }

        protected LoanRepayment()
        {
        }

        public LoanRepayment(
            Guid id,
            Guid loanId,
            decimal amountPaid,
            DateTime paymentDate,
            string notes = null
        ) : base(id)
        {
            LoanId = loanId;
            AmountPaid = amountPaid;
            PaymentDate = paymentDate;
            Notes = notes;
        }
    }
}
