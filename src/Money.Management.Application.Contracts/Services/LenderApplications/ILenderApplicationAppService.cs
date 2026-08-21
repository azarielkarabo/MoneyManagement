using Money.Management.Services.LenderApplications.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.LenderApplications
{
    public interface ILenderApplicationAppService : IApplicationService
    {
        Task<LenderApplicationDto> CreateAsync(CreateLenderApplicationDto input);
        Task<LenderApplicationDto> GetAsync(Guid id);
        Task<PagedResultDto<LenderApplicationDto>> GetListAsync(GetLenderApplicationsInput input);
        Task<ApproveLenderApplicationResultDto> ApproveAsync(Guid id, ApproveLenderApplicationDto input);
        Task RejectAsync(Guid id, RejectLenderApplicationDto input);
    }
}
