using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.credits.Dtos
{
    // One top-up on a loan, for the loan's transaction history.
    public class CreditTopUpDto : EntityDto<Guid>
    {
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestAmount { get; set; }
        public DateTime TopUpDate { get; set; }
        public decimal? OutstandingAfter { get; set; }
        public string? Note { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
