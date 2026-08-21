using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Money.Management.Data;

/// <summary>
/// Ensures a "lender" ABP Identity role exists and grants it every MoneyManagement business
/// permission except the admin-only Lenders/LenderApplications management groups. Runs on every
/// DbMigrator pass so newly-added permissions get retroactively granted too.
///
/// Lives in the Domain project (not Application, where MoneyManagementPermissions is defined)
/// because Money.Management.DbMigratorModule only depends on the Domain/EntityFrameworkCore/
/// Application.Contracts modules — adding a dependency on the Application module pulls in
/// AbpHangfireModule, whose OnApplicationInitialization requires Hangfire storage that's only
/// configured in the HttpApi.Host process, breaking DbMigrator startup entirely. The permission
/// names below are therefore duplicated as literals (mirroring Money.Management.Permissions.
/// MoneyManagementPermissions) instead of referenced directly, to avoid a Domain -> Application.
/// Contracts -> Domain circular project reference.
/// </summary>
public class MoneyManagementLenderRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string LenderRoleName = "lender";
    private const string RoleProviderName = "R";
    private const string GroupName = "MoneyManagement";

    private static readonly string[] LenderPermissionNames =
    {
        // Borrowers
        $"{GroupName}.Borrowers",
        $"{GroupName}.Borrowers.Create",
        $"{GroupName}.Borrowers.Edit",
        $"{GroupName}.Borrowers.Delete",
        $"{GroupName}.Borrowers.ViewFinancials",

        // Credits (permission group name is lowercase "credits" — matches MoneyManagementPermissions.Credits.Default)
        $"{GroupName}.credits",
        $"{GroupName}.credits.Create",
        $"{GroupName}.credits.Edit",
        $"{GroupName}.credits.Delete",
        $"{GroupName}.credits.Approve",
        $"{GroupName}.credits.WriteOff",

        // Payments
        $"{GroupName}.Payments",
        $"{GroupName}.Payments.Create",
        $"{GroupName}.Payments.Edit",
        $"{GroupName}.Payments.Delete",
        $"{GroupName}.Payments.Approve",

        // Dashboard
        $"{GroupName}.Dashboard",
        $"{GroupName}.Dashboard.ViewFinancials",

        // Reports
        $"{GroupName}.Reports",
        $"{GroupName}.Reports.Generate",
        $"{GroupName}.Reports.Export",

        // Loans
        $"{GroupName}.Loans",
        $"{GroupName}.Loans.Create",
        $"{GroupName}.Loans.Edit",
        $"{GroupName}.Loans.Delete",
        $"{GroupName}.Loans.MakeRepayment",

        // Budgets
        $"{GroupName}.Budgets",
        $"{GroupName}.Budgets.Create",
        $"{GroupName}.Budgets.Edit",
        $"{GroupName}.Budgets.Delete",
        $"{GroupName}.Budgets.ManageItems",
    };

    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IGuidGenerator _guidGenerator;

    public MoneyManagementLenderRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IPermissionDataSeeder permissionDataSeeder,
        IGuidGenerator guidGenerator)
    {
        _roleRepository = roleRepository;
        _permissionDataSeeder = permissionDataSeeder;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(LenderRoleName.ToUpperInvariant());

        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), LenderRoleName, context?.TenantId)
            {
                IsDefault = false,
                IsPublic = false,
            };
            await _roleRepository.InsertAsync(role);
        }

        await _permissionDataSeeder.SeedAsync(
            RoleProviderName,
            role.Name,
            new List<string>(LenderPermissionNames),
            context?.TenantId
        );
    }
}
