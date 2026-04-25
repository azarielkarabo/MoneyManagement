using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Money.Management.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class MoneyManagementDbContextFactory : IDesignTimeDbContextFactory<MoneyManagementDbContext>
{
    public MoneyManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        MoneyManagementEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<MoneyManagementDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new MoneyManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Money.Management.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
