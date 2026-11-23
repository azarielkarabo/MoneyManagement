using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.LoanRequests
{
    public interface ILoanRequestAppService : IApplicationService
    {
        // Account holder side
        Task<List<LenderOptionDto>> GetLenderOptionsAsync();
        Task<LoanRequestDto> CreateAsync(CreateLoanRequestDto input);
        Task<PagedResultDto<LoanRequestDto>> GetMyListAsync(GetLoanRequestsInput input);
        Task CancelAsync(Guid id);

        // Lender side
        Task<PagedResultDto<LoanRequestDto>> GetIncomingListAsync(GetLoanRequestsInput input);
        Task<LoanRequestDto> ApproveAsync(Guid id, ApproveLoanRequestDto input);
        Task<LoanRequestDto> RejectAsync(Guid id, RejectLoanRequestDto input);
    }
}
