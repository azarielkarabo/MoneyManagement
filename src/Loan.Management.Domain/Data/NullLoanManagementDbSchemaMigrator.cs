using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Loan.Management.Data;

/* This is used if database provider does't define
 * IOthenticertDbSchemaMigrator implementation.
 */
public class NullLoanManagementDbSchemaMigrator : ILoanManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
