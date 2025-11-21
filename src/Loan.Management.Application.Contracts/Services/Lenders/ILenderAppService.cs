// Loan.Management/Services/Lenders/ILenderAppService.cs
using Loan.Management.DomainServices;
using Loan.Management.Services.Lenders.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Loan.Management.Services.Lenders
{
    public interface ILenderAppService : IApplicationService
    {
        Task<LenderDto> GetAsync(Guid id);
        Task<LenderDto> GetCurrentLenderAsync();
        Task<PagedResultDto<LenderDto>> GetListAsync(GetLendersInput input);
        Task<LenderDto> CreateAsync(CreateLenderDto input);
        Task<LenderDto> UpdateAsync(Guid id, UpdateLenderDto input);
        Task DeleteAsync(Guid id);
        Task<LenderStatisticsDto> GetStatisticsAsync(Guid id);
        Task<LenderStatisticsDto> GetCurrentLenderStatisticsAsync();
    }
}