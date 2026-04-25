using Money.Management.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Money.Management.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MoneyManagementController : AbpControllerBase
{
    protected MoneyManagementController()
    {
        LocalizationResource = typeof(MoneyManagementResource);
    }
}
