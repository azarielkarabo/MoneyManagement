using Money.Management.Services.Loans.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Loans
{
    public interface ILoanAppService : IApplicationService
    {
        // ── CRUD ──────────────────────────────────────────────────────────────────
        Task<LoanDto> GetLoanAsync(Guid id);
        Task<PagedResultDto<LoanDto>> GetLoansAsync(GetLoansInput input);
        Task<LoanDto> CreateLoanAsync(CreateLoanDto input);
        Task DeleteLoanAsync(Guid id);

        // ── Workflow ──────────────────────────────────────────────────────────────
        Task<LoanDto> ApproveLoanAsync(Guid id);
        Task<LoanDto> MarkAsDefaultedAsync(Guid id);

        // ── Repayments ────────────────────────────────────────────────────────────
        Task<LoanRepaymentDto> MakeRepaymentAsync(Guid loanId, MakeRepaymentDto input);
        Task<PagedResultDto<LoanRepaymentDto>> GetLoanRepaymentsAsync(Guid loanId);

        // ── Reports ───────────────────────────────────────────────────────────────
        Task<LoanSummaryReportDto> GetLoanSummaryReportAsync();

        // ── Calculations ──────────────────────────────────────────────────────────
        Task<LoanCalculationDto> CalculateLoanAsync(LoanCalculationInput input);
        Task<AmortizationScheduleDto> GetAmortizationScheduleAsync(AmortizationScheduleInput input);
    }
}
