using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan.Management.Services.Dashboards.Dtos
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; }
        public List<RecentTransactionDto> RecentTransactions { get; set; }
        public LoanStatusDistributionDto LoanDistribution { get; set; }
        public List<TopBorrowerDto> TopBorrowers { get; set; }
        public MoneyFlowDto MoneyFlow { get; set; }
        public CollectionMetricsDto CollectionMetrics { get; set; }
        public List<UpcomingPaymentDto> UpcomingPayments { get; set; }

        public DashboardDto()
        {
            Summary = new DashboardSummaryDto();
            RecentTransactions = new List<RecentTransactionDto>();
            LoanDistribution = new LoanStatusDistributionDto();
            TopBorrowers = new List<TopBorrowerDto>();
            MoneyFlow = new MoneyFlowDto();
            CollectionMetrics = new CollectionMetricsDto();
            UpcomingPayments = new List<UpcomingPaymentDto>();
        }
    }
}