using Money.Management.Services.Accounts.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Accounts
{
    public interface IFinancialAccountAppService : IApplicationService
    {
        Task<FinancialAccountDto> GetAsync(Guid id);
        Task<PagedResultDto<FinancialAccountDto>> GetListAsync(GetFinancialAccountsInput input);
        Task<FinancialAccountDto> CreateAsync(CreateFinancialAccountDto input);
        Task<FinancialAccountDto> UpdateAsync(Guid id, UpdateFinancialAccountDto input);
        Task<FinancialAccountDto> DeactivateAsync(Guid id);
        Task<FinancialAccountDto> ActivateAsync(Guid id);
        Task DeleteAsync(Guid id);

        Task<AccountTransferDto> TransferAsync(TransferFundsDto input);
        Task<PagedResultDto<AccountTransferDto>> GetTransfersAsync(PagedAndSortedResultRequestDto input);
    }
}
