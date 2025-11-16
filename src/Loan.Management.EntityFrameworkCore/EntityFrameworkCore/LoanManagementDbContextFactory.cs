using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Loan.Management.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class LoanManagementDbContextFactory : IDesignTimeDbContextFactory<LoanManagementDbContext>
{
    public LoanManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        LoanManagementEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<LoanManagementDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new LoanManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Loan.Management.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
