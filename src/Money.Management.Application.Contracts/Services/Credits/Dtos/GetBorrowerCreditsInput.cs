using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.credits.Dtos
{
    /// <summary>
    /// Same filters as <see cref="GetCreditsInput"/> but without BorrowerId — used on
    /// endpoints where the borrower is already scoped by a route parameter, so a
    /// duplicate BorrowerId query parameter doesn't get generated in the API contract.
    /// </summary>
    public class GetBorrowerCreditsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public CreditStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool? IsOverdue { get; set; }
    }
}
