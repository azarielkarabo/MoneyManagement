using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Loan.Management.Data;
using Volo.Abp.DependencyInjection;

namespace Loan.Management.EntityFrameworkCore;

public class EntityFrameworkCoreLoanManagementDbSchemaMigrator
    : ILoanManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreLoanManagementDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the LoanManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<LoanManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
