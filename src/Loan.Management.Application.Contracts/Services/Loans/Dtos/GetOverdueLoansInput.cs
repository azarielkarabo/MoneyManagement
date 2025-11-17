using Loan.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Loan.Management.Services.Loans.Dtos
{
    public class GetOverdueLoansInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public OverdueSeverity? Severity { get; set; }
        public int? MinDaysOverdue { get; set; }
        public int? MaxDaysOverdue { get; set; }
    }
}
