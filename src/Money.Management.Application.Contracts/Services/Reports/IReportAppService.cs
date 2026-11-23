using Money.Management.Services.Reports.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Reports
{
    public interface IReportAppService : IApplicationService
    {
        /// <summary>GET /api/app/report/loan-report?fromDate&amp;toDate&amp;lenderId (lenderId: admins only).</summary>
        Task<LoanReportDto> GetLoanReportAsync(DateTime fromDate, DateTime toDate, Guid? lenderId = null);
        Task<BudgetReportDto> GetBudgetReportAsync(int year);
        Task<ReportSummaryDto> GetSummaryReportAsync();
    }
}
