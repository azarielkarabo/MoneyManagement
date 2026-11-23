using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Registration
{
    public interface IRegistrationAppService : IApplicationService
    {
        Task RegisterAsync(RegisterAccountHolderDto input);
    }
}
