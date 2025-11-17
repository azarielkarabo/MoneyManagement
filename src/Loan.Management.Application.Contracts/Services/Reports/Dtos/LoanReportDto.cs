using System;
using System.Collections.Generic;

namespace Loan.Management.Services.Reports.Dtos
{
    public class LoanReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalLoans { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int NewBorrowers { get; set; }
        public List<LoansByStatusDto> LoansByStatus { get; set; }

        public LoanReportDto()
        {
            LoansByStatus = new List<LoansByStatusDto>();
        }

        public class LoansByStatusDto
        {
            public string Status { get; set; }
            public int Count { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
