using Microsoft.Extensions.Localization;
using Money.Management.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Money.Management;

[Dependency(ReplaceServices = true)]
public class MoneyManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<MoneyManagementResource> _localizer;

    public MoneyManagementBrandingProvider(IStringLocalizer<MoneyManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
