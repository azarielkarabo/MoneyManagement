using Money.Management.Services.BorrowerApplications.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.BorrowerApplications
{
    public interface IBorrowerApplicationAppService : IApplicationService
    {
        Task<BorrowerApplicationDto> CreateAsync(CreateBorrowerApplicationDto input);
        Task<BorrowerApplicationDto> GetAsync(Guid id);
        Task<PagedResultDto<BorrowerApplicationDto>> GetListAsync(GetBorrowerApplicationsInput input);
        Task<ApproveBorrowerApplicationResultDto> ApproveAsync(Guid id, ApproveBorrowerApplicationDto input);
        Task RejectAsync(Guid id, RejectBorrowerApplicationDto input);
    }
}
