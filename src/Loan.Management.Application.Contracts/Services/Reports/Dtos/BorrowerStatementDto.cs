using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Reports.Dtos
{
    public class BorrowerStatementDto
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime StatementDate { get; set; }

        public decimal TotalBorrowed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }

        public List<LoanSummaryDto> Loans { get; set; }
        public List<PaymentSummaryDto> Payments { get; set; }

        public BorrowerStatementDto()
        {
            Loans = new List<LoanSummaryDto>();
            Payments = new List<PaymentSummaryDto>();
        }
    }
}
