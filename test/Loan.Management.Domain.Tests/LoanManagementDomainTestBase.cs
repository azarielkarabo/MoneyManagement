using Volo.Abp.Modularity;

namespace Loan.Management;

/* Inherit from this class for your domain layer tests. */
public abstract class LoanManagementDomainTestBase<TStartupModule> : LoanManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
