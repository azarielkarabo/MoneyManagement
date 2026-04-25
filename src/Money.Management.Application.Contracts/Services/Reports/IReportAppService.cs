using Money.Management.Services.Reports.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Reports
{
    public interface IReportAppService : IApplicationService
    {
        Task<LoanReportDto> GetLoanReportAsync(DateTime fromDate, DateTime toDate);
        Task<BudgetReportDto> GetBudgetReportAsync(int year);
        Task<ReportSummaryDto> GetSummaryReportAsync();
    }
}
