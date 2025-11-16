using Volo.Abp.Modularity;

namespace Loan.Management;

[DependsOn(
    typeof(LoanManagementApplicationModule),
    typeof(LoanManagementDomainTestModule)
)]
public class LoanManagementApplicationTestModule : AbpModule
{

}
