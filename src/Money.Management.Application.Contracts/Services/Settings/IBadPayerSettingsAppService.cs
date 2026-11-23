using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Settings
{
    public interface IBadPayerSettingsAppService : IApplicationService
    {
        Task<BadPayerSettingsDto> GetAsync();
        Task<BadPayerSettingsDto> UpdateAsync(BadPayerSettingsDto input);
    }
}
