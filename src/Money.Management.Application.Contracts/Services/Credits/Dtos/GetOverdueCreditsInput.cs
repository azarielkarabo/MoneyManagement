using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.credits.Dtos
{
    public class GetOverdueCreditsInput : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
        public OverdueSeverity? Severity { get; set; }
        public int? MinDaysOverdue { get; set; }
        public int? MaxDaysOverdue { get; set; }
    }
}
