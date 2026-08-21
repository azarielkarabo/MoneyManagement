using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Payments.Dtos
{
    /// <summary>
    /// Same filters as <see cref="GetPaymentsInput"/> but without BorrowerId — used on
    /// endpoints where the borrower is already scoped by a route parameter, so a
    /// duplicate BorrowerId query parameter doesn't get generated in the API contract.
    /// </summary>
    public class GetBorrowerPaymentsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public Guid? CreditId { get; set; }
        public PaymentMethod? Method { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool GroupByBorrower { get; set; }
    }
}
