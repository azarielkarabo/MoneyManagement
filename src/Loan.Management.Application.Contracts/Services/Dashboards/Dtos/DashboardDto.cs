using System.Collections.Generic;

namespace Loan.Management.Services.Dashboards.Dtos
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; }
        public List<RecentTransactionDto> RecentTransactions { get; set; }
        public LoanStatusDistributionDto LoanDistribution { get; set; }
        public List<TopBorrowerDto> TopBorrowers { get; set; }
        public MoneyFlowDto MoneyFlow { get; set; }

        public DashboardDto()
        {
            Summary = new DashboardSummaryDto();
            RecentTransactions = new List<RecentTransactionDto>();
            LoanDistribution = new LoanStatusDistributionDto();
            TopBorrowers = new List<TopBorrowerDto>();
            MoneyFlow = new MoneyFlowDto();
        }
    }
}
