using Loan.Management.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Loan.Management.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class LoanManagementController : AbpControllerBase
{
    protected LoanManagementController()
    {
        LocalizationResource = typeof(LoanManagementResource);
    }
}
