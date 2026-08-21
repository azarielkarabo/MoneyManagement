using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class GetBorrowerApplicationsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public BorrowerApplicationStatus? Status { get; set; }
    }
}
