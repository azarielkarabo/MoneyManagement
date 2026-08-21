using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Money.Management.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Money.Management.Data;

/// <summary>
/// Grants every permission under <see cref="MoneyManagementPermissions"/> to the "admin" role.
/// Runs on every DbMigrator pass (via MoneyManagementDbMigrationService) so that permissions
/// added after the admin role already exists still get granted — ABP's own identity seeder
/// only grants permissions that existed at the moment the role was first created.
/// </summary>
public class MoneyManagementPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string AdminRoleName = "admin";

    // Volo.Abp.PermissionManagement.Identity.RolePermissionManagementProvider.Name — this ABP
    // version (9.3.6) made permission providers injectable services with an instance "Name"
    // property instead of the old static "ProviderName" const, so there's nothing to reference
    // at compile time. Confirmed via reflection: RolePermissionManagementProvider.Name == "R".
    private const string RoleProviderName = "R";

    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IIdentityRoleRepository _roleRepository;

    public MoneyManagementPermissionDataSeedContributor(
        IPermissionDataSeeder permissionDataSeeder,
        IIdentityRoleRepository roleRepository)
    {
        _permissionDataSeeder = permissionDataSeeder;
        _roleRepository = roleRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var adminRole = await _roleRepository.FindByNormalizedNameAsync(AdminRoleName.ToUpperInvariant());
        if (adminRole == null)
        {
            return;
        }

        await _permissionDataSeeder.SeedAsync(
            RoleProviderName,
            adminRole.Name,
            GetAllPermissionNames(),
            context?.TenantId
        );
    }

    private static List<string> GetAllPermissionNames()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        var names = new List<string>();

        foreach (var nestedType in typeof(MoneyManagementPermissions).GetNestedTypes(flags))
        {
            names.AddRange(
                nestedType.GetFields(flags)
                    .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                    .Select(f => (string)f.GetRawConstantValue()!)
            );
        }

        return names;
    }
}
