using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Money.Management.Permissions;
using Money.Management.Services.Settings;
using Money.Management.Settings;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Money.Management.Services
{
    // Admin-configurable rules for when a late borrower counts as a bad payer.
    public class BadPayerSettingsAppService : ApplicationService, IBadPayerSettingsAppService
    {
        private readonly ISettingManager _settingManager;

        public BadPayerSettingsAppService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<BadPayerSettingsDto> GetAsync()
        {
            return new BadPayerSettingsDto
            {
                GraceDays = await SettingProvider.GetAsync(
                    MoneyManagementSettings.BadPayers.GraceDays, MoneyManagementSettings.BadPayers.DefaultGraceDays),
                HighDays = await SettingProvider.GetAsync(
                    MoneyManagementSettings.BadPayers.HighDays, MoneyManagementSettings.BadPayers.DefaultHighDays),
                CriticalDays = await SettingProvider.GetAsync(
                    MoneyManagementSettings.BadPayers.CriticalDays, MoneyManagementSettings.BadPayers.DefaultCriticalDays),
            };
        }

        [Authorize(Roles = "admin")]
        public async Task<BadPayerSettingsDto> UpdateAsync(BadPayerSettingsDto input)
        {
            if (input.HighDays <= input.GraceDays)
            {
                throw new UserFriendlyException("The High period must be longer than the grace period.");
            }
            if (input.CriticalDays <= input.HighDays)
            {
                throw new UserFriendlyException("The Critical period must be longer than the High period.");
            }

            await _settingManager.SetGlobalAsync(MoneyManagementSettings.BadPayers.GraceDays, input.GraceDays.ToString());
            await _settingManager.SetGlobalAsync(MoneyManagementSettings.BadPayers.HighDays, input.HighDays.ToString());
            await _settingManager.SetGlobalAsync(MoneyManagementSettings.BadPayers.CriticalDays, input.CriticalDays.ToString());

            return await GetAsync();
        }
    }
}
