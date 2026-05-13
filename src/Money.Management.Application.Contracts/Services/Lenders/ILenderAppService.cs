using Money.Management.DomainServices;
using Money.Management.Services.Lenders.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Lenders
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

        // ── Profile management ────────────────────────────────────────────────────
        Task<LenderDto> UpdateProfileAsync(Guid id, UpdateLenderProfileDto input);
        Task<LenderDto> SetAvatarAsync(Guid id, string avatarUrl);
    }
}
