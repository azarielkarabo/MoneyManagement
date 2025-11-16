using Loan.Management.Localization;
using Volo.Abp.Application.Services;

namespace Loan.Management;

/* Inherit your application services from this class.
 */
public abstract class LoanManagementAppService : ApplicationService
{
    protected LoanManagementAppService()
    {
        LocalizationResource = typeof(LoanManagementResource);
    }
}
