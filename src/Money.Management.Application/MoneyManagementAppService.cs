using Money.Management.Localization;
using Volo.Abp.Application.Services;

namespace Money.Management;

/* Inherit your application services from this class.
 */
public abstract class MoneyManagementAppService : ApplicationService
{
    protected MoneyManagementAppService()
    {
        LocalizationResource = typeof(MoneyManagementResource);
    }
}
