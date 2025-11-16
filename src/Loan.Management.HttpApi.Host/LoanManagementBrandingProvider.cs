using Microsoft.Extensions.Localization;
using Loan.Management.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Loan.Management;

[Dependency(ReplaceServices = true)]
public class LoanManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<LoanManagementResource> _localizer;

    public LoanManagementBrandingProvider(IStringLocalizer<LoanManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
