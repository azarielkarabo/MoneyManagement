using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Payments.Dtos
{
    public class GetPaymentsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public Guid? BorrowerId { get; set; }
        public Guid? CreditId { get; set; }
        public PaymentMethod? Method { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool GroupByBorrower { get; set; }
    }
}
