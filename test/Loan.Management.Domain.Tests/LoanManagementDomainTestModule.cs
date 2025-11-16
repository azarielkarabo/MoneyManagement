using Volo.Abp.Modularity;

namespace Loan.Management;

[DependsOn(
    typeof(LoanManagementDomainModule),
    typeof(LoanManagementTestBaseModule)
)]
public class LoanManagementDomainTestModule : AbpModule
{

}
