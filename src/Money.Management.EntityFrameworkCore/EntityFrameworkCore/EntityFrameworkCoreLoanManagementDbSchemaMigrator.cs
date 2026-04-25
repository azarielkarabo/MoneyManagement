using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Management.Data;
using Volo.Abp.DependencyInjection;

namespace Money.Management.EntityFrameworkCore;

public class EntityFrameworkCoreMoneyManagementDbSchemaMigrator
    : IMoneyManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreMoneyManagementDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the MoneyManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<MoneyManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
