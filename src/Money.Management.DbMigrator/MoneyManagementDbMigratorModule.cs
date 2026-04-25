using Money.Management.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Money.Management.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MoneyManagementEntityFrameworkCoreModule),
    typeof(MoneyManagementApplicationContractsModule)
)]
public class MoneyManagementDbMigratorModule : AbpModule
{
}
