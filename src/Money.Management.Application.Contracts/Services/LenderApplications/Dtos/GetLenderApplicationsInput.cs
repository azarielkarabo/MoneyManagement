using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class GetLenderApplicationsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public LenderApplicationStatus? Status { get; set; }
    }
}
