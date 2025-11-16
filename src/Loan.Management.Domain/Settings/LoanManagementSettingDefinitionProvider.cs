using Volo.Abp.Settings;

namespace Loan.Management.Settings;

public class LoanManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(LoanManagementSettings.MySetting1));
    }
}
