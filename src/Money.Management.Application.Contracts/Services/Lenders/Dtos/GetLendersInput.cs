using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Lenders.Dtos
{
    public class GetLendersInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
