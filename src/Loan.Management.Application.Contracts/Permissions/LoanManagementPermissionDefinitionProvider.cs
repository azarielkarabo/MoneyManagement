using Loan.Management.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Loan.Management.Permissions;

public class LoanManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(LoanManagementPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(LoanManagementPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LoanManagementResource>(name);
    }
}
