using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Loans.Dtos
{
    public class GetLoansInput : PagedAndSortedResultRequestDto
    {
        public Guid? BorrowerId { get; set; }
        public LoanStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
