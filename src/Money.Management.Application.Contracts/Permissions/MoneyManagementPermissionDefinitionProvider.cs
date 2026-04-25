using Money.Management.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Money.Management.Permissions;

public class MoneyManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MoneyManagementPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(MoneyManagementPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MoneyManagementResource>(name);
    }
}
