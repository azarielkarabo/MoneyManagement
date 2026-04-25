using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Money.Management.Data;

/* This is used if database provider does't define
 * IOthenticertDbSchemaMigrator implementation.
 */
public class NullMoneyManagementDbSchemaMigrator : IMoneyManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
