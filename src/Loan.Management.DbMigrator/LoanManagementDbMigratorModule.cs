using Loan.Management.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Loan.Management.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(LoanManagementEntityFrameworkCoreModule),
    typeof(LoanManagementApplicationContractsModule)
)]
public class LoanManagementDbMigratorModule : AbpModule
{
}
