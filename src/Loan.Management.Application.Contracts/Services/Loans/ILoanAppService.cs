using Loan.Management.Services.Loans.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Loan.Management.Services.Loans
{
    public interface ILoanAppService : IApplicationService
    {
        Task<LoanDto> GetAsync(Guid id);

        Task<PagedResultDto<LoanDto>> GetListAsync(GetLoansInput input);

        Task<LoanDto> CreateAsync(CreateLoanDto input);

        Task<LoanDto> UpdateAsync(Guid id, UpdateLoanDto input);

        Task DeleteAsync(Guid id);

        Task<PagedResultDto<OverdueLoanDto>> GetOverdueLoansAsync(GetOverdueLoansInput input);

        Task<LoanDto> ApproveLoanAsync(Guid id);

        Task<LoanDto> CancelLoanAsync(Guid id);

        Task<LoanDto> WriteOffLoanAsync(Guid id);

        Task CheckAndUpdateOverdueLoansAsync();

        Task<LoanCalculationDto> CalculateLoanAsync(LoanCalculationInput input);
    }
}
