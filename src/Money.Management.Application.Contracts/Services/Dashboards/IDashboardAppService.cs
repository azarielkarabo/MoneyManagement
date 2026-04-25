using Money.Management.Services.Dashboards.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Dashboards
{
    public interface IDashboardAppService : IApplicationService
    {
        /// <summary>
        /// Get complete dashboard data with all metrics
        /// </summary>
        Task<DashboardDto> GetDashboardDataAsync();

        /// <summary>
        /// Get dashboard summary statistics
        /// </summary>
        Task<DashboardSummaryDto> GetSummaryAsync();

        /// <summary>
        /// Get credit status distribution
        /// </summary>
        Task<MoneyStatusDistributionDto> GetMoneyDistributionAsync();

        /// <summary>
        /// Get top borrowers by total borrowed amount
        /// </summary>
        Task<List<TopBorrowerDto>> GetTopBorrowersAsync(int count = 10);

        /// <summary>
        /// Get money flow analysis for specified months
        /// </summary>
        Task<MoneyFlowDto> GetMoneyFlowAsync(int months = 12);

        /// <summary>
        /// Get recent transactions (credits and payments)
        /// </summary>
        Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 10);

        /// <summary>
        /// Get collection metrics and performance
        /// </summary>
        Task<CollectionMetricsDto> GetCollectionMetricsAsync();

        /// <summary>
        /// Get upcoming payments/due credits
        /// </summary>
        Task<List<UpcomingPaymentDto>> GetUpcomingPaymentsAsync(int days = 30);

        /// <summary>
        /// Get borrower analytics
        /// </summary>
        Task<BorrowerAnalyticsDto> GetBorrowerAnalyticsAsync();

        /// <summary>
        /// Get portfolio health metrics
        /// </summary>
        Task<PortfolioHealthDto> GetPortfolioHealthAsync();

        /// <summary>
        /// Get credit aging analysis
        /// </summary>
        Task<MoneyAgingDto> GetMoneyAgingAsync();

        /// <summary>
        /// Get key performance indicators (KPIs)
        /// </summary>
        Task<Dictionary<string, decimal>> GetKpisAsync();

        /// <summary>
        /// Export dashboard data to Excel
        /// </summary>
        Task<byte[]> ExportDashboardDataAsync();
    }
}
