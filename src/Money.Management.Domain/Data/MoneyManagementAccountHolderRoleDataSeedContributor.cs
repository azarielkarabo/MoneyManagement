using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Money.Management.Data;

/// <summary>
/// Everyone who uses the system is an account holder: they manage their own money (accounts,
/// budgets, expenses), see their own loans and payments, and can request loans or apply to
/// become a lender. The "account-holder" role is ABP's default role, so every new user gets it,
/// and this seeder also adds it to existing users that don't have it yet.
///
/// Permission names are literals for the same reason as in
/// <see cref="MoneyManagementLenderRoleDataSeedContributor"/>.
/// </summary>
public class MoneyManagementAccountHolderRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string AccountHolderRoleName = "account-holder";
    private const string RoleProviderName = "R";
    private const string GroupName = "MoneyManagement";

    private static readonly string[] AccountHolderPermissionNames =
    {
        // Budgets and expenses (always scoped to the user's own data)
        $"{GroupName}.Budgets",
        $"{GroupName}.Budgets.Create",
        $"{GroupName}.Budgets.Edit",
        $"{GroupName}.Budgets.Delete",
        $"{GroupName}.Budgets.ManageItems",

        // Money accounts (always scoped to the user's own data)
        $"{GroupName}.Accounts",
        $"{GroupName}.Accounts.Create",
        $"{GroupName}.Accounts.Edit",
        $"{GroupName}.Accounts.Delete",
        $"{GroupName}.Accounts.Transfer",
    };

    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IGuidGenerator _guidGenerator;

    public MoneyManagementAccountHolderRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IPermissionDataSeeder permissionDataSeeder,
        IGuidGenerator guidGenerator)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _permissionDataSeeder = permissionDataSeeder;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(AccountHolderRoleName.ToUpperInvariant());

        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), AccountHolderRoleName, context?.TenantId)
            {
                IsDefault = true,
                IsPublic = true,
            };
            await _roleRepository.InsertAsync(role, autoSave: true);
        }
        else if (!role.IsDefault)
        {
            role.IsDefault = true;
            await _roleRepository.UpdateAsync(role, autoSave: true);
        }

        await _permissionDataSeeder.SeedAsync(
            RoleProviderName,
            role.Name,
            new List<string>(AccountHolderPermissionNames),
            context?.TenantId
        );

        // Everyone is an account holder: add the role to existing users that lack it.
        var users = await _userRepository.GetListAsync(includeDetails: true);
        foreach (var user in users.Where(u => u.Roles.All(r => r.RoleId != role.Id)))
        {
            await _userManager.AddToRoleAsync(user, AccountHolderRoleName);
        }
    }
}
