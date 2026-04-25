using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class GetBorrowersInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public BorrowerStatus? Status { get; set; }
        public decimal? MinOutstanding { get; set; }
        public decimal? MaxOutstanding { get; set; }
    }
}
