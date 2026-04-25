using Money.Management.Services.credits.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.credits
{
    public interface ICreditAppService : IApplicationService
    {
        Task<CreditDto> GetAsync(Guid id);
        Task<PagedResultDto<CreditDto>> GetListAsync(GetCreditsInput input);
        Task<CreditDto> CreateAsync(CreateCreditDto input);
        Task<CreditDto> UpdateAsync(Guid id, UpdateCreditDto input);
        Task DeleteAsync(Guid id);
        Task<PagedResultDto<OverdueCreditDto>> GetOverdueMoneysAsync(GetOverdueCreditsInput input);
        Task<CreditDto> ApproveAsync(Guid id);
        Task<CreditDto> ActivateAsync(Guid id);
        Task<CreditDto> SuspendAsync(Guid id);
        Task CheckAndUpdateOverdueMoneysAsync();
        Task<CreditCalculationDto> CalculateMoneyAsync(CreditCalculationInput input);
    }
}