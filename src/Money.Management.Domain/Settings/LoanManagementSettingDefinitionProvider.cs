using Volo.Abp.Settings;

namespace Money.Management.Settings;

public class MoneyManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MoneyManagementSettings.MySetting1));
    }
}
