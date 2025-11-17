using Loan.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Loan.Management.Services.Loans.Dtos
{
    public class GetLoansInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public Guid? BorrowerId { get; set; }
        public LoanStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? IsOverdue { get; set; }
    }
}
