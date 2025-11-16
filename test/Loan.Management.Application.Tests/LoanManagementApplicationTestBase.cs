using Volo.Abp.Modularity;

namespace Loan.Management;

public abstract class LoanManagementApplicationTestBase<TStartupModule> : LoanManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
