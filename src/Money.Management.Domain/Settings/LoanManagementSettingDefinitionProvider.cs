using Volo.Abp.Settings;

namespace Money.Management.Settings;

public class MoneyManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                MoneyManagementSettings.BadPayers.GraceDays,
                MoneyManagementSettings.BadPayers.DefaultGraceDays.ToString(),
                isVisibleToClients: true),
            new SettingDefinition(
                MoneyManagementSettings.BadPayers.HighDays,
                MoneyManagementSettings.BadPayers.DefaultHighDays.ToString(),
                isVisibleToClients: true),
            new SettingDefinition(
                MoneyManagementSettings.BadPayers.CriticalDays,
                MoneyManagementSettings.BadPayers.DefaultCriticalDays.ToString(),
                isVisibleToClients: true)
        );
    }
}
