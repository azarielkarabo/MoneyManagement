using Money.Management.Services.Loans.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Loans
{
    public interface ILoanAppService : IApplicationService
    {
        Task<LoanDto> GetLoanAsync(Guid id);
        Task<PagedResultDto<LoanDto>> GetLoansAsync(GetLoansInput input);
        Task<LoanDto> CreateLoanAsync(CreateLoanDto input);
        Task<LoanRepaymentDto> MakeRepaymentAsync(Guid loanId, MakeRepaymentDto input);
        Task<PagedResultDto<LoanRepaymentDto>> GetLoanRepaymentsAsync(Guid loanId);
        Task<LoanSummaryReportDto> GetLoanSummaryReportAsync();
    }
}
